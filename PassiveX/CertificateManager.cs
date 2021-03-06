﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace PassiveX
{
    internal static partial class CertificateManager
    {
        internal static Dictionary<X509Certificate2, EncryptedPrivateKeyInfo> GetListFromDisk(IEnumerable<string> oids)
        {
            string[] paths;

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    paths = NpkiDiskPathOnWindows;
                    break;
                case PlatformID.Unix:
                    paths = NpkiDiskPathOnLinux;
                    break;
                case PlatformID.MacOSX:
                    paths = NpkiDiskPathOnMac;
                    break;
                default:
                    paths = new string[0];
                    break;
            }

            var collection = new X509Certificate2Collection();
            var pathMap = new Dictionary<X509Certificate2, DirectoryInfo>();
            foreach (var directoryPath in paths)
            {
                var directoryInfo = new DirectoryInfo(Environment.ExpandEnvironmentVariables(directoryPath));
                if (!directoryInfo.Exists)
                {
                    continue;
                }

                foreach (var fileInfo in directoryInfo.EnumerateFiles("*.der", SearchOption.AllDirectories))
                {
                    var certificate = new X509Certificate2(fileInfo.FullName);
                    pathMap[certificate] = fileInfo.Directory;
                    collection.Add(certificate);
                }
            }

            collection = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

            var certificates = new X509Certificate2Collection();
            foreach (var oid in oids)
            {
                certificates.AddRange(collection.Find(X509FindType.FindByCertificatePolicy, oid, false));
            }

            var list = new Dictionary<X509Certificate2, EncryptedPrivateKeyInfo>();
            foreach (var certificate in certificates)
            {
                foreach (var fileInfo in pathMap[certificate].EnumerateFiles("*.key", SearchOption.TopDirectoryOnly))
                {
                    var data = File.ReadAllBytes(fileInfo.FullName);
                    list[certificate] = EncryptedPrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(data));
                    break;
                }
            }

            return list;
        }

        internal static RSA DecryptPrivateKey(EncryptedPrivateKeyInfo encryptedPrivateKeyInfo, string password)
        {
            var parameters = (DerSequence)encryptedPrivateKeyInfo.EncryptionAlgorithm.Parameters;
            var salt = ((DerOctetString)parameters[0]).GetOctets();
            var iterations = ((DerInteger)parameters[1]).Value.IntValue;

            var pbkdf1 = new PasswordDeriveBytes(password, salt, "SHA1", iterations);
            var keyBytes = pbkdf1.GetBytes(16);
            var ivBytes = SHA1.Create().ComputeHash(pbkdf1.GetBytes(4));

            var engine = new SeedEngine();
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher);
            var cipherParams = new ParametersWithIV(new KeyParameter(keyBytes), ivBytes, 0, 16);

            try
            {
                cipher.Init(false, cipherParams);
                var decoded = cipher.DoFinal(encryptedPrivateKeyInfo.GetEncryptedData());

                var rsaParams = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(decoded);
                return DotNetUtilities.ToRSA(rsaParams);
            }
            catch (InvalidCipherTextException)
            {
                return null;
            }
        }
    }
}

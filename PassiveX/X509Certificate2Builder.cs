using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace PassiveX
{
    internal static class X509Certificate2Builder
    {
        private const int KeyStrength = 2048;
        private static char[] _emptyPassword;
        private static string _rootCertPfxPath;
        private static Dictionary<string, X509Certificate2> _cachedCertificates;

        internal static void Initialize(string rootCertPfxPath)
        {
            _rootCertPfxPath = rootCertPfxPath;
            _emptyPassword = new char[0];
            _cachedCertificates = new Dictionary<string, X509Certificate2>();
        }

        internal static X509Certificate2 Build(string hostname)
        {
            if (_cachedCertificates.TryGetValue(hostname, out var cachedCert))
            {
                return cachedCert;
            }

            var random = new SecureRandom();

            AsymmetricKeyParameter rootCertPrivKey = null;
            Org.BouncyCastle.X509.X509Certificate rootCert = null;
            using (var stream = new FileStream(_rootCertPfxPath, FileMode.Open))
            {
                var store = new Pkcs12Store(stream, _emptyPassword);
                foreach (string alias in store.Aliases)
                {
                    if (store.IsKeyEntry(alias))
                    {
                        rootCert = store.GetCertificate(alias).Certificate;
                        rootCertPrivKey = store.GetKey(alias).Key;
                        break;
                    }
                }
            }

            if (rootCertPrivKey == null)
            {
                return null;
            }

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(new KeyGenerationParameters(random, KeyStrength));
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.SetIssuerDN(rootCert.IssuerDN);
            certificateGenerator.SetSubjectDN(new X509Name($"CN={hostname}"));

            var notBefore = DateTime.Now.Date;
            var notAfter = notBefore.AddYears(1);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", rootCertPrivKey, random);

            var x509 = certificateGenerator.Generate(signatureFactory);

            using (var stream = new MemoryStream())
            {
                var store = new Pkcs12Store();
                store.SetKeyEntry(Guid.NewGuid().ToString(), new AsymmetricKeyEntry(subjectKeyPair.Private),
                    new[] { new X509CertificateEntry(x509) });
                store.Save(stream, _emptyPassword, random);

                var cert = new X509Certificate2(stream.ToArray());
                _cachedCertificates.Add(hostname, cert);

                return cert;
            }
        }

        internal static void Install()
        {
            var cert = new X509Certificate2(_rootCertPfxPath);
            cert.FriendlyName = "Ministry of Cats and Kittens";

            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
            }
        }
    }
}

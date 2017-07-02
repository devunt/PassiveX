using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using X509Extension = Org.BouncyCastle.Asn1.X509.X509Extension;

namespace PassiveX
{
    internal static class CertificateBuilder
    {
        private const int KeyStrength = 2048;

        private static X509Certificate2 _rootCertificate;
        private static AsymmetricKeyParameter _rootCertificatePrivateKey;
        private static Dictionary<string, X509Certificate2> _cachedCertificates;

        internal static void Initialize(X509Certificate2 rootCertificate)
        {
            _rootCertificate = rootCertificate;
            _rootCertificatePrivateKey = DotNetUtilities.GetKeyPair(_rootCertificate.PrivateKey).Private;
            _cachedCertificates = new Dictionary<string, X509Certificate2>();
        }

        internal static X509Certificate2 Build(string hostname)
        {
            if (_cachedCertificates.TryGetValue(hostname, out var cachedCert))
            {
                return cachedCert;
            }

            var random = new SecureRandom();

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(new KeyGenerationParameters(random, KeyStrength));
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

            certificateGenerator.SetIssuerDN(new X509Name(_rootCertificate.Issuer));
            certificateGenerator.SetSubjectDN(new X509Name($"CN={hostname}"));

            var notBefore = DateTime.Now.Date;
            var notAfter = notBefore.AddYears(1);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", _rootCertificatePrivateKey, random);

            var x509 = certificateGenerator.Generate(signatureFactory);

            var certificate = new X509Certificate2(x509.GetEncoded());
            certificate.PrivateKey = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)subjectKeyPair.Private);

            _cachedCertificates[hostname] = certificate;

            return certificate;
        }

        internal static void Install()
        {
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(_rootCertificate);
            store.Close();
        }
    }
}

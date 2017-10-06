using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace PassiveX
{
    internal static class CertificateBuilder
    {
        private const int KeyStrength = 2048;

        private static X509Certificate2 _rootCertificate;
        private static AsymmetricCipherKeyPair _rootCertificateKeyPair;
        private static Dictionary<string, X509Certificate2> _cachedCertificates;

        internal static void Initialize(X509Certificate2 rootCertificate)
        {
            _rootCertificate = rootCertificate;
            _rootCertificateKeyPair = DotNetUtilities.GetKeyPair(_rootCertificate.PrivateKey);
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

            var subjectAltName = new GeneralNames(new[] {
                new GeneralName(GeneralName.IPAddress, "127.0.0.1"),
                new GeneralName(GeneralName.DnsName, "localhost"),
            });

            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigInteger.ProbablePrime(120, random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));
            certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, subjectAltName);
            certificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                new SubjectKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public)));
            certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_rootCertificateKeyPair.Public)));
            certificateGenerator.AddExtension(X509Extensions.CertificatePolicies, false,
                new CertificatePolicies(
                    new PolicyInformation(new DerObjectIdentifier("1.2.410.200004.2"), new DerSequence(
                        new PolicyQualifierInfo("https://github.com/devunt/PassiveX"),
                        new PolicyQualifierInfo(PolicyQualifierID.IdQtUnotice,
                            new UserNotice(null, "PassiveX 가 자동으로 생성한 가짜 인증서입니다."))))));

            certificateGenerator.SetIssuerDN(new X509Name(_rootCertificate.Issuer));
            certificateGenerator.SetSubjectDN(new X509Name($"CN={hostname}"));

            var notBefore = DateTime.UtcNow.Date;
            var notAfter = notBefore.AddYears(1);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", _rootCertificateKeyPair.Private, random);

            var x509 = certificateGenerator.Generate(signatureFactory);

            var certificate = new X509Certificate2(x509.GetEncoded());
            certificate.PrivateKey = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)subjectKeyPair.Private);

            _cachedCertificates[hostname] = certificate;

            return certificate;
        }

        internal static X509Certificate2 BuildRootCertificate()
        {
            var name = "!!! PassiveX Root CA !!!";

            var random = new SecureRandom();

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(new KeyGenerationParameters(random, KeyStrength));
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

            var certificateGenerator = new X509V3CertificateGenerator();

            var serialNumber = BigInteger.ProbablePrime(120, random);
            certificateGenerator.SetSerialNumber(serialNumber);

            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true,
                new KeyUsage(KeyUsage.KeyCertSign | KeyUsage.CrlSign));
            certificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                new SubjectKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public)));
            certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(subjectKeyPair.Public)));

            certificateGenerator.SetIssuerDN(new X509Name($"CN={name}"));
            certificateGenerator.SetSubjectDN(new X509Name($"CN={name}"));

            var notBefore = new DateTime(2000, 01, 01);
            var notAfter = new DateTime(2039, 12, 31);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", subjectKeyPair.Private, random);

            var x509 = certificateGenerator.Generate(signatureFactory);

            var certificate = new X509Certificate2(x509.GetEncoded());
            certificate.PrivateKey = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)subjectKeyPair.Private);
            certificate.FriendlyName = name;

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

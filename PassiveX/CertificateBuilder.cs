using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace PassiveX
{
    internal static class CertificateBuilder
    {
        private static X509Certificate2 _rootCertificate;
        private static Dictionary<string, X509Certificate2> _cachedCertificates;

        internal static void Initialize(X509Certificate2 rootCertificate)
        {
            _rootCertificate = rootCertificate;
            _cachedCertificates = new Dictionary<string, X509Certificate2>();
        }

        internal static X509Certificate2 Build(string hostname)
        {
            if (_cachedCertificates.TryGetValue(hostname, out var cachedCert))
            {
                return cachedCert;
            }

            using (var rsa = RSA.Create())
            {
                var request = new CertificateRequest($"CN={hostname}", rsa, HashAlgorithmName.SHA256);

                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                sanBuilder.AddDnsName(hostname);

                request.CertificateExtensions.Add(sanBuilder.Build());
                request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                var serialNumber = new byte[8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(serialNumber);
                }

                var now = DateTimeOffset.UtcNow;
                var intermidiateCertificate = request.Create(_rootCertificate, now, now.AddYears(1), serialNumber);
                var certificate = intermidiateCertificate.CopyWithPrivateKey(rsa);

                // https://github.com/dotnet/corefx/issues/19888
                certificate = new X509Certificate2(certificate.Export(X509ContentType.Pkcs12));

                _cachedCertificates[hostname] = certificate;

                return certificate;
            }
        }

        internal static void Install()
        {
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(_rootCertificate);
            }
        }
    }
}

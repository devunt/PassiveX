using Newtonsoft.Json;
using PassiveX.Transports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto.Operators;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 4441)]
    internal class CrossWebHandler : IHttpHandler
    {
        private static readonly Dictionary<string, string> OidMap = new Dictionary<string, string>
        {
            // 금융결제원 (Yessign)
            { "a1", "1.2.410.200005.1.1.1" },   // 범용 (개인)
            { "a2", "1.2.410.200005.1.1.5" },   // 범용 (기업)
            { "a4", "1.2.410.200005.1.1.4" },   // 은행/보험 (개인)
            { "a5", "1.2.410.200005.1.1.2" },   // 은행/보험 (기업)
            { "a6", "1.2.410.200005.1.1.6.2" }, // 신용카드

            // 한국무역정보통신 (TradeSign)
            { "b1", "1.2.410.200012.1.1.1" },   // 범용 (개인)
            { "b2", "1.2.410.200012.1.1.3" },   // 범용 (법인)
            { "b4", "1.2.410.200012.1.1.101" }, // 은행/보험용
            { "b5", "1.2.410.200012.1.1.105" }, // 신용카드용

            // 한국증권전산 (SignKorea)
            { "c1", "1.2.410.200004.5.1.1.5" },   // 범용 (개인)
            { "c2", "1.2.410.200004.5.1.1.7" },   // 범용 (법인)
            { "c4", "1.2.410.200004.5.1.1.9.2" }, // 신용카드용

            // 한국정보인증 (SignGate)
            { "d1", "1.2.410.200004.5.2.1.2" },   // 1등급 (개인)
            { "d2", "1.2.410.200004.5.2.1.1" },   // 1등급 (법인)
            { "d4", "1.2.410.200004.5.2.1.7.1" }, // 은행/보험용
            { "d5", "1.2.410.200004.5.2.1.7.3" }, // 신용카드용

            // 한국전자인증 (CrossCert)
            { "e1", "1.2.410.200004.5.4.1.1" },   // 범용 (개인)
            { "e2", "1.2.410.200004.5.4.1.2" },   // 범용 (법인)
            { "e4", "1.2.410.200004.5.4.1.101" }, // 은행/보험용
            { "e5", "1.2.410.200004.5.4.1.103" }, // 신용카드용

            // 한국전산원 (NCA)
            { "f1", "1.2.410.200004.5.3.1.1" },   // 범용 (기관)
            { "f2", "1.2.410.200004.5.3.1.2" },   // 범용 (법인)
            { "f3", "1.2.410.200004.5.3.1.9" },   // 범용 (개인)
        };

        private static readonly Dictionary<string, string> Property = new Dictionary<string, string>();

        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            if (!request.Parameters.TryGetValue("request", out var requestField))
            {
                return Task.FromResult(response);
            }

            dynamic data = JsonConvert.DeserializeObject(requestField);

            dynamic result = null;
            dynamic reply = null;
            var callback = data.callback;

            if (data.init != null)
            {
                result = new
                {
                    response = new
                    {
                        id = data.id,
                        status = "TRUE",
                    },
                    daemon = "9.9.9.9",
                    ex = "9.9.9.9",
                    m = new { name = data.m, version = "9.9.9.9" },
                };
            }
            else if (data.cmd != null)
            {
                switch ((string)data.cmd)
                {
                    case "native":
                        dynamic args = data.exfunc.args;
                        switch ((string)data.exfunc.fname)
                        {
                            case "InstallModule":
                                reply = "1";
                                break;

                            case "DisableInvalidCert":
                            case "SetVerifyNegoTime":
                            case "setSharedAttribute":
                                reply = "";
                                break;

                            case "LoadCACert":
                            case "LoadCert":
                            case "SetLogoPath":
                            case "SetProperty":
                                reply = "TRUE";
                                break;

                            case "SetPropertyEX":
                                foreach (var entry in args[0])
                                {
                                    Property[(string)entry[0]] = (string)entry[1];
                                }
                                reply = "TRUE";
                                break;
                                
                            case "ExtendMethod":
                                reply = "ok";
                                break;

                            case "CWEXRequestCmd":
                                var param = JsonConvert.DeserializeObject(Uri.UnescapeDataString((string)args[0]));
                                reply = Uri.EscapeDataString(JsonConvert.SerializeObject(CwExRequestCmd(param)));
                                break;

                            default:
                                Log.W($"Unknown native call: {data.exfunc.fname}");
                                break;
                        }
                        break;

                    default:
                        Log.W($"Unknown command: {data.cmd}");
                        break;
                }

                result = new
                {
                    response = new
                    {
                        id = data.id,
                        tabid = data.tabid,
                        status = "TRUE",
                        reply = new { reply = new { status = "0", reply = reply } },
                        callback = callback ?? "",
                    }
                };
            }
            response.SetJson(result);

            return Task.FromResult(response);
        }

        private dynamic CwExRequestCmd(dynamic data)
        {
            dynamic result = null;
            switch ((string)data.COMMAND)
            {
                case "GET_VERSION":
                    result = new
                    {
                        STATE = "SUCCEEDED",
                        VERSION_LIST = new []
                        {
                            new
                            {
                                TYPE = "1",
                                MODULE_NAME = Uri.EscapeDataString("INISAFE CrossWeb EX"),
                                VERSION = Uri.EscapeDataString("9.9.9.9"),
                            }
                        },
                    };
                    break;

                case "GET_PUBLIC_KEY":
                    /*
                    result = new
                    {
                        STATE = "SUCCEEDED",
                        PUBLIC_KEY = Uri.EscapeDataString(File.ReadAllText("Resources/CW.pem")),
                    };
                    */
                   break;

                case "GET_PUBLIC_KEY_FOR_KEYBOARD_SECURITY":
                    /*result = new
                    {
                        STATE = "SUCCEEDED",
                        PUBLIC_KEY = Uri.EscapeDataString(File.ReadAllText("Resources/CWKeyboardSecurity.pem")),
                    };*/
                    break;

                case "GET_CERT_LIST":
                    result = new
                    {
                        STATE = "SUCCEEDED",
                        CERT_LIST = GetCertificateList((string)data.PARAMS.DEVICE_ID),
                    };
                    break;

                case "DIGITAL_SIGN":
                    // Log.D(data.PARAMS);
                    var originalData = (string)data.PARAMS.ORIGINAL_DATA;
                    if (data.PARAMS.ORIGINAL_URL_ENCODING == "TRUE")
                    {
                        originalData = Uri.UnescapeDataString(originalData);
                    }

                    var sessionData = DigitalSign(
                        (string)data.PARAMS.DEVICE_ID,
                        Uri.UnescapeDataString((string)data.PARAMS.CERT_ID),
                        Uri.UnescapeDataString((string)data.PARAMS.PASSWORD),
                        Encoding.UTF8.GetBytes(originalData)
                    );

                    if (sessionData == null)
                    {
                        result = new
                        {
                            STATE = "FAILED",
                            CODE = "0608",
                            MSG = Uri.EscapeDataString("Invalid password"),
                        };
                    }
                    else
                    {
                        result = new
                        {
                            STATE = "SUCCEEDED",
                            SESSION_DATA = sessionData,
                        };
                    }

                    break;

                default:
                    Log.W($"Unknown command: {data.COMMAND}");
                    Log.D(data);
                    break;
            }

            return new
            {
                data.PROTOCOLVER,
                data.PRODUCTID,
                data.DOMAIN,
                data.SESSIONID,
                data.SESSION,
                data.ENCRYPTED,
                data.COMMAND,
                data.TRANS_SEQ,
                data.GET_RESULT,
                PARAMS = result,
            };
        }

        private string DigitalSign(string location, string id, string password, byte[] data)
        {
            foreach (var certificatePair in GetCertificates(location))
            {
                var certId = GetCertificateId(certificatePair.Key);
                if (certId == id)
                {
                    // certificate.GetRSAPublicKey().SignData(new byte[0], System.Security.Cryptography.HashAlgorithmName.SHA1, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
                    // var generator = new CmsSignedDataGenerator();
                    // var a = new SignerInfoGeneratorBuilder();
                    // a.Build(X509SignatureGenerator.CreateForRSA(certificate.GetRSAPrivateKey(), System.Security.Cryptography.RSASignaturePadding.Pkcs1), new byte[0]);
                    // generator.AddSigner(, new byte[0], "");

                    var rsa = CertificateManager.DecryptPrivateKey(certificatePair.Value, password);
                    if (rsa == null)
                    {
                        return null;
                    }

                    return null;

                    // var signedData = rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                    // return Uri.EscapeDataString(Convert.ToBase64String(signedData));
                }
            }

            return null;
        }

        private dynamic GetCertificateList(string location)
        {
            var list = new List<object>();
            foreach (var certificate in GetCertificates(location).Keys)
            {
                var sb = new StringBuilder();

                sb.AppendLine("-----BEGIN CERTIFICATE-----");
                sb.AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                sb.AppendLine("-----END CERTIFICATE-----");

                var cert = sb.ToString();
                var certId = GetCertificateId(certificate);

                list.Add(new
                {
                    CERT_ID = Uri.EscapeDataString(certId),
                    CERT = Uri.EscapeDataString(cert),
                });
            }

            return list;
        }

        private Dictionary<X509Certificate2, EncryptedPrivateKeyInfo> GetCertificates(string location)
        {
            if (!Property.TryGetValue("certmanui_oid", out var oidProperty))
            {
                return null;
            }

            var oids = oidProperty.Split('|').Where(x => OidMap.ContainsKey(x)).Select(x => OidMap[x]);

            switch (location)
            {
                case "HARD_DISK":
                    return CertificateManager.GetListFromDisk(oids);
                default:
                    Log.W($"Unknown device id: {location}");
                    return new Dictionary<X509Certificate2, EncryptedPrivateKeyInfo>();
            }
        }

        private string GetCertificateId(X509Certificate2 certificate)
        {
            var issuer = certificate.Issuer.Replace(", ", ",");
            issuer = Regex.Replace(issuer, @"[A-Z]+=", m => m.Value.ToLower());
            issuer = Uri.EscapeDataString(issuer);

            var serialNumber = certificate.SerialNumber;
            serialNumber = Regex.Replace(serialNumber, ".{2}", "$0:");
            serialNumber = Uri.EscapeDataString(serialNumber);

            return $"id={issuer}&sn={serialNumber}";
        }
    }
}

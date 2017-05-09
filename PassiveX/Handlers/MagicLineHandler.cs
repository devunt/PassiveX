using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace PassiveX.Handler
{
    [Handler(hostname: "mlnp.dreamsecurity.com", port: 52233)]
    class MagicLineHandler : IHandler
    {
        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            dynamic data = JsonConvert.DeserializeObject(request.Content.Replace("\\", "\\\\"));

            switch ((string)data.MessageID)
            {
                case "InstallCheck":
                    response.SetJson(new { ResultCode = 0, ResultMessage = "9.9.9.9" }); // clientVersion;
                    break;
                case "CrossCertification":
                    response.SetJson(new { ResultCode = 0, ResultMessage = "MIIFiwIBADGB5DCB4QIBADBKMEQxCzAJBgNVBAYTAktSMRYwFAYDVQQKEw1EcmVhbVNlY3VyaXR5MQ4wDAYDVQQLEwVXaXJlZDENMAsGA1UEAxMEUk9PVAICB6AwDQYJKoZIhvcNAQEBBQAEgYDRIgGIV/iB2yVA2hRtl9JfWYA8j7SMz/b4pnI2QzR9ClSWAXVe5m3+Sn6MeN/p3voirfMrUaFjZM8H/AN3fimm4PWGLiZ1EHJn2O1wXk1XLCzsw4QxDsb1p79pA9IgP++mmCecfFXN9O4J8cW1WrfGBo6gzjLJNzbqsIq+WKmzPTCCBJ0GCSqGSIb3DQEHATAcBggqgxqMmkQBBAQQRnwr93CQSux/Kf3pn3EoVICCBHDU5pSShsxpqwMv3DwlHTj3aPLmtA2ntpZBKd05ZDzj8Nil/oHvnpz2WiyCdON2IctPpWJ68Iuq9G/MHduuGk1t5LKsb3r/6TCmWkgmNNfcHjLqKPIAXZH5FZUujJX29p31D68ID4vVj43kwbErQYEqlDwXp6UMaS3cAhI/uwvX+qCrs9v8Ah+naDM1+6NBqZKgVvcyS0X6jC3BRanq8H/aEbh1qnCjPsP2sO3CjHkMYlEulKZjpS45MEVRof+o/DnEQLP91nY7Cg6kdFLkIENvhovQVzhHTsH6+LB6ko46tmY/1kTy9021JDjpyYWBnXDApH2vtb1I8yBaugByB6gWKt0im5MXQiPCDiC+RF0IaN21l5JH8IorYP/aD6+dLQyom5GAXyGi54kUigo/aPTlYLS3ApRs+19fK3B48h6Mg+NigcE8Ho9t6pBcO60yBRpRzY962ZmAbUyIQtrE1S745bljyhE0//kRilUJpoq/LUo3lBIeLa7XPl/k00m+wzBlWTzh/+W2WV23UN/nvJEgfDMG/O3Bgq99jOAODoaCzAcl4O2zCWUVYgeKcXxaVchRcyyidS4He1FMfaVO0dvlECdw0CSAvHYWXblrIWlRPXbEjaoTTfqNe+CdyTjVpG/8Hg3mpDGvPlnEjIcx5L308H5/61Ye5fkp6nuuVyrMxY5CR7p2SObkLrNueQHzeid+weeu10SUlNN3X1u0gBbuOTS2oG7uV8bI9/B85K3Qx5B5pskd9eN58uOYKh2hF5DTPh18h9DnrCguEq4pLXVnIDW95E7bRJXxK3z5fJXxWs/YCZJ/vLqV2gzfHKQ+BrUK9P2ffQDP8rGiEpIjobOm94cl8dmcorYeU1NmC2QY0jI5L4jaTqoDNm9pWzaF7I172xbBl6/olYcWI3opBuAWgom8ZdRzh7E6Q/DpyaSH/Y5fIe3FDb2fQs/BbXpEacx6wuUvXeDmGCtYg+m6IgGr/miK0erxhsx28eqnTmza3ZU2/R82muKfB5pr+CG30KK7oBLTf4J9vjyDBsuPH8Z8lvJdsq/jThLYpM9k6VvxUHu0FQzR5Funefv9rSSAPj4k8mSG4xcfbpeHekaY1LZHqTObAQj/k1s14LWp4K94ZSyJ9yDGQTW88CPwl+NqPumCbVeTissCMsIV2HXTq4WRuVqXSpRYvcB7lpEAkFDHVkMokreAr68jr5c+DefkJxMetIM/peXf+FO+NenrmUgoR2SPPiURIiRyJlqWVmZp9IpUtvz3OyC7kMx6fdtqYyyW4Z2TpI9E6pxKtr5aeHIFH3gG34NjRafejEQtib3FD0xIavzPjtj6UAiUvxt/fgtPa2vhnIzH2LXhDvnBeXHMY7hJ8p8kk6PS+QOdkoFfS9wZB6VVOmB5bQ8nq60D/BhLmW60xssCMb2eqXAQFEN7Fbc9givXFGU/McrF6TSs5QXvS1iJVYceDwT9lIAZG/eGV6Xu8A5b96tS531BzBTaAxofC1jIy2hMYq/fn6/Hig==" });
                    break;
                case "EnvelopData":
                    response.SetJson(new { ResultCode = 1, ResultMessage = "aGVsbG93b3JsZA==" });
                    break;
                case "GetReturnData":
                    response.SetJson(new { ResultCode = 0, ResultMessage = "" });
                    break;
                case "Init":
                case "SetVerifyRootCert":
                    response.SetJson(new { ResultCode = 1 });
                    break;
                case "UbiKeyInit":
                case "SetInitOption":
                case "SetPhoneCertOpt":
                case "SetMobileKeyURL":
                case "SetLanguageOption":
                case "SetCertManageOption":
                case "SetEnableMediaType":
                case "SetVirtualKeyPad":
                case "SetTokenOption":
                case "SetProperty":
                case "SetProperty2":
                case "setCharset":
                case "setSessionID":
                case "TerminateCS":
                    response.SetJson(new { ResultCode = 0 });
                    break;
                default:
                    Console.WriteLine($"Unknwon message: {data.MessageID}");
                    break;
            }

            response.Headers["Access-Control-Allow-Origin"] = "*";
            return Task.FromResult(response);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace PassiveX.Handler
{
    [Handler(hostname: "127.0.0.1", port: 16105)]
    class VeraportHandler : IHandler
    {
        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            if (request.Method == "GET")
            {
                var callback = request.Parameters["callback"];
                var datafield = request.Parameters["data"];
                dynamic data = JsonConvert.DeserializeObject(datafield);

                object result = null;

                var method = GetType().GetMethod((string)data.cmd, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    result = method.Invoke(this, null);
                }
                else
                {
                    Console.WriteLine(data);
                }

                var serialized = JsonConvert.SerializeObject(result);
                response.Content = $"{callback}({serialized})";
                response.Headers["Content-Type"] = "application/javascript";
            }

            return Task.FromResult(response);
        }

        private dynamic getVersion()
        {
            return new { res = 0, data = "9,9,9,9" };
        }

        private dynamic getAxInfo()
        {
            return new
            {
                res = 0,
                data = new[] {
                    new {
                        allowrun = true,
                        allowrundomains = "",
                        backupurl = "http://download.softforum.com/Published/AnySign/v1.1.0.7/AnySign_Installer.exe",
                        block = false,
                        browsertype = "Mozilla",
                        browserversion = "-1",
                        description = "https://www.shinhancard.com/solution/anysign/install/v1.1.0.5/AnySign_Installer.exe",
                        displayname = "AnySign-multi",
                        downloadurl = "http://download.softforum.com/Published/AnySign/v1.1.0.7/AnySign_Installer.exe",
                        forceinstall = false,
                        installstate = true,
                        killbit = false,
                        localversion = "9,9,9,9",
                        objectclsid = "file:%ProgramFiles%\\Softforum\\XecureWeb\\AnySign\\dll\\AnySign4PCLauncher.exe",
                        objectname = "AnySign",
                        objecttype = 0,
                        objectversion = "9.9.9.9",
                        policydisable = false,
                        systemtype = 0,
                        updatestate = false,
                        version = 5
                    },
                    new {
                        allowrun = true,
                        allowrundomains = "",
                        backupurl = "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe",
                        block = false,
                        browsertype = "Mozilla",
                        browserversion = "-1",
                        description = "",
                        displayname = "NOS-multi",
                        downloadurl = "https://supdate.nprotect.net/nprotect/nos_service/windows/install/nos_setup.exe",
                        forceinstall = false,
                        installstate = true,
                        killbit = false,
                        localversion = "9999,99,9,9",
                        objectclsid = "file:%ProgramFiles%\\INCAInternet\\nProtect Online Security\\nosstarter.npe",
                        objectname = "NOS",
                        objecttype = 0,
                        objectversion = "",
                        policydisable = false,
                        systemtype = 0,
                        updatestate = false,
                        version = 5
                    },
                },
            };
        }

        private dynamic isRunning()
        {
            return new { res = 0, data = 0 };
        }
    }
}

using System;
using System.Threading.Tasks;
using PassiveX.Transports;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 55920)]
    internal class ASTxHandler : IHttpHandler
    {
        private readonly string _prefix = "/ASTX2";

        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            var callback = request.Parameters["callback"];

            dynamic result = null;
            switch (request.Path.Remove(0, _prefix.Length))
            {
                case "/hello":
                case "/alive":
                case "/get_pclog":
                case "/set_cert":
                    result = new { result = "ACK" };
                    break;

                default:
                    Log.W($"Unknwon path: {request.Path}");
                    break;
            }

            response.SetJsonCallback(callback, result);

            return Task.FromResult(response);
        }
    }
}

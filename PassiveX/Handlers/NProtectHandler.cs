using System.Threading.Tasks;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 14430)]
    internal class NProtectHandler : IHandler
    {
        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            if (request.Method == "GET")
            {
                var code = request.Parameters["code"];
                var dummy = request.Parameters["dummy"];

                response.SetResource("1x1.gif");
            }

            return Task.FromResult(response);
        }
    }
}

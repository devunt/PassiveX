using System.Threading.Tasks;

namespace PassiveX.Handler
{
    interface IHandler
    {
        Task<HttpResponse> HandleRequest(HttpRequest request);
    }
}

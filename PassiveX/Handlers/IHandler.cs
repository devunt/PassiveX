using System.Threading.Tasks;

namespace PassiveX.Handlers
{
    interface IHandler
    {
        Task<HttpResponse> HandleRequest(HttpRequest request);
    }
}

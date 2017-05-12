using System.Threading.Tasks;
using PassiveX.Transports;

namespace PassiveX.Handlers
{
    internal interface IHandler<in TRequest, TResponse>
        where TRequest : IRequest
        where TResponse : IResponse
    {
        Task<TResponse> HandleRequest(TRequest request);
    }
}

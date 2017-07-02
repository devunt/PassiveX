using System.Threading.Tasks;
using PassiveX.Transports;
using PassiveX.Utils;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 10531, type: HandlerType.Ws)]
    internal class AnySignHandler : IWsHandler
    {
        public Task<WsResponse> HandleRequest(WsRequest request)
        {
            var response = new WsResponse();

            var data = request.GetJson();
            var message = data.message;
            dynamic value = null;

            switch ((string)message.InterfaceName)
            {
                case "setAttributeInfo":
                    value = "hello world";
                    break;

                default:
                    Log.W($"Unknwon interface name: {message.InterfaceName}");
                    break;
            }

            var result = new
            {
                protocolType = "general",
                message = new
                {
                    message.InterfaceName,
                    message.MessageUID,
                    message.SessionID,
                    InterfaceErrorCode = 0,
                    InterfaceErrorMessage = "",
                    ReturnValue = value,
                    ReturnType = value is int ? "number" : "string",
                },
            };

            response.SetJson(result);

            return Task.FromResult(response);
        }
    }
}

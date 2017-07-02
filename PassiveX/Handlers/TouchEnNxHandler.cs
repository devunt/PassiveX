using System.Threading.Tasks;
using Newtonsoft.Json;
using PassiveX.Transports;
using PassiveX.Utils;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 34581, type: HandlerType.Ws)]
    internal class TouchEnNxHandler : IWsHandler
    {
        public Task<WsResponse> HandleRequest(WsRequest request)
        {
            var response = new WsResponse();

            var data = request.GetJson();
            if (data == null)
            {
                return Task.FromResult(response);
            }

            dynamic result = null;
            dynamic callbackReply = null;
            if (data.init != null)
            {
                switch ((string)data.init)
                {
                    case "get_versions":
                        result = new
                        {
                            tabid = data.tabid,
                            daemon = "9.9.9.9",
                            ex = "9.9.9.9",
                            m = new[] { new { name = data.m, version = "9.9.9.9" } }
                        };
                        break;
                    default:
                        Log.W($"Unknwon init command: {data.init}");
                        break;
                }
            }
            else if (data.cmd != null)
            {
                var callback = data.callback;
                dynamic reply = null;
                switch ((string)data.cmd)
                {
                    case "setcallback":
                        reply = "";
                        break;

                    case "native":
                        var args = data.exfunc.args;
                        switch ((string)data.exfunc.fname)
                        {
                            case "Request":
                                dynamic requestParams = JsonConvert.DeserializeObject((string)args[0]);
                                switch ((string)requestParams.key)
                                {
                                    case "Clear":
                                        callbackReply = new { InputClear = requestParams.elename };
                                        break;
                                    case "InvalidateSession":
                                        break;
                                    default:
                                        Log.W($"Unknown request type: {requestParams.key}");
                                        break;
                                }
                                reply = "";
                                break;

                            case "Key_Init":
                                reply = new
                                {
                                    result = "true",
                                    isvm = "false",
                                };
                                callbackReply = new
                                {
                                    SeedKey = "SeedKey",
                                    value = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                                };
                                break;

                            case "Key_Start":
                                reply = "StartReadyComplete";
                                // callbackReply = new { addChar = $"a_#_{args[1]}" };
                                break;

                            case "Key_Keydown":
                                reply = "";
                                break;

                            case "Key_Stop":
                                reply = "";
                                callbackReply = new { GetActiveElement = args[2] };
                                break;

                            case "Key_RealStop":
                                reply = "";
                                break;

                            default:
                                Log.W($"Unknown native call: {data.exfunc.fname}");
                                break;
                        }
                        break;

                    default:
                        Log.W($"Unknwon command: {data.cmd}");
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

            if (callbackReply != null)
            {
                var callbackResult = new
                {
                    response = new
                    {
                        id = "setcallback",
                        callback = "update_callback",
                        reply = callbackReply,
                    }
                };

                var callbackResponse = new WsResponse();
                callbackResponse.SetJson(callbackResult);
                response.Additional = callbackResponse;
            }

            return Task.FromResult(response);
        }
    }
}

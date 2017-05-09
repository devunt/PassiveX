using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassiveX.Handlers
{
    [Handler(hostname: "127.0.0.1", port: 64032)]
    class KDefenseHandler : IHandler
    {
        private static byte[] mapping = new byte[] { 32, 46, 36, 91, 94, 39, 58, 62, 126, 96, 124, 45, 123, 64, 59, 43, 55, 53, 51, 50, 56, 48, 57, 49, 52, 54, 35, 125, 40, 44, 42, 95, 61, 87, 86, 88, 69, 90, 71, 70, 79, 82, 84, 74, 81, 78, 75, 85, 89, 76, 77, 66, 67, 65, 72, 68, 80, 83, 73, 60, 37, 93, 34, 38, 41, 99, 108, 120, 102, 103, 116, 113, 109, 114, 106, 104, 101, 111, 115, 112, 107, 98, 118, 100, 97, 119, 110, 122, 105, 121, 117, 33, 92, 47, 63, 55 };
        private List<string> elements = new List<string>();

        public Task<HttpResponse> HandleRequest(HttpRequest request)
        {
            var response = new HttpResponse();

            switch (request.Path)
            {
                case "/":
                    response.SetResource("kdefense.html");
                    break;

                case "/images/ping.png":
                    response.SetResource("1x1.png");
                    break;

                case "/handshake":
                    response.SetJson(new { needUpdate = false, bridgeVersion = 99999999, serviceVersion = 99999999 });
                    break;

                case "/info":
                    response.SetJson(new { result = "OK", seed = "OzzPd4nP+l43vNepnRc7dENZVOlvxAmvEUaLka39Q3jlGtcA0CX536BD3Jn95NFs8a4w2RK287wKinYoeGc+NoV/drxUZhecSr62whNiZYUXNUefwKOEi43jj2LjUJnamg7UMy/GcKg9bgcVWS0CtKonfQYWxd5vevpI/ui2jY6Ix3WqrmdcVN0XDQhhkXZZNxJrxeQL6odeX8+7CI+fxjZrqCmmz3KGe/acy9x/gsFExHzW4+h+56HMhGeHD8vFh6gnbZc2Ud+njqa3fDx4nXdBVgiJlqIFLO1MlJV9uh0K1Rq3XwSi9kpF3Pi7wIPjwEoyN2O/rpfH5kroc3795w==" });
                    break;

                case "/registerElement":
                    elements.Add(request.Parameters["inputname"]);
                    response.SetJson(new { result = "OK" });
                    break;

                case "/inputFocus":
                    response.SetJson(new { result = "OK" });
                    break;

                case "/inputBlur":
                    response.SetJson(new { result = "OK" });
                    break;

                case "/inputKeyPress":
                    var keycode = byte.Parse(request.Parameters["dummy"]);
                    if (elements.Contains(request.Parameters["inputname"]))
                    {
                        var idx = keycode - 32;
                        if (idx > 0 && idx < mapping.Length)
                            keycode = mapping[idx];
                        else
                            keycode = 55;
                    }
                    response.SetJson(new { result = "OK", keyCode = keycode });
                    break;

                case "/inputBackspace":
                    response.SetJson(new { result = "OK" });
                    break;

                default:
                    Console.WriteLine($"Unknwon path: {request.Path}");
                    break;
            }

            return Task.FromResult(response);
        }
    }
}

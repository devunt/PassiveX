using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PassiveX.Handlers;
using PassiveX.Transports;
using PassiveX.Utils;

namespace PassiveX
{
    internal class ServiceRunner<T> where T : new()
    {
        private HandlerAttribute Attribute { get; }
        private string Identifier { get; }
        private T Handler { get; }

        internal ServiceRunner()
        {
            Handler = new T();

            var type = typeof(T);
            Attribute = type.GetTypeInfo().GetCustomAttribute<HandlerAttribute>();

            Identifier = $"{type.Name}";
        }

        internal async Task Run()
        {
            var certificate = CertificateBuilder.Build(Attribute.Hostname);
            var listener = new TcpListener(IPAddress.Loopback, Attribute.Port);
            listener.Start();

            Log.I($"[{Identifier}] Listening on {Attribute.Hostname}:{Attribute.Port}");

            while (true)
            {
                // Log.D($"[{Identifier}] Waiting for new connection");
                var client = await listener.AcceptTcpClientAsync();
                // Log.D($"[{Identifier}] Got connection from {client.Client.RemoteEndPoint}");
                var sslStream = new SslStream(client.GetStream(), false);
                try
                {
                    await sslStream.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls12, false);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;

                    using (var reader = new StreamReader(sslStream))
                    {
                        IAsyncEnumerator<byte[]> responses = null;

                        switch (Attribute.Type)
                        {
                            case HandlerType.Http:
                                responses = HandleHttpRequest(reader);
                                break;
                            case HandlerType.Ws:
                                responses = HandleWsRequest(reader);
                                break;
                        }

                        try
                        {
                            await responses.ForEachAsync(
                                async response => await sslStream.WriteAsync(response, 0, response.Length));
                        }
                        catch (TaskCanceledException) { }
                    }
                }
                catch (Exception ex)
                {
                    Log.Ex(ex, $"Exception occured while handling request on {Identifier} ");
                }
                finally
                {
                    sslStream.Dispose();
                    client.Close();
                }
            }
        }

        private IAsyncEnumerator<byte[]> HandleHttpRequest(StreamReader reader)
        {
            return new AsyncEnumerator<byte[]>(async yield =>
            {
                while (true)
                {
                    var requestLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(requestLine))
                    {
                        yield.Break();
                    }

                    var request = new HttpRequest(requestLine);
                    while (true)
                    {

                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line))
                        {
                            if (request.Headers.TryGetValue("Content-Length", out string contentLength))
                            {
                                var length = int.Parse(contentLength);
                                var bytes = new List<char>(length);
                                while (bytes.Count < length)
                                {
                                    var buffer = new char[length];
                                    var readCount = await reader.ReadAsync(buffer, 0, length);
                                    bytes.AddRange(buffer.Take(readCount));
                                }

                                request.AddBody(bytes.Select(c => (byte) c));
                            }

                            break;
                        }

                        request.AddHeader(line);
                    }
                    Log.D($"[{Identifier}] {request.Method} {request.Path}");

                    var response = await ((IHttpHandler) Handler).HandleRequest(request);
                    await yield.ReturnAsync(response.ToBytes());

                    if (response.Headers.TryGetValue("Connection", out object connection))
                    {
                        if (connection.ToString().ToLower() == "close")
                        {
                            yield.Break();
                        }
                    }
                }
            });
        }

        private IAsyncEnumerator<byte[]> HandleWsRequest(StreamReader reader)
        {
            return new AsyncEnumerator<byte[]>(async yield =>
            {
                var requestLine = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    yield.Break();
                }

                var httpRequest = new HttpRequest(requestLine);
                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    httpRequest.AddHeader(line);
                }

                Log.D($"[{Identifier}] WS Handshake {httpRequest.Path}");

                var key = Encoding.ASCII.GetBytes(httpRequest.Headers["Sec-WebSocket-Key"] + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");

                var httpResponse = new HttpResponse();
                httpResponse.StatusCode = HttpStatusCode.SwitchingProtocols;
                httpResponse.Headers["Connection"] = "Upgrade";
                httpResponse.Headers["Upgrade"] = "websocket";
                httpResponse.Headers["Sec-WebSocket-Accept"] = Convert.ToBase64String(SHA1.Create().ComputeHash(key));

                await yield.ReturnAsync(httpResponse.ToBytes());

                while (true)
                {
                    var request = new WsRequest();

                    while (true)
                    {
                        var buffer = new byte[1024];
                        var readCount = await reader.BaseStream.ReadAsync(buffer, 0, buffer.Length);

                        if (request.AddBytes(buffer.Take(readCount)))
                        {
                            break;
                        }
                    }

                    Log.D($"[{Identifier}] WS Message ({request.Opcode})");

                    switch (request.Opcode)
                    {
                        case WsOpcode.Text:
                        case WsOpcode.Binary:
                            var response = await ((IWsHandler)Handler).HandleRequest(request);
                            await yield.ReturnAsync(response.ToBytes());
                            break;
                        case WsOpcode.Close:
                            yield.Break();
                            break;
                        case WsOpcode.Ping:
                        case WsOpcode.Pong:
                            break;
                        default:
                            Log.B(request.RawContent);
                            break;
                    }
                }
            });
        }
    }
}

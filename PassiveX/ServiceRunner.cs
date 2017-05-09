using PassiveX.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PassiveX
{
    class ServiceRunner<T> where T : IHandler, new()
    {
        HandlerAttribute Attribute { get; set; }
        string Identifier { get; set; }
        T Handler { get; set; }

        public ServiceRunner() {
            Handler = new T();

            var type = typeof(T);
            Attribute = type.GetTypeInfo().GetCustomAttribute<HandlerAttribute>();

            Identifier = $"{type.Name}:{Attribute.Port}";
        }

        public async Task Run()
        {
            var certificate = new X509Certificate2Builder("Resources/ca.pfx").Build(Attribute.Hostname);
            var listener = new TcpListener(IPAddress.Loopback, Attribute.Port);
            listener.Start();

            Console.WriteLine($"[{Identifier}] Waiting for a client to connect.");

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                // Console.WriteLine($"[{Identifier}] Got a client connection.");
                var sslStream = new SslStream(client.GetStream(), false);
                try
                {
                    await sslStream.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls, true);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;

                    using (var reader = new StreamReader(sslStream))
                    {
                        var requestline = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(requestline))
                        {
                            continue;
                        }

                        var request = new HttpRequest(requestline);
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

                                    request.AddBody(bytes.Select(c => (byte)c));
                                }

                                break;
                            }

                            request.AddHeader(line);
                        }

                        var response = await Handler.HandleRequest(request);
                        var responseBytes = response.ToBytes();

                        await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }
                finally
                {
                    sslStream.Dispose();
                    client.Dispose();
                }
            }
        }
    }
}

using PassiveX.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PassiveX
{
    class ServiceRunner<T> where T : IHandler, new()
    {
        string Identifier { get; set; }
        int Port { get; set; }
        T Handler { get; set; }

        public ServiceRunner(int port) {
            Port = port;
            Handler = new T();
            Identifier = $"{typeof(T).Name}:{Port}";
        }

        public async Task Run()
        {
            var certificate = new X509Certificate2("Resources/localhost.pfx", "");
            var listener = new TcpListener(IPAddress.Loopback, Port);
            listener.Start();

            while (true)
            {
                Console.WriteLine($"[{Identifier}] Waiting for a client to connect.");
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine($"[{Identifier}] Got a client connection.");
                var sslStream = new SslStream(client.GetStream(), false);
                try
                {
                    await sslStream.AuthenticateAsServerAsync(certificate, false, SslProtocols.Tls, true);
                    sslStream.ReadTimeout = 5000;
                    sslStream.WriteTimeout = 5000;

                    using (var reader = new StreamReader(sslStream))
                    {
                        var request = new HttpRequest(await reader.ReadLineAsync());
                        while (true)
                        {

                            var line = await reader.ReadLineAsync();
                            if (line == "")
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

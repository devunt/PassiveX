using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PassiveX
{
    internal class HttpResponse
    {
        internal string Version { get; set; }
        internal HttpStatusCode StatusCode { get; set; }
        internal Dictionary<string, object> Headers { get; set; }
        internal byte[] RawContent { get; set; }
        internal string Content
        {
            get
            {
                return Encoding.UTF8.GetString(RawContent);
            }
            set
            {
                RawContent = Encoding.UTF8.GetBytes(value);
            }
        }

        internal HttpResponse()
        {
            Version = "HTTP/1.1";
            StatusCode = HttpStatusCode.OK;
            Headers = new Dictionary<string, object>();
            RawContent = new byte[0];

            Headers["Connection"] = "close";
        }

        internal byte[] ToBytes()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("{0} {1} {2}\r\n", Version.Trim(), (int)StatusCode, StatusCode);
            builder.AppendFormat("{0}\r\n", string.Join("\r\n", Headers.Select(x => $"{x.Key.Trim()}: {x.Value.ToString().Trim()}")));

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.UTF8.GetBytes(builder.ToString()));

            if (RawContent.Any())
            {
                Headers["Content-Length"] = RawContent.Length;

                bytes.AddRange(Encoding.UTF8.GetBytes("\r\n"));
                bytes.AddRange(RawContent);
            }

            bytes.AddRange(Encoding.UTF8.GetBytes("\r\n"));

            return bytes.ToArray();
        }

        internal void SetResource(string path, string mime = null)
        {
            if (mime == null)
            {
                new FileExtensionContentTypeProvider().TryGetContentType(path, out mime);
            }

            Headers["Content-Type"] = mime ?? "application/octet-stream";
            RawContent = File.ReadAllBytes(Path.Combine("Resources", path));
        }

        internal void SetJson(dynamic obj)
        {
            Headers["Content-Type"] = "application/json";
            Content = JsonConvert.SerializeObject(obj);
        }

        internal void SetJsonCallback(string callback, dynamic obj)
        {
            Headers["Content-Type"] = "application/javascript";
            var json = obj == null ? "{}" : JsonConvert.SerializeObject(obj);
            Content = $"{callback}({json})";
        }
    }
}

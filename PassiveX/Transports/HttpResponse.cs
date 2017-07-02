using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using PassiveX.Utils;

namespace PassiveX.Transports
{
    internal class HttpResponse : IResponse
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
            Headers["Access-Control-Allow-Origin"] = "*";
        }

        public byte[] ToBytes()
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

        internal void SetResource(string filename, string mime = null)
        {
            if (mime == null)
            {
                mime = MimeMapping.GetMimeMapping(filename);
            }

            Headers["Content-Type"] = mime ?? "application/octet-stream";
            RawContent = Resource.Get(filename);
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

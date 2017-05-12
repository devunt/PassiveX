using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace PassiveX.Transports
{
    internal class HttpRequest : IRequest
    {
        internal string Method { get; private set; }
        internal string Path { get; private set; }
        internal string Version { get; private set; }
        internal Dictionary<string, string> Headers { get; }
        internal Dictionary<string, string> Parameters { get; private set; }
        internal byte[] RawContent { get; private set; }
        internal string Content
        {
            get
            {
                return Encoding.UTF8.GetString(RawContent);
            }
            private set
            {
                RawContent = Encoding.UTF8.GetBytes(value);
            }
        }

        internal HttpRequest(string requestLine)
        {
            var entities = requestLine.Split(' ');
            Method = entities[0];
            Version = entities[2];

            var uri = new Uri(@"https://127.0.0.1" + entities[1]);
            var qs = QueryHelpers.ParseQuery(uri.Query);

            Path = uri.AbsolutePath;
            Parameters = qs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value[0]);

            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            RawContent = new byte[0];
        }

        internal void AddHeader(string line)
        {
            var pair = line.Split(new[] { ':' }, 2);
            var name = pair[0].Trim();
            var value = pair[1].Trim();

            Headers[name] = value;
        }

        internal void AddBody(IEnumerable<byte> data)
        {
            RawContent = data.ToArray();

            if (Headers.TryGetValue("Content-Type", out string type))
            {
                if (type == "application/x-www-form-urlencoded")
                {
                    var formdata = new FormReader(Content).ReadForm();
                    Parameters = formdata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value[0]);
                }
            }
        }
    }
}

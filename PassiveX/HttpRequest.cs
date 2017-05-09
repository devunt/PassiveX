﻿using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PassiveX
{
    class HttpRequest
    {
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string Version { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
        public byte[] RawContent { get; set; }
        public string Content
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

        public HttpRequest(string requestLine)
        {
            var entities = requestLine.Split(' ');
            Method = entities[0];
            Version = entities[2];

            var uri = new Uri(@"https://127.0.0.1" + entities[1]);
            var qs = QueryHelpers.ParseQuery(uri.Query);

            Path = uri.AbsolutePath;
            Parameters = qs.ToDictionary(kvp => { return kvp.Key; }, kvp => { return kvp.Value[0]; });

            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            RawContent = new byte[0];
        }

        public void AddHeader(string line)
        {
            var pair = line.Split(new char[] { ':' }, 2);
            var name = pair[0].Trim();
            var value = pair[1].Trim();

            Headers[name] = value;
        }

        public void AddBody(IEnumerable<byte> data)
        {
            if (Headers.TryGetValue("Content-Type", out string type))
            {
                if (type == "application/x-www-form-urlencoded")
                {
                    var formdata = new FormReader(Encoding.UTF8.GetString(data.ToArray())).ReadForm();
                    Parameters = formdata.ToDictionary(kvp => { return kvp.Key; }, kvp => { return kvp.Value[0]; });
                    return;
                }
            }

            RawContent = data.ToArray();
        }
    }
}

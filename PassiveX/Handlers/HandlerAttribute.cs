using System;

namespace PassiveX.Handlers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed internal class HandlerAttribute : Attribute
    {
        public string Hostname { get; private set; }
        public int Port { get; private set; }
        public HandlerType Type { get; private set; }

        internal HandlerAttribute(string hostname, int port, HandlerType type = HandlerType.Http)
        {
            Hostname = hostname;
            Port = port;
            Type = type;
        }
    }

    internal enum HandlerType
    {
        Http,
        Ws,
    }
}

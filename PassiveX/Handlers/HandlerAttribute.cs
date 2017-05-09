using System;

namespace PassiveX.Handler
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    sealed class HandlerAttribute : Attribute
    {
        public string Hostname { get; private set; }
        public int Port { get; private set; }

        public HandlerAttribute(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }
    }
}

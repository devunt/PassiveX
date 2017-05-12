using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PassiveX.Transports
{
    internal class WsResponse : IResponse
    {
        internal WsOpcode Opcode { get; private set; }
        internal bool Finish { get; private set; }
        internal byte[] RawContent { get; set; }

        internal string Content
        {
            get { return Encoding.UTF8.GetString(RawContent); }
            set
            {
                RawContent = Encoding.UTF8.GetBytes(value);
                Opcode = WsOpcode.Text;
            }
        }

        internal WsResponse()
        {
            Opcode = WsOpcode.Binary;
            Finish = true;
            RawContent = new byte[0];
        }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                ms.WriteByte((byte) ((byte) Opcode | (Finish ? 0x80 : 0x00)));

                if (RawContent.Length < 126)
                {
                    ms.WriteByte((byte)RawContent.Length);
                }
                else if (RawContent.Length < ushort.MaxValue)
                {
                    ms.WriteByte(126);
                    var bytes = BitConverter.GetBytes((ushort)RawContent.Length);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    ms.Write(bytes, 0, bytes.Length);
                }
                else
                {
                    ms.WriteByte(127);
                    var bytes = BitConverter.GetBytes((ulong)RawContent.Length);
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    ms.Write(bytes, 0, bytes.Length);
                }

                ms.Write(RawContent, 0, RawContent.Length);

                return ms.ToArray();
            }
        }

        internal void SetJson(dynamic obj)
        {
            Content = JsonConvert.SerializeObject(obj);
        }
    }
}

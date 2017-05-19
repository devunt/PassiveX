using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PassiveX.Transports
{
    internal class WsRequest : IRequest
    {
        internal WsOpcode Opcode { get; private set; }
        internal ulong Length { get; private set; }
        internal bool Finish { get; private set; }
        internal byte[] XorParameters { get; private set; }
        internal byte[] RawContent { get; private set; }

        internal string Content
        {
            get { return Encoding.UTF8.GetString(RawContent); }
            private set { RawContent = Encoding.UTF8.GetBytes(value); }
        }

        private bool _length;
        private bool _masked;
        private readonly List<byte> _data = new List<byte>();

        internal WsRequest()
        {
            Opcode = WsOpcode.None;
            RawContent = new byte[0];
        }

        internal bool AddBytes(IEnumerable<byte> bytes)
        {
            _data.AddRange(bytes);

            if (Opcode == WsOpcode.None)
            {
                if (_data.Count == 0)
                {
                    return false;
                }

                Finish = (_data[0] & 0x80) == 0x80;
                Opcode = (WsOpcode) (_data[0] & 0xf);

                _data.RemoveRange(0, 1);
            }

            if (!_length)
            {
                if (_data.Count == 0)
                {
                    return false;
                }

                _masked = (_data[0] & 0x80) == 0x80;
                Length = (ulong)(_data[0] & 0x7f);
                if (Length <= 125)
                {
                    _length = true;
                    _data.RemoveRange(0, 1);
                }
                else if (Length == 126)
                {
                    if (_data.Count < 3)
                    {
                        return false;
                    }

                    var lengthBytes = _data.Skip(1).Take(2);
                    if (BitConverter.IsLittleEndian)
                        lengthBytes = lengthBytes.Reverse();
                    Length = BitConverter.ToUInt16(lengthBytes.ToArray(), 0);
                    _length = true;
                    _data.RemoveRange(0, 3);
                }
                else if (Length == 127)
                {
                    if (_data.Count < 9)
                    {
                        return false;
                    }

                    var lengthBytes = _data.Skip(1).Take(8);
                    if (BitConverter.IsLittleEndian)
                        lengthBytes = lengthBytes.Reverse();
                    Length = BitConverter.ToUInt64(lengthBytes.ToArray(), 0);
                    _length = true;
                    _data.RemoveRange(0, 9);
                }

                RawContent = new byte[Length];
            }

            if (_masked && XorParameters == null)
            {
                if (_data.Count < 4)
                {
                    return false;
                }

                XorParameters = _data.Take(4).ToArray();
                _data.RemoveRange(0, 4);
            }

            if ((ulong) _data.Count != Length)
            {
                return false;
            }

            for (var i = 0ul; i < Length; i++)
            {
                RawContent[i] = _masked ? (byte) (_data[(int) i] ^ XorParameters[i % 4]) : _data[(int) i];
            }

            return true;
        }

        internal dynamic GetJson()
        {
            try
            {
                return JsonConvert.DeserializeObject(Content);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }

    internal enum WsOpcode
    {
        None = 0,
        Unknown = -1,
        Text = 1,
        Binary = 2,
        Close = 8,
        Ping = 9,
        Pong = 10,
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PassiveX
{
    internal static class Log
    {
        private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");
        private static readonly object Lock = new object();

        private static void Write(ConsoleColor color, string format, params object[] args)
        {
            var datetime = DateTime.Now.ToString("HH:mm:ss");
            var formatted = format;
            try
            {
                formatted = string.Format(format, args);
            } catch (FormatException) { }

            lock (Lock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{datetime}] {formatted}");
                Console.ResetColor();
            }
        }

        internal static void S(string format, params object[] args)
        {
            Write(ConsoleColor.DarkGreen, format, args);
        }

        internal static void I(string format, params object[] args)
        {
            Write(ConsoleColor.Gray, format, args);
        }

        internal static void W(string format, params object[] args)
        {
            Write(ConsoleColor.DarkYellow, format, args);
        }

        internal static void E(string format, params object[] args)
        {
            Write(ConsoleColor.DarkRed, format, args);
        }

        internal static void Ex(Exception ex, string format, params object[] args)
        {
#if DEBUG
            var message = ex.ToString();
#else
            var message = ex.Message;
#endif
            message = Escape(message);
            E(string.Format("{0}: {1}", format, message), args);
        }

        internal static void D(string format, params object[] args)
        {
#if DEBUG
            Write(ConsoleColor.DarkGray, format, args);
#endif
        }

        internal static void B(IEnumerable<byte> buffer)
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            var bytes = buffer.ToArray();

            for (var i = 0; i < bytes.Length; i++)
            {
                if (i != 0)
                {
                    if (i % 16 == 0)
                    {
                        sb.AppendLine();
                    }
                    else if (i % 8 == 0)
                    {
                        sb.Append(' ', 2);
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(bytes[i].ToString("X2"));
            }

            D(sb.ToString());
        }

        private static string Escape(string line)
        {
            return EscapePattern.Replace(line, "{{$1}}");
        }
    }
}
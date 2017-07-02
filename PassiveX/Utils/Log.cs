using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using PassiveX.Forms;

namespace PassiveX.Utils
{
    internal static class Log
    {
        private static readonly Regex EscapePattern = new Regex(@"\{(.+?)\}");
        internal static MainForm Form { get; set; }

        private static void Write(Color color, object format, params object[] args)
        {
            var formatted = format ?? "(null)";
            try
            {
                formatted = string.Format(formatted.ToString(), args);
            }
            catch (FormatException) { }

            var datetime = DateTime.Now.ToString("HH:mm:ss");

            Form.Invoke(() =>
            {
                var item = new ListViewItem(new[] { datetime, formatted.ToString() });
                item.ForeColor = color;
                Form.listView.Items.Insert(0, item);
                Form.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            });
        }

        internal static void S(object format, params object[] args)
        {
            Write(Color.Green, format, args);
        }

        internal static void I(object format, params object[] args)
        {
            Write(Color.Black, format, args);
        }

        internal static void W(object format, params object[] args)
        {
            Write(Color.Yellow, format, args);
        }

        internal static void E(object format, params object[] args)
        {
            Write(Color.Red, format, args);
        }

        internal static void Ex(Exception ex, object format, params object[] args)
        {
#if DEBUG
            var message = ex.Message;
#else
            var message = ex.Message;
#endif
            message = Escape(message);
            E($"{format}: {message}", args);
        }

        internal static void D(object format, params object[] args)
        {
#if DEBUG
            Write(Color.Gray, format, args);
#endif
        }

        internal static void B(byte[] buffer)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < buffer.Length; i++)
            {
                if (i != 0)
                {
                    if (i % 16 == 0)
                    {
                        D(sb.ToString());
                        sb.Clear();
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

                sb.Append(buffer[i].ToString("X2"));
            }
        }

        private static string Escape(string line)
        {
            return EscapePattern.Replace(line, "{{$1}}");
        }
    }
}
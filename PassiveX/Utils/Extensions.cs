using System;
using System.Reflection;
using System.Windows.Forms;

namespace PassiveX.Utils
{
    internal static class Extensions
    {
        internal static void Invoke(this Form form, Action task)
        {
            form.Invoke((MethodInvoker)delegate { task(); });
        }

        public static void DoubleBuffer(this Control control)
        {
            // http://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form/77233#77233
            // Taxes: Remote Desktop Connection and painting: http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx

            if (SystemInformation.TerminalServerSession) return;
            var dbProp = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dbProp != null) dbProp.SetValue(control, true, null);
        }
    }
}

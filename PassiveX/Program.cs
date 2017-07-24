using System;
using System.Windows.Forms;
using PassiveX.Forms;

namespace PassiveX
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}

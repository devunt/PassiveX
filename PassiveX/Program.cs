using System;
using System.Windows.Forms;
using PassiveX.Forms;

namespace PassiveX
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            /*
             * makecert.exe -r -pe -n "CN=! PassiveX Root CA" -sv ca.pvk -a sha256 -len 2048 -b 01/01/2000 -e 12/31/2039 -cy authority ca.cer
             * pvk2pfx -pvk ca.pvk -spc ca.cer -pfx ca.pfx
             */




            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}

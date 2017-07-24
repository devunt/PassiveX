using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using PassiveX.Handlers;
using PassiveX.Utils;

namespace PassiveX.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            listView.DoubleBuffer();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Log.Form = this;

            var rootCertificate = new X509Certificate2(@"C:\projects\PassiveX\PassiveX\Resources\ca.pfx", "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            CertificateBuilder.Initialize(rootCertificate);
            CertificateBuilder.Install();

            GlobalKeyboardHook.Install();

            Task.Run(() =>
            {
                var runners = new[]
                {
                    new ServiceRunner<ASTxHandler>().Run(),
                    new ServiceRunner<AnySignHandler>().Run(),
                    new ServiceRunner<VeraportHandler>().Run(),
                    new ServiceRunner<NProtectHandler>().Run(),
                    new ServiceRunner<KDefenseHandler>().Run(),
                    new ServiceRunner<CrossWebHandler>().Run(),
                    new ServiceRunner<MagicLineHandler>().Run(),
                    new ServiceRunner<TouchEnNxHandler>().Run(),
                };

                Task.WaitAll(runners);
            });
        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var info = listView.HitTest(e.X, e.Y);
            var item = info.Item;

            if (item != null)
            {
                var detailForm = new LogDetailForm { richTextBox = { Text = item.SubItems[1].Text } };
                detailForm.ShowDialog();
            }
        }
    }
}

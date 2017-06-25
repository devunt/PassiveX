using PassiveX.Handlers;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PassiveX
{
    class Program
    {
        static async Task AsyncMain()
        {
            /*
             * makecert.exe -r -pe -n "CN=! PassiveX Root CA" -sv ca.pvk -a sha256 -len 2048 -b 01/01/2000 -e 12/31/2039 -cy authority ca.cer
             * pvk2pfx -pvk ca.pvk -spc ca.cer -pfx ca.pfx
             */

            var rootCertificate = new X509Certificate2(@"Resources/ca.pfx");
            CertificateBuilder.Initialize(rootCertificate);
            CertificateBuilder.Install();

            var astxHandler     = new ServiceRunner<ASTxHandler>();
            var anysignRunner   = new ServiceRunner<AnySignHandler>();
            var veraportRunner  = new ServiceRunner<VeraportHandler>();
            var nProtectRunner  = new ServiceRunner<NProtectHandler>();
            var kDefenceRunner  = new ServiceRunner<KDefenseHandler>();
            var crossWebRunner  = new ServiceRunner<CrossWebHandler>();
            var magicLineRunner = new ServiceRunner<MagicLineHandler>();
            var touchEnNxRunner = new ServiceRunner<TouchEnNxHandler>();

            await await Task.WhenAny(
                astxHandler    .Run(),
                anysignRunner  .Run(),
                veraportRunner .Run(),
                nProtectRunner .Run(),
                kDefenceRunner .Run(),
                crossWebRunner .Run(),
                magicLineRunner.Run(),
                touchEnNxRunner.Run()
            );
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => { Environment.Exit(1); };
            AsyncMain().GetAwaiter().GetResult();
        }
    }
}

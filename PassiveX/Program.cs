using PassiveX.Handlers;
using System;
using System.Threading.Tasks;

namespace PassiveX
{
    class Program
    {
        static async Task AsyncMain()
        {
            CertificateBuilder.Initialize(@"Resources/ca.pfx");
            CertificateBuilder.Install();

            var astxHandler     = new ServiceRunner<ASTxHandler>();
            var veraportRunner  = new ServiceRunner<VeraportHandler>();
            var nProtectRunner  = new ServiceRunner<NProtectHandler>();
            var kDefenceRunner  = new ServiceRunner<KDefenseHandler>();
            var magicLineRunner = new ServiceRunner<MagicLineHandler>();

            await Task.WhenAll(
                astxHandler    .Run(),
                veraportRunner .Run(),
                nProtectRunner .Run(),
                kDefenceRunner .Run()
                kDefenceRunner .Run(),
                magicLineRunner.Run()
            );
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => { Environment.Exit(1); };
            AsyncMain().GetAwaiter().GetResult();
        }
    }
}

using PassiveX.Handler;
using System;
using System.Threading.Tasks;

namespace PassiveX
{
    class Program
    {
        static async Task AsyncMain()
        {
            var builder = new X509Certificate2Builder(@"Resources/ca.pfx");
            builder.Install();

            var veraportRunner  = new ServiceRunner<VeraportHandler>();
            var nProtectRunner  = new ServiceRunner<NProtectHandler>();
            var kDefenceRunner  = new ServiceRunner<KDefenseHandler>();

            await Task.WhenAll(
                veraportRunner .Run(),
                nProtectRunner .Run(),
                kDefenceRunner .Run()
            );
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => { Environment.Exit(1); };
            AsyncMain().GetAwaiter().GetResult();
        }
    }
}

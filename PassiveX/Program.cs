using PassiveX.Handler;
using System;
using System.Threading.Tasks;

namespace PassiveX
{
    class Program
    {
        static async Task AsyncMain()
        {
            var veraportRunner  = new ServiceRunner<VeraportHandler>(16105);
            var nProtectRunner1 = new ServiceRunner<NProtectHandler>(14430);
            var nProtectRunner2 = new ServiceRunner<NProtectHandler>(14440);
            var kDefenceRunner  = new ServiceRunner<KDefenseHandler>(54032);

            await Task.WhenAll(
                veraportRunner .Run(),
                nProtectRunner1.Run(),
                nProtectRunner2.Run(),
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

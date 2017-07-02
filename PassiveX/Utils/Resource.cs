using System.IO;
using System.Reflection;

namespace PassiveX.Utils
{
    internal static class Resource
    {
        public static byte[] Get(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var rs = assembly.GetManifestResourceStream($"PassiveX.Resources.{name}"))
            using (var ms = new MemoryStream())
            {
                if (rs == null)
                {
                    throw new FileNotFoundException($"Resource `{name}` not found");
                }

                rs.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}

using System.IO;

namespace PassiveX
{
    internal static partial class CertificateManager
    {
        private static readonly string[] NpkiDiskPathOnWindows =
        {
            Path.Combine("%LocalAppData%", "..", "LocalLow", "NPKI"),
            Path.Combine("%ProgramFiles%", "NPKI"),
            Path.Combine("%ProgramFiles(x86)%", "NPKI"),
        };

        private static readonly string[] NpkiDiskPathOnLinux =
        {
        };

        private static readonly string[] NpkiDiskPathOnMac =
        {
        };
    }
}

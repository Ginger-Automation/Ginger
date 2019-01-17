using System.Runtime.InteropServices;

namespace GingerUtils
{
    public static class OperatingSystem
    {

        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string GetCurrentOS()
        {
            return
            (IsWindows() ? "windows" : null) ??
            (IsMacOS() ? "mac" : null) ??
            (IsLinux() ? "linux" : null);
        }
    }

}

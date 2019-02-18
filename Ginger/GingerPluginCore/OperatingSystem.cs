using System.Runtime.InteropServices;

namespace GingerPluginCore
{
    //TDOO: Dup with GingerUtils remove and use GingerUtils but verify it is included in the nuget created 
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

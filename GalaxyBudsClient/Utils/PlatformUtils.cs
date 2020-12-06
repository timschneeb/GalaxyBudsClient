using System;
using System.IO;
using System.Runtime.InteropServices;

namespace GalaxyBudsClient.Utils
{
    public static class PlatformUtils
    {
        public static bool IsPlatformSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static string CombineDataPath(string postfix)
        {
            return Path.Combine(AppDataPath, postfix);
        }

        public static string AppDataPath =>
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/GalaxyBudsClient/";
    }
}
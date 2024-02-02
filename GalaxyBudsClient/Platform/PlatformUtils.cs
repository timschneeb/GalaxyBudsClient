using System;
using System.IO;
using System.Runtime.InteropServices;
using Org.BouncyCastle.Bcpg.Sig;
using Serilog;

namespace GalaxyBudsClient.Platform
{
    public static class PlatformUtils
    {
        public enum Platforms
        {
            Windows,
            Linux,
            OSX,
            Other
        }
        
        public static bool IsPlatformSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
                   RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsOSX => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool SupportsTrayIcon => (IsWindows && !IsARMCompileTarget) || IsLinux || IsOSX;
        public static bool SupportsAutoboot => IsWindows || IsOSX;
        public static bool SupportsHotkeys => IsWindows;
        
        public static Platforms Platform
        {
            get
            {
                if (IsWindows)
                {
                    return Platforms.Windows;
                }
                if (IsLinux)
                {
                    return Platforms.Linux;
                }
                if (IsOSX)
                {
                    return Platforms.OSX;
                }

                return Platforms.Other;
            }
        }

        public static bool IsARMCompileTarget
        {
            get
            {
//#if WindowsNoARM
                return false;
//#else
//                return true;
//#endif
            }
        }

        public static bool IsWindowsContractsSdkSupported
        {
            get
            {
                if (!IsWindows)
                {
                    return false;
                }
                
                try
                {
#pragma warning disable CA1416
                    var release = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        "ReleaseId", "")?.ToString();
                    var major = Convert.ToInt32(release?.Substring(0, 2));
                    var minor = Convert.ToInt32(release?.Substring(2, 2));
                    return major >= 18 && minor >= 03;
#pragma warning restore CA1416
                }
                catch (Exception ex)
                {
                    Log.Error($"PlatformUtils: Cannot determine build version: {ex.Message}");
                    return false;
                }
            }
        }


        public static string CombineDataPath(string postfix)
        {
            return Path.Combine(AppDataPath, postfix);
        }

        public static string AppDataPath =>
            $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/GalaxyBudsClient/";
    }
}
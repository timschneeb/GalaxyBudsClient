using System;
using System.IO;
using System.Runtime.InteropServices;
using Serilog;

namespace GalaxyBudsClient.Platform
{
    public static class PlatformUtils
    {
        public enum Platforms
        {
            Windows,
            Linux,
            Other
        }
        
        public static bool IsPlatformSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                   RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool SupportsTrayIcon => IsWindows && !IsARMCompileTarget;
        public static bool SupportsHotkeys => IsWindows;
        
        public static Platforms Platform
        {
            get
            {
                if (IsWindows)
                {
                    return Platforms.Windows;
                }
                else if (IsLinux)
                {
                    return Platforms.Linux;
                }

                return Platforms.Other;
            }
        }

        public static bool IsARMCompileTarget
        {
            get
            {
#if WindowsNoARM
                return false;
#else
                return true;
#endif
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
                    var release = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion",
                        "ReleaseId", "")?.ToString();
                    var major = Convert.ToInt32(release?.Substring(0, 2));
                    var minor = Convert.ToInt32(release?.Substring(2, 2));
                    return major >= 18 && minor >= 03;
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
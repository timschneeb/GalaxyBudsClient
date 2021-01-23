using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Platform
{
    public static class NotifyIconImpl
    {
        public static readonly ITrayIcon Instance;
        
        static NotifyIconImpl()
        {
            if (PlatformUtils.IsWindows)
            {
#if WindowsNoARM
                Instance = new ThePBone.Interop.Win32.TrayIcon.TrayIcon();
#else
                Instance = new Dummy.TrayIcon(); /* No ARM support */
#endif
            }
            else if (PlatformUtils.IsLinux)
            {
                Instance = new ThePBone.Interop.Linux.TrayIcon.TrayIcon();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            Instance.PreferDarkMode = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
            ThemeUtils.ThemeReloaded += OnThemeReloaded;
        }

        private static void OnThemeReloaded(object? sender, DarkModes e)
        {
            Instance.PreferDarkMode = e == DarkModes.Dark;
        }

        public static void Shutdown()
        {
            Log.Debug("NotifyIconImpl: Disposing notify icon");
            Instance.Dispose();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Threading;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Utils;
using Serilog;

namespace GalaxyBudsClient.Platform
{
   
    class NotifyIconImpl
    {
        private readonly ITrayIcon _tray;
        
        public NotifyIconImpl()
        {
            if (PlatformUtils.IsWindows)
            {
                _tray = new ThePBone.Interop.Win32.TrayIcon.TrayIcon();
            }
            else if (PlatformUtils.IsLinux)
            {
                _tray = new ThePBone.Interop.Linux.TrayIcon.TrayIcon();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            
            _tray.MenuItems = new List<TrayMenuItem>()
            {
                new TrayMenuItem("Left: 100%", false),
                new TrayMenuItem("Right: 100%", false),
                new TrayMenuItem("Case: 100%", false),
                new TrayMenuSeparator(),
                new TrayMenuItem("Enable EQ")
            };
            _tray.TrayMenuItemSelected += TrayOnTrayMenuItemSelected;
            _tray.LeftClicked += TrayOnLeftClicked;
            _tray.PreferDarkMode = SettingsProvider.Instance.DarkMode == DarkModes.Dark;
        }

        private void TrayOnLeftClicked(object? sender, EventArgs e)
        {
            Log.Debug("Left clicked");
        }

        private void TrayOnTrayMenuItemSelected(object? sender, TrayMenuItem e)
        {
            Log.Debug(e.Title);
        }
        
    }
}


using System;
using System.Collections.Generic;

namespace GalaxyBudsClient.Interop.TrayIcon
{
    public interface ITrayIcon : IDisposable
    {
        event EventHandler<TrayMenuItem> TrayMenuItemSelected;
        event EventHandler LeftClicked;
        event EventHandler RightClicked;

        List<TrayMenuItem> MenuItems { set; get; }

        bool PreferDarkMode { set; get; }
    }
}

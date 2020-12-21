using System;
using System.Collections.Generic;

namespace GalaxyBudsClient.Interop.TrayIcon
{
    public interface ITrayIcon
    {
        event EventHandler<TrayMenuItem> TrayMenuItemSelected;
        event EventHandler LeftClicked;

        List<TrayMenuItem> MenuItems { set; get; }

        bool PreferDarkMode { set; get; }
    }
}

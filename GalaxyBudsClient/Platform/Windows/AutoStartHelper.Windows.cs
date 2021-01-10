using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace GalaxyBudsClient.Platform.Windows
{
    public class AutoStartHelper : IAutoStartHelper
    {
        public bool Enabled
        {
            get
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                return key?.GetValue("Galaxy Buds Client", null) != null;
            }
            set
            {
                if (value)
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key?.SetValue("Galaxy Buds Client", "\"" + Process.GetCurrentProcess().MainModule.FileName + "\" /StartMinimized");
                }
                else
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    key?.DeleteValue("Galaxy Buds Client");
                }
            }
        }
    }
}
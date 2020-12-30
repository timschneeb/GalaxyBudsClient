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
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                var result = key?.GetValue(curAssembly.GetName().Name, null);
                return result != null;
            }
            set
            {
                if (value)
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    key?.SetValue(curAssembly.GetName().Name, "\"" + curAssembly.Location + "\" /StartMinimized");
                }
                else
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    if (curAssembly?.GetName()?.Name is { } name)
                    {
                        key?.DeleteValue(name);
                    }
                }
            }
        }
    }
}
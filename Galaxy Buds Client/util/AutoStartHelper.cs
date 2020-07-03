using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Galaxy_Buds_Client.util
{
    class AutoStartHelper
    {
        public static bool Enabled
        {
            get
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                var result = key.GetValue(curAssembly.GetName().Name, null);
                return result != null;
            }
            set
            {
                if (value)
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    key.SetValue(curAssembly.GetName().Name, "\"" + curAssembly.Location + "\" /StartMinimized");
                }
                else
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    Assembly curAssembly = Assembly.GetExecutingAssembly();
                    key.DeleteValue(curAssembly.GetName().Name);
                }
            }
        }
    }
}

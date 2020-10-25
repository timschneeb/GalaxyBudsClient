using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.Properties;
using Microsoft.Win32;
using Sentry.Protocol;

namespace Galaxy_Buds_Client.util
{
    public static class DarkModeHelper
    {
        public static void Update()
        {
            switch (Settings.Default.DarkMode2)
            {
                case DarkMode.Unset:
                case DarkMode.Light:
                    SetDark(false);
                    break;
                case DarkMode.Dark:
                    SetDark(true);
                    break;
                case DarkMode.System:
                    try
                    {
                        string RegistryKey =
                            @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                        object light = Registry.GetValue(RegistryKey, "AppsUseLightTheme", true);
                        if (light == null)
                            break;

                        SetDark((int) light != 1);
                    }
                    catch (InvalidCastException ex)
                    {
                        Sentry.SentrySdk.AddBreadcrumb($"InvalidCastException: {ex.Data}", "darkModeHelperUpdate", level: BreadcrumbLevel.Warning);
                    }

                    break;
            }
        }

        private static void SetDark(bool b)
        {
            if (b)
            {
                Application.Current.Resources.MergedDictionaries[0].Source =
                    new Uri("pack://application:,,,/ui/themes/BrushesDark.xaml");

            }
            else
            {
                Application.Current.Resources.MergedDictionaries[0].Source =
                    new Uri("pack://application:,,,/ui/themes/Brushes.xaml");
            }

        }
    }
}

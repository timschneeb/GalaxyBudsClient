using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Galaxy_Buds_Client.util
{
    public static class DarkModeHelper
    {
        public static void SetState(bool b)
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

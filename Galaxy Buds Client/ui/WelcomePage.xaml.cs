using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class WelcomePage : BasePage
    {
        private MainWindow _mainWindow;

        public WelcomePage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        public override void OnPageShown()
        {
            _mainWindow.SetOptionsEnabled(false);
            DarkMode.Switch.SetChecked(Properties.Settings.Default.DarkMode);
        }

        public override void OnPageHidden()
        {

        }
        private void Continue_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.DeviceSelect);
        }

        private void DarkMode_OnSwitchToggled(object sender, bool e)
        {
            DarkModeHelper.SetState(e);
            Properties.Settings.Default.DarkMode = e;
            Properties.Settings.Default.Save();
        }
    }
}

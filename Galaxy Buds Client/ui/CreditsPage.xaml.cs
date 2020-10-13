using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using AutoUpdaterDotNET;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for CreditsPage.xaml
    /// </summary>
    public partial class CreditsPage : BasePage
    {
        private MainWindow _mainWindow;

        public CreditsPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        public override void OnPageShown()
        {
            Version.TextDetail = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public override void OnPageHidden()
        {
        }
        
        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

        private void Telegram_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://t.me/ThePBone");
        }

        private void GitHub_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://github.com/ThePBone/GalaxyBudsClient");
        }

        private void Website_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite("https://timschneeberger.me");
        }

        private void OpenWebsite(String url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

    }
}

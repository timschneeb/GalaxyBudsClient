using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using AutoUpdaterDotNET;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : BasePage
    {
        private MainWindow _mainWindow;
        private UpdateInfoEventArgs _args;

        public UpdatePage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        public void SetInfo(UpdateInfoEventArgs args)
        {
            _args = args;
            Title.Text = $"Version {args.CurrentVersion} of Galaxy Buds Manager has been released";
        }

        public override void OnPageShown()
        {
        }

        public override void OnPageHidden()
        {
        }
        
        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome(true);
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

        private void ViewChangelog_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenWebsite(_args.ChangelogURL);
        }

        private void Postpone_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.ReturnToHome(true);
        }

        private void Skip_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Properties.Settings.Default.SkippedUpdate = _args.CurrentVersion;
            Properties.Settings.Default.Save();
            _mainWindow.ReturnToHome(true);
        }

        private void Install_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AutoUpdater.DownloadUpdate(_args))
            {
                Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                Task.Delay(50).ContinueWith((task) =>
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                });
            }
        }
    }
}

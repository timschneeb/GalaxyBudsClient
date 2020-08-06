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
using System.Windows.Threading;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for ConnectionLostPage.xaml
    /// </summary>
    public partial class ConnectionLostPage : BasePage
    {
        private MainWindow _mainWindow;

        public event EventHandler RetryRequested;

        public ConnectionLostPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SetInfo("");
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            _mainWindow.SetOptionsEnabled(false);
        }

        public override void OnPageHidden()
        {
            Reset();
            _mainWindow.SetOptionsEnabled(true);
        }

        public void SetInfo(String info)
        {
            Dispatcher.Invoke(() =>
            {
                if (info == "")
                    info = Loc.GetString("connlost_noinfo");
                AdditionalInfo.Text = info;
            });

        }

        public void Reset()
        {
            Dispatcher.Invoke(() =>
            {
                LoadingSpinner.Visibility = Visibility.Hidden;
                LoadingSpinner.Stop();
                Retry.IsEnabled = true;
                Retry.Text = Loc.GetString("connlost_connect");
            });

        }

        private void Retry_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadingSpinner.Visibility = Visibility.Visible;
            LoadingSpinner.Start();
            Retry.IsEnabled = false;
            Retry.Text = Loc.GetString("connlost_connecting");

            Task.Delay(50).ContinueWith(t =>
            {
                RetryRequested?.Invoke(this, null);
            });

        }
    }
}

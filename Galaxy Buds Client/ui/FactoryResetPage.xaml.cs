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

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for FactoryResetPage.xaml
    /// </summary>
    public partial class FactoryResetPage : BasePage
    {
        private MainWindow _mainWindow;

        private readonly String Waiting = "Waiting for device response...";

        public FactoryResetPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            LoadingSpinner.Visibility = Visibility.Hidden;
            SPPMessageHandler.Instance.ResetResponse += InstanceOnResetResponse;
        }

        private void InstanceOnResetResponse(object sender, int e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadingSpinner.Visibility = Visibility.Hidden;
                LoadingSpinner.Stop();
                BackButton.Visibility = Visibility.Visible;
                FactoryReset.IsEnabled = true;
                FactoryReset.Text = "Confirm factory reset";

                if (e != 0)
                    MessageBox.Show($"Device returned error code: {e}\n" +
                                    $"Make sure both Earbuds are turned on and reachable.\n" +
                                    $"You might need to manually reconnect your Earbuds.", "Factory reset failed", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);

                _mainWindow.ReturnToHome();
            });
        }

        public override void OnPageShown()
        {
        }

        public override void OnPageHidden()
        {
            
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.System, true);
        }

        private void FactoryReset_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadingSpinner.Visibility = Visibility.Visible;
            LoadingSpinner.Start();
            BackButton.Visibility = Visibility.Hidden;
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FactoryReset());
            FactoryReset.Text = Waiting;
            FactoryReset.IsEnabled = false;
        }
    }
}

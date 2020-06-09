using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using InTheHand.Net;
using InTheHand.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for DeviceSelectPage.xaml
    /// </summary>
    public partial class DeviceSelectPage : BasePage
    {
        private MainWindow _mainWindow;

        private BluetoothAddress _address = null;

        public DeviceSelectPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
        }


        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            if (_address == null)
                BottomNavBar.Visibility = Visibility.Hidden;
        }

        public override void OnPageHidden()
        {
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.GoToPage(MainWindow.Pages.Welcome, true);
        }

        private void Scan_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dlg = new SelectBluetoothDeviceDialog();
            dlg.DeviceFilter = delegate (BluetoothDeviceInfo info)
            {
                return info.DeviceName.Contains("Buds");
            };
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }
            BluetoothDeviceInfo device = dlg.SelectedDevice;
            _address = device.DeviceAddress;

            DevName.TextDetail = device.DeviceName;
            DevAddress.TextDetail = BytesToMacString(_address.ToByteArrayBigEndian(), 0);
            BottomNavBar.Visibility = Visibility.Visible;
        }

        private String BytesToMacString(byte[] payload, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i13 = 0; i13 < 6; i13++)
            {
                if (i13 != 0)
                {
                    sb.Append(":");
                }
                sb.Append(((payload[i13 + startIndex] & 240) >> 4).ToString("X"));
                sb.Append((payload[i13 + startIndex] & 15).ToString("X"));
            }
            return sb.ToString();
        }

        private void Continue_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_address == null)
                MessageBox.Show("Please select a valid device first", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);

            Properties.Settings.Default.RegisteredDevice = BytesToMacString(_address?.ToByteArrayBigEndian(), 0);
            Properties.Settings.Default.Save();
            _mainWindow.FinalizeSetup();
        }
    }
}

using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.ui.element;
using Galaxy_Buds_Client.util.DynamicLocalization;
using System;
using System.Windows;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for AdvancedPage.xaml
    /// </summary>
    public partial class AdvancedPage : BasePage
    {
        private MainWindow _mainWindow;

        private bool _seamlessSupported;
        private bool _sidetoneSupported;

        public AdvancedPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BluetoothService.Instance.ActiveModel == Model.Buds)
                {
                    _seamlessSupported = e.Revision >= 3;
                }
                else if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                {
                    _seamlessSupported = e.Revision >= 11;
                    _sidetoneSupported = e.Revision >= 8;
                    Sidetone.Switch.SetChecked(e.SideToneEnabled);
                    GamingMode.Switch.SetChecked(e.AdjustSoundSync);
                }
                else if (BluetoothService.Instance.ActiveModel == Model.BudsLive)
                {
                    _seamlessSupported = e.Revision >= 11;
                    Passthrough.Switch.SetChecked(e.RelieveAmbient);
                    GamingMode.Switch.SetChecked(e.AdjustSoundSync);
                }
                SeamlessConnection.Switch.SetChecked(e.SeamlessConnectionEnabled);

                ResumeSensor.Switch.SetChecked(Settings.Default.ResumePlaybackOnSensor);
            });
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            SidetoneBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsPlus
                ? Visibility.Visible
                : Visibility.Collapsed;
            PassthroughBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsLive
                ? Visibility.Visible
                : Visibility.Collapsed;
            GamingModeBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.Buds
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public override void OnPageHidden()
        {

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

        private void SeamlessConnection_OnSwitchToggled(object sender, bool e)
        {
            if (!_seamlessSupported)
            {
                _mainWindow.ShowUnsupportedFeaturePage(
                    string.Format(
                        Loc.GetString("adv_required_firmware_later"), "R170XXU0ATF2"));
                return;
            }
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetSeamlessConnection(e));
        }

        private void ResumeSensor_OnSwitchToggled(object sender, bool e)
        {
            Settings.Default.ResumePlaybackOnSensor = e;
            Settings.Default.Save();
        }

        private void Sidetone_OnSwitchToggled(object sender, bool e)
        {
            if (!_sidetoneSupported)
            {
                _mainWindow.ShowUnsupportedFeaturePage(
                    string.Format(
                        Loc.GetString("adv_required_firmware_later"), "R175XXU0ATF2"));
                return;
            }
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetSidetoneEnabled(e));
        }

        private void GamingMode_OnSwitchToggled(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetAdjustSoundSyncEnabled(e));
        }

        private void Passthrough_OnSwitchToggled(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetRelieveAmbient(e));
        }
    }
}

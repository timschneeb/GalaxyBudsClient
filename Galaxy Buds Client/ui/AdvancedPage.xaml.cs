using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.ui.element;
using Galaxy_Buds_Client.util.DynamicLocalization;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for AdvancedPage.xaml
    /// </summary>
    public partial class AdvancedPage : BasePage
    {
        private MainWindow _mainWindow;

        private bool _seamlessSupported;

        public AdvancedPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.NoiseCancellingUpdated += InstanceOnNoiseCancellingUpdate;
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BluetoothService.Instance.ActiveModel == Model.Buds)
                {
                    _seamlessSupported = e.Revision >= 3;
                    SeamlessConnection.Switch.SetChecked(e.SeamlessConnectionEnabled);
                }
                else if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                {
                    _seamlessSupported = e.Revision >= 11;
                    SeamlessConnection.Switch.SetChecked(e.SeamlessConnectionEnabled);
                    Sidetone.Switch.SetChecked(e.SideToneEnabled);
                    GamingMode.Switch.SetChecked(e.AdjustSoundSync);
                }
                else if (BluetoothService.Instance.ActiveModel == Model.BudsLive)
                {
                    _seamlessSupported = e.Revision >= 1;
                    SeamlessConnection.Switch.SetChecked(e.SeamlessConnectionEnabled);
                    GamingMode.Switch.SetChecked(e.AdjustSoundSync);
                }
                ResumeSensor.Switch.SetChecked(Settings.Default.ResumePlaybackOnSensor);
            });
        }

        private void InstanceOnNoiseCancellingUpdate(object sender, bool e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BluetoothService.Instance.ActiveModel == Model.BudsLive)
                {
                    NoiseCancelling.Switch.SetChecked(e);
                }
            });
        }
        
        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            SidetoneBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsPlus
                ? Visibility.Visible
                : Visibility.Collapsed;
            GamingModeBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.Buds
                ? Visibility.Collapsed
                : Visibility.Visible;
            NoiseCancellingBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.BudsLive
                ? Visibility.Visible
                : Visibility.Collapsed;
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
                        Loc.GetString("adv_required_firmware_seamless_connection"), "R170XXU0ATF2"));
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
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetSidetoneEnabled(e));
        }

        private void GamingMode_OnSwitchToggled(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetAdjustSoundSyncEnabled(e));
        }

        private void NoiseCancelling_OnSwitchToggled(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetActiveNoiseCancellingEnabled(e));
            }
    }
}

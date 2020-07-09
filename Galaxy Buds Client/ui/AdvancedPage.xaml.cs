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
                else
                {
                    _seamlessSupported = e.Revision >= 11;
                    SeamlessConnection.Switch.SetChecked(e.SeamlessConnectionEnabled);
                    Sidetone.Switch.SetChecked(e.SideToneEnabled);
                    GamingMode.Switch.SetChecked(e.AdjustSoundSync);
                }
                ResumeSensor.Switch.SetChecked(Settings.Default.ResumePlaybackOnSensor);
            });
        }
        
        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            SidetoneBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.Buds
                ? Visibility.Collapsed
                : Visibility.Visible;
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
                _mainWindow.ShowUnsupportedFeaturePage("R170XXU0ATF2 or later");
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
    }
}

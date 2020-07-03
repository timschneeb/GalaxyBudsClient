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
                    SeamlessToggle.SetChecked(e.SeamlessConnectionEnabled);
                }
                else
                {
                    _seamlessSupported = e.Revision >= 11;
                    SeamlessToggle.SetChecked(e.SeamlessConnectionEnabled);
                    SidetoneToggle.SetChecked(e.SideToneEnabled);
                    GamingModeToggle.SetChecked(e.AdjustSoundSync);
                }
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

        private void SeamlessBorder_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_seamlessSupported)
            {
                _mainWindow.ShowUnsupportedFeaturePage("R170XXU0ATF2 or later");
                return;
            }
            SeamlessToggle.Toggle();
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetSeamlessConnection(SeamlessToggle.IsChecked));
        }

        private void Sidetone_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SidetoneToggle.Toggle();
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetSidetoneEnabled(SidetoneToggle.IsChecked));
        }

        private void GamingMode_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GamingModeToggle.Toggle();
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetAdjustSoundSyncEnabled(GamingModeToggle.IsChecked));
        }
    }
}

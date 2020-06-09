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
using Galaxy_Buds_Client.ui.element;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for AmbientSoundPage.xaml
    /// </summary>
    public partial class AmbientSoundPage : BasePage
    {
        private MainWindow _mainWindow;

        public AmbientSoundPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.AmbientEnabledUpdateResponse += InstanceOnAmbientEnabledUpdateResponse;
        }

        private void InstanceOnAmbientEnabledUpdateResponse(object sender, bool e)
        {
            Dispatcher.Invoke(() =>
            {
                AmbientToggle.SetChecked(e);
            });
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                AmbientToggle.SetChecked(e.AmbientSoundEnabled);
                VoiceFocusToggle.SetChecked(e.AmbientSoundMode == Constants.AmbientType.VoiceFocus);
                AmbientVolume.Value = e.AmbientSoundVolume;
            });
        }


        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
        }

        public override void OnPageHidden()
        {

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

        private void EnableAmbientBorder_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AmbientToggle.Toggle();
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetEnabled(AmbientToggle.IsChecked));
        }

        private void EnableVoiceFocus_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            VoiceFocusToggle.Toggle();
            var type = VoiceFocusToggle.IsChecked ? Constants.AmbientType.VoiceFocus : Constants.AmbientType.Default;
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetType(type));
        }

        private void AmbientVolume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetVolume((int)e.NewValue));
        }
    }
}

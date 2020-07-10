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
            SPPMessageHandler.Instance.OtherOption += InstanceOnOtherOption;
        }

        private void InstanceOnOtherOption(object sender, TouchOption.Universal e)
        {
            int action;
            if (e == TouchOption.Universal.OtherL)
            {
                if (Settings.Default.LeftCustomAction == -1)
                    return;
                action = Settings.Default.LeftCustomAction;
            }
            else if (e == TouchOption.Universal.OtherR)
            {
                if (Settings.Default.RightCustomAction == -1)
                    return;
                action = Settings.Default.RightCustomAction;
            }
            else
            {
                return;
            }

            switch ((CustomAction.Actions)action)
            {
                case CustomAction.Actions.AmbientVolumeUp:
                    Dispatcher.Invoke(() =>
                    {
                        AmbientToggle.SetChecked(true);
                        BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetEnabled(AmbientToggle.IsChecked));

                        AmbientVolume.Value++;
                        BluetoothService.Instance.SendAsync(
                            SPPMessageBuilder.Ambient.SetVolume((int) AmbientVolume.Value));
                    });
                    break;
                case CustomAction.Actions.AmbientVolumeDown:
                    Dispatcher.Invoke(() =>
                    {
                        AmbientToggle.SetChecked(true);
                        BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetEnabled(AmbientToggle.IsChecked));

                        AmbientVolume.Value--;
                        BluetoothService.Instance.SendAsync(
                                SPPMessageBuilder.Ambient.SetVolume((int) AmbientVolume.Value));
                    });
                    break;
            }
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
                AmbientVolume.Value = e.AmbientSoundVolume;

                if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
                {
                    ExtraLoud.Switch.SetChecked(e.ExtraHighAmbientEnabled);
                    AmbientVolume.Maximum = e.ExtraHighAmbientEnabled ? 3 : 2;
                }
                else
                {
                    VoiceFocusToggle.SetChecked(e.AmbientSoundMode == AmbientType.VoiceFocus);
                }
            });
        }


        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
            {
                VoiceFocusBorder.Visibility = Visibility.Collapsed;
                ExtraLoudBorder.Visibility = Visibility.Visible;
            }
            else
            {
                VoiceFocusBorder.Visibility = Visibility.Visible;
                ExtraLoudBorder.Visibility = Visibility.Collapsed;
                AmbientVolume.Maximum = 4;
            }
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
            var type = VoiceFocusToggle.IsChecked ? AmbientType.VoiceFocus : AmbientType.Default;
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetType(type));
        }

        private void AmbientVolume_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetVolume((int)e.NewValue));
        }
        
        private void ExtraLoud_OnSwitchToggled(object sender, bool e)
        {
            AmbientVolume.Maximum = e ? 3 : 2;
            if (e || AmbientVolume.Value >= 3)
                AmbientVolume.Value = AmbientVolume.Maximum;
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetExtraHighVolumeEnabled(e));
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Ambient.SetVolume((int)AmbientVolume.Value));
        }

        public void ToggleAmbient()
        {
            EnableAmbientBorder_OnMouseLeftButtonUp(this, null);
        }
    }
}

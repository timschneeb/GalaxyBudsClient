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
    /// Interaction logic for AmbientSoundPage.xaml
    /// </summary>
    public partial class EqualizerPage : BasePage
    {
        private MainWindow _mainWindow;

        private int newPresetValue = -1;

        public EqualizerPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
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
                case CustomAction.Actions.EnableEqualizer:
                    Dispatcher.Invoke(() =>
                    {
                        EQToggle.Toggle();
                        UpdateEqualizer();
                    });
                    break;
                case CustomAction.Actions.SwitchEqualizerPreset:
                    Dispatcher.Invoke(() =>
                    {
                        var newVal = PresetSlider.Value + 1;
                        if (newVal >= 5)
                            newVal = 0;

                        PresetSlider.Value = newVal;
                        EQToggle.SetChecked(true);
                        UpdateEqualizer();
                    });
                    break;
            }
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                if (BluetoothService.Instance.ActiveModel == Model.Buds)
                {
                    EQToggle.SetChecked(e.EqualizerEnabled);
                    DolbyToggle.SetChecked(e.EqualizerMode < 5);

                    int preset = e.EqualizerMode;
                    if (preset >= 5)
                        preset -= 5;

                    UpdateSlider(preset);
                }
                else
                {
                    EQToggle.SetChecked(e.EqualizerMode != 0);
                    if (e.EqualizerMode == 0)
                    {
                        UpdateSlider(2);
                    }
                    else
                    {
                        UpdateSlider(e.EqualizerMode - 1);
                    }
                }
            });
        }


        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                DolbyModeBorder.Visibility = Visibility.Visible;
            }
            else
            {
                DolbyModeBorder.Visibility = Visibility.Collapsed;
            }
            UpdateString();
        }

        public override void OnPageHidden()
        {

        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.ReturnToHome();
        }

        private void EnableEQ_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EQToggle.Toggle();
            UpdateEqualizer();
        }
        private void DolbyMode_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DolbyToggle.Toggle();
            UpdateEqualizer();
        }

        private void PresetSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (newPresetValue != e.NewValue)
            {
                UpdateSlider((int) PresetSlider.Value);
                UpdateEqualizer();
            }
            newPresetValue = -1;
        }

        private void UpdateEqualizer()
        {
            if (BluetoothService.Instance.ActiveModel == Model.BudsPlus)
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetEqualizer(EQToggle.IsChecked, (EqPreset)PresetSlider.Value, false));
            }
            else
            {
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetEqualizer(EQToggle.IsChecked, (EqPreset)PresetSlider.Value, DolbyToggle.IsChecked));
            }
        }

        private void UpdateSlider(int i)
        {
            newPresetValue = i;
            PresetSlider.Value = i;
            UpdateString();
        }

        private void UpdateString()
        {
            switch (PresetSlider.Value)
            {
                case 0:
                    PresetLabel.Content = Loc.GetString("eq_bass");
                    break;
                case 1:
                    PresetLabel.Content = Loc.GetString("eq_soft");
                    break;
                case 2:
                    PresetLabel.Content = Loc.GetString("eq_dynamic");
                    break;
                case 3:
                    PresetLabel.Content = Loc.GetString("eq_clear");
                    break;
                case 4:
                    PresetLabel.Content = Loc.GetString("eq_treble");
                    break;
            }
        }

        public void ToggleEqualizer()
        {
            EnableEQ_OnMouseLeftButtonUp(this, null);
        }
    }
}

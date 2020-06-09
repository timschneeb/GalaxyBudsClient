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
    public partial class EqualizerPage : BasePage
    {
        private MainWindow _mainWindow;

        public EqualizerPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                EQToggle.SetChecked(e.EqualizerEnabled);
                DolbyToggle.SetChecked(e.EqualizerMode < 5);

                int preset = e.EqualizerMode;
                if (preset >= 5)
                    preset -= 5;

                UpdateSlider(preset);
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
            UpdateSlider((int)PresetSlider.Value);
            UpdateEqualizer();
        }

        private void UpdateEqualizer()
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetEqualizer(EQToggle.IsChecked, (Constants.EqPreset)PresetSlider.Value, DolbyToggle.IsChecked));
        }

        private void UpdateSlider(int i)
        {
            PresetSlider.Value = i;
            switch (PresetSlider.Value)
            {
                case 0:
                    PresetLabel.Content = "Bass Boost";
                    break;
                case 1:
                    PresetLabel.Content = "Soft";
                    break;
                case 2:
                    PresetLabel.Content = "Dynamic";
                    break;
                case 3:
                    PresetLabel.Content = "Clear";
                    break;
                case 4:
                    PresetLabel.Content = "Treble Boost";
                    break;
            }
        }
    }
}

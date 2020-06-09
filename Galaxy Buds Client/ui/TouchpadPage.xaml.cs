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
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class TouchpadPage : BasePage
    {
        private MainWindow _mainWindow;

        public TouchpadPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;

            foreach (var child in LeftTouchMenu.Items)
            {
                if (child is MenuItem)
                {
                    (child as MenuItem).Click += LeftEventSetter_OnHandlerMenuItem_Click;
                }
            }
            foreach (var child in RightTouchMenu.Items)
            {
                if (child is MenuItem)
                {
                    (child as MenuItem).Click += RightEventSetter_OnHandlerMenuItem_Click;
                }
            }
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                LockToggle.SetChecked(e.TouchpadLock);
                LeftOption.TextDetail = OptionToString(e.TouchpadOptionL, Constants.Devices.L);
                RightOption.TextDetail = OptionToString(e.TouchpadOptionR, Constants.Devices.R);
            });
        }

        private String OptionToString(Constants.TouchOption b, Constants.Devices d)
        {
            switch (b)
            {
                case Constants.TouchOption.VoiceAssistant:
                    return "Voice Assistant (Android only)";
                case Constants.TouchOption.QuickAmbientSound:
                    return "Quick Ambient Sound";
                case Constants.TouchOption.Volume:
                    return d == Constants.Devices.L ? "Volume Down" : "Volume Up";
                case Constants.TouchOption.AmbientSound:
                    return "Ambient Sound";
                case Constants.TouchOption.SpotifySpotOn:
                    return "Spotify SpotOn (Android only)";
                /*case ExtendedStatusUpdateParser.TouchOption.OtherL:
                    return "Custom Action";
                case ExtendedStatusUpdateParser.TouchOption.OtherR:
                    return "Custom Action";*/
            }

            return "Unknown";
        }

        private Constants.TouchOption StringToOption(String s, Constants.Devices d)
        {
            if (s == "Voice Assistant (Android only)")
            {
                return Constants.TouchOption.VoiceAssistant;
            }
            else if (s == "Quick Ambient Sound")
            {
                return Constants.TouchOption.QuickAmbientSound;
            }
            else if (s.StartsWith("Volume"))
            {
                return Constants.TouchOption.Volume;
            }
            else if (s == "Ambient Sound")
            {
                return Constants.TouchOption.AmbientSound;
            }
            else if (s == "Spotify SpotOn (Android only)")
            {
                return Constants.TouchOption.SpotifySpotOn;
            }
            /*else if (s == "Custom Action")
            {
                return d == ExtendedStatusUpdateParser.Device.L ?
                    ExtendedStatusUpdateParser.TouchOption.OtherL : ExtendedStatusUpdateParser.TouchOption.OtherR;
            }*/

            Console.WriteLine("Touchpad: Unknown Touch Option");
            return Constants.TouchOption.Volume;
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

        private void LockTouchpadBorder_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LockToggle.Toggle();
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.Lock(LockToggle.IsChecked));
        }

        private void LeftOption_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.PlacementTarget = el;
            el.ContextMenu.Placement = PlacementMode.Bottom;
            el.ContextMenu.IsOpen = true;
        }

        private void RightOption_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.PlacementTarget = el;
            el.ContextMenu.Placement = PlacementMode.Bottom;
            el.ContextMenu.IsOpen = true;
        }

        private void ChangeTouchpad()
        {
            var left = StringToOption(LeftOption.TextDetail, Constants.Devices.L);
            var right = StringToOption(RightOption.TextDetail, Constants.Devices.R);
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOptions(left, right));
        }

        private void LeftEventSetter_OnHandlerMenuItem_Click(object sender, RoutedEventArgs e)
        {  
            var m = (MenuItem) e.Source;
            LeftOption.TextDetail = m.Header.ToString();
            ChangeTouchpad();
        }

        private void RightEventSetter_OnHandlerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var m = (MenuItem)e.Source;
            RightOption.TextDetail = m.Header.ToString();
            ChangeTouchpad();
        }
    }
}

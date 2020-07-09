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
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.ui.element;
using Galaxy_Buds_Client.util;

namespace Galaxy_Buds_Client.ui
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class TouchpadPage : BasePage
    {
        private MainWindow _mainWindow;

        private Devices _lastPressedMenu;
        private TouchOption.Universal _lastLeftOption;
        private TouchOption.Universal _lastRightOption;

        public TouchpadPage(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
        }

        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            Dispatcher.Invoke(() =>
            {
                LockToggle.SetChecked(e.TouchpadLock);

                _lastLeftOption = e.TouchpadOptionL;
                _lastRightOption = e.TouchpadOptionR;

                LeftOption.TextDetail = OptionToString(e.TouchpadOptionL, Devices.L);
                RightOption.TextDetail = OptionToString(e.TouchpadOptionR, Devices.R);
                DoubleTapVolume.Switch.SetChecked(e.OutsideDoubleTap);
            });
        }

        private ContextMenu GenerateMenu(UIElement target, Devices side)
        {
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = target;
            ctxMenu.Placement = PlacementMode.Bottom;

            Menu_AddItem(ctxMenu, "Custom Action...",
                side == Devices.L ? TouchOption.Universal.OtherL : TouchOption.Universal.OtherR, side);
            Menu_AddSeparator(ctxMenu);
            Menu_AddItem(ctxMenu, "Volume " + (side == Devices.L ? "Down" : "Up"), TouchOption.Universal.Volume, side);
            Menu_AddSeparator(ctxMenu);
            Menu_AddItem(ctxMenu, "Ambient Sound", TouchOption.Universal.AmbientSound, side);
            Menu_AddSeparator(ctxMenu);

            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                Menu_AddItem(ctxMenu, "Quick Ambient Sound", TouchOption.Universal.QuickAmbientSound, side);
                Menu_AddSeparator(ctxMenu);
            }

            Menu_AddItem(ctxMenu, "Voice Assistant (Android only)", TouchOption.Universal.VoiceAssistant, side);
            Menu_AddSeparator(ctxMenu);
            Menu_AddItem(ctxMenu, "Spotify SpotOn (Android only)", TouchOption.Universal.SpotifySpotOn, side);
            return ctxMenu;
        }

        private void Menu_AddItem(ContextMenu c, String header, TouchOption.Universal option, Devices side)
        {
            MenuItem m = new MenuItem();
            m.Header = header;
            m.Click += delegate
            {
                _lastPressedMenu = side;
                if (side == Devices.L)
                {
                    _lastLeftOption = option;
                }
                else
                {
                    _lastRightOption = option;
                }

                if (side == Devices.L && option == TouchOption.Universal.OtherL)
                    _mainWindow.GoToPage(MainWindow.Pages.TouchCustomAction);
                else if (side == Devices.R && option == TouchOption.Universal.OtherR)
                    _mainWindow.GoToPage(MainWindow.Pages.TouchCustomAction);
                else
                {
                    if (side == Devices.L)
                    {
                        LeftOption.TextDetail = m.Header.ToString();
                    }
                    else
                    {
                        RightOption.TextDetail = m.Header.ToString();
                    }
                }

                BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOptions(_lastLeftOption, _lastRightOption));
            };
            m.Style = (Style)FindResource("MenuItemStyle");
            c.Items.Add(m);
        }
        private void Menu_AddSeparator(ContextMenu c)
        {
            Separator s = new Separator();
            s.Style = (Style)FindResource("SeparatorStyle");
            c.Items.Add(s);
        }

        private String OptionToString(TouchOption.Universal b, Devices d)
        {
            switch (b)
            {
                case TouchOption.Universal.VoiceAssistant:
                    return "Voice Assistant (Android only)";
                case TouchOption.Universal.QuickAmbientSound:
                    return "Quick Ambient Sound";
                case TouchOption.Universal.Volume:
                    return d == Devices.L ? "Volume Down" : "Volume Up";
                case TouchOption.Universal.AmbientSound:
                    return "Ambient Sound";
                case TouchOption.Universal.SpotifySpotOn:
                    return "Spotify SpotOn (Android only)";
                case TouchOption.Universal.OtherL:
                    var currentCustomActionL = (Settings.Default.LeftCustomAction == -1) ? (CustomAction)null : 
                        new CustomAction((CustomAction.Actions) Settings.Default.LeftCustomAction, Settings.Default.LeftCustomActionParameter);
                    return "Custom Action: " + (currentCustomActionL == null ? "Not set" : currentCustomActionL.ToString());
                case TouchOption.Universal.OtherR:
                    var currentCustomActionR = (Settings.Default.RightCustomAction == -1) ? (CustomAction)null :
                        new CustomAction((CustomAction.Actions)Settings.Default.RightCustomAction, Settings.Default.RightCustomActionParameter);
                    return "Custom Action: " + (currentCustomActionR == null ? "Not set" : currentCustomActionR.ToString());
            }

            return "Unknown";
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            LeftOptionBorder.ContextMenu = GenerateMenu(LeftOptionBorder, Devices.L);
            RightOptionBorder.ContextMenu = GenerateMenu(RightOptionBorder, Devices.R);
            DoubleTapVolumeBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.Buds
                ? Visibility.Collapsed
                : Visibility.Visible;

            _mainWindow.CustomActionPage.Accept += CustomActionPageOnAccept;
        }

        public override void OnPageHidden()
        {
        }

        private void CustomActionPageOnAccept(object sender, CustomAction e)
        {
            String name;
            if (e == null)
            {
                name = "Not set";
            }
            else
            {
                name = e.ToString();
            }

            if (_lastPressedMenu == Devices.L)
            {
                LeftOption.TextDetail = $"Custom Action: {name}";
                if (e != null)
                {
                    Settings.Default.LeftCustomAction = (int) e.Action;
                    Settings.Default.LeftCustomActionParameter = e.Parameter;
                }
                else
                {
                    Settings.Default.RightCustomAction = -1;
                }
            }
            else
            {
                RightOption.TextDetail = $"Custom Action: {name}";
                if (e != null)
                {
                    Settings.Default.RightCustomAction = (int) e.Action;
                    Settings.Default.RightCustomActionParameter = e.Parameter;
                }
                else
                {
                    Settings.Default.RightCustomAction = -1;
                }
            }

            if (e != null)
            {
                Settings.Default.MinimizeTray = true;
                AutoStartHelper.Enabled = true;
            }

            Settings.Default.Save();
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.CustomActionPage.Accept -= CustomActionPageOnAccept;
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
            el.ContextMenu.IsOpen = true;
        }

        private void RightOption_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border el = (Border)sender;
            el.ContextMenu.IsOpen = true;
        }
        
        private void DoubleTapVolume_OnSwitchToggled(object sender, bool e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOutsideDoubleTapEnabled(e));
        }
    }
}

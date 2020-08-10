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
using Galaxy_Buds_Client.util.DynamicLocalization;

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

            Menu_AddItem(ctxMenu, Loc.GetString("touchoption_custom"),
                side == Devices.L ? TouchOption.Universal.OtherL : TouchOption.Universal.OtherR, side);
            Menu_AddSeparator(ctxMenu);

            Menu_AddItem(ctxMenu,
                side == Devices.L ? Loc.GetString("touchoption_volume_down") : Loc.GetString("touchoption_volume_up"),
                TouchOption.Universal.Volume, side);
            Menu_AddSeparator(ctxMenu);

            Menu_AddItem(ctxMenu, Loc.GetString("touchoption_ambientsound"), TouchOption.Universal.AmbientSound, side);
            Menu_AddSeparator(ctxMenu);

            if (BluetoothService.Instance.ActiveModel == Model.Buds)
            {
                Menu_AddItem(ctxMenu, Loc.GetString("touchoption_quickambientsound"), TouchOption.Universal.QuickAmbientSound, side);
                Menu_AddSeparator(ctxMenu);
            }

            Menu_AddItem(ctxMenu, Loc.GetString("touchoption_voice"), TouchOption.Universal.VoiceAssistant, side);
            Menu_AddSeparator(ctxMenu);
            Menu_AddItem(ctxMenu, Loc.GetString("touchoption_spotify"), TouchOption.Universal.SpotifySpotOn, side);
            return ctxMenu;
        }

        private void Menu_AddItem(ContextMenu c, String header, TouchOption.Universal option, Devices side)
        {
            MenuItem m = new MenuItem();
            m.Header = header;
            m.Click += delegate
            {
                _lastPressedMenu = side;

                if (side == Devices.L && option == TouchOption.Universal.OtherL)
                {
                    int act = Settings.Default.LeftCustomAction;
                    _mainWindow.CustomActionPage.CurrentSide = Devices.L;

                    _mainWindow.CustomActionPage.Action.TextDetail = act < 0 ? Loc.GetString("touchoption_custom_null") :
                        new CustomAction((CustomAction.Actions) act,
                            Settings.Default.LeftCustomActionParameter).ToLongString();
                    _mainWindow.GoToPage(MainWindow.Pages.TouchCustomAction);
                }
                else if (side == Devices.R && option == TouchOption.Universal.OtherR)
                {
                    int act = Settings.Default.RightCustomAction;
                    _mainWindow.CustomActionPage.CurrentSide = Devices.R;

                    _mainWindow.CustomActionPage.Action.TextDetail = act < 0 ? Loc.GetString("touchoption_custom_null") :
                        new CustomAction((CustomAction.Actions)act,
                            Settings.Default.RightCustomActionParameter).ToLongString();
                    _mainWindow.GoToPage(MainWindow.Pages.TouchCustomAction);
                }
                else
                {
                    if (side == Devices.L)
                    {
                        _lastLeftOption = option;
                        LeftOption.TextDetail = m.Header.ToString();

                        Settings.Default.LeftCustomAction = -1;
                        Settings.Default.LeftCustomActionParameter = "";
                        Settings.Default.Save();
                    }
                    else
                    {
                        _lastRightOption = option;
                        RightOption.TextDetail = m.Header.ToString();

                        Settings.Default.RightCustomAction = -1;
                        Settings.Default.RightCustomActionParameter = "";
                        Settings.Default.Save();
                    }
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOptions(_lastLeftOption, _lastRightOption));
                }
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
                    return Loc.GetString("touchoption_voice");
                case TouchOption.Universal.QuickAmbientSound:
                    return Loc.GetString("touchoption_quickambientsound");
                case TouchOption.Universal.Volume:
                    return d == Devices.L ?
                        Loc.GetString("touchoption_volume_down") :
                        Loc.GetString("touchoption_volume_up");
                case TouchOption.Universal.AmbientSound:
                    return Loc.GetString("touchoption_ambientsound");
                case TouchOption.Universal.SpotifySpotOn:
                    return Loc.GetString("touchoption_spotify");
                case TouchOption.Universal.OtherL:
                    var currentCustomActionL = (Settings.Default.LeftCustomAction == -1) ? (CustomAction)null :
                        new CustomAction((CustomAction.Actions)Settings.Default.LeftCustomAction, Settings.Default.LeftCustomActionParameter);
                    return Loc.GetString("touchoption_custom_prefix") + " " + (currentCustomActionL == null ? Loc.GetString("touchoption_custom_null") : currentCustomActionL.ToString());
                case TouchOption.Universal.OtherR:
                    var currentCustomActionR = (Settings.Default.RightCustomAction == -1) ? (CustomAction)null :
                        new CustomAction((CustomAction.Actions)Settings.Default.RightCustomAction, Settings.Default.RightCustomActionParameter);
                    return Loc.GetString("touchoption_custom_prefix") + " " + (currentCustomActionR == null ? Loc.GetString("touchoption_custom_null") : currentCustomActionR.ToString());
            }

            return Loc.GetString("touchoption_custom_unknown");
        }

        public override void OnPageShown()
        {
            LoadingSpinner.Visibility = Visibility.Hidden;
            LeftOptionBorder.ContextMenu = GenerateMenu(LeftOptionBorder, Devices.L);
            RightOptionBorder.ContextMenu = GenerateMenu(RightOptionBorder, Devices.R);
            DoubleTapVolumeBorder.Visibility = BluetoothService.Instance.ActiveModel == Model.Buds
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (_lastLeftOption == TouchOption.Universal.OtherL)
            {
                 LeftOption.TextDetail =
                     $"{Loc.GetString("touchoption_custom_prefix")} {new CustomAction((CustomAction.Actions)Settings.Default.LeftCustomAction, Settings.Default.LeftCustomActionParameter).ToString()}";
            }
            else
            {
                LeftOption.TextDetail = OptionToString(_lastLeftOption, Devices.L);
            }
            if (_lastRightOption == TouchOption.Universal.OtherR)
            {
                RightOption.TextDetail =
                    $"{Loc.GetString("touchoption_custom_prefix")} {new CustomAction((CustomAction.Actions) Settings.Default.RightCustomAction, Settings.Default.RightCustomActionParameter).ToString()}";
            }
            else
            {
                RightOption.TextDetail = OptionToString(_lastRightOption, Devices.R);
            }

            _mainWindow.CustomActionPage.Accept += CustomActionPageOnAccept;
        }

        public override void OnPageHidden()
        {
        }

        private void CustomActionPageOnAccept(object sender, CustomAction e)
        {
            if (e == null)
            {
                return;
            }

            if (_lastPressedMenu == Devices.L)
            {
                _lastLeftOption = TouchOption.Universal.OtherL;
                LeftOption.TextDetail = $"{Loc.GetString("touchoption_custom_prefix")} {e}";
                Settings.Default.LeftCustomAction = (int)e.Action;
                Settings.Default.LeftCustomActionParameter = e.Parameter;
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOptions(TouchOption.Universal.OtherL, _lastRightOption));
            }
            else
            {
                _lastRightOption = TouchOption.Universal.OtherR;
                RightOption.TextDetail = $"{Loc.GetString("touchoption_custom_prefix")} {e}";
                Settings.Default.RightCustomAction = (int)e.Action;
                Settings.Default.RightCustomActionParameter = e.Parameter;
                BluetoothService.Instance.SendAsync(SPPMessageBuilder.Touch.SetOptions(_lastLeftOption, TouchOption.Universal.OtherR));
            }

            Settings.Default.MinimizeTray = true;
            AutoStartHelper.Enabled = true;
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

        public void ToggleTouchlock()
        {
            LockTouchpadBorder_OnMouseLeftButtonUp(this, null);
        }
    }
}

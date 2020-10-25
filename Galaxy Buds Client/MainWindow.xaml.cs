using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using AutoUpdaterDotNET;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.ui;
using Galaxy_Buds_Client.ui.basewindow;
using Galaxy_Buds_Client.ui.devmode;
using Galaxy_Buds_Client.ui.dialog;
using Galaxy_Buds_Client.util;
using Galaxy_Buds_Client.util.DynamicLocalization;
using Hardcodet.Wpf.TaskbarNotification;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using Sentry;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;

namespace Galaxy_Buds_Client
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : SWWindow
    {
        private readonly MainPage _mainPage;
        private readonly SystemPage _systemPage;
        private readonly SelfTestPage _selfTestPage;
        private readonly FactoryResetPage _factoryResetPage;
        private readonly FindMyGearPage _findMyGearPage;
        private readonly TouchpadPage _touchpadPage;
        private readonly CustomActionPage _customActionPage;
        private readonly AmbientSoundPage _ambientSoundPage;
        private readonly EqualizerPage _equalizerPage;
        private readonly ConnectionLostPage _connectionLostPage;
        private readonly DeviceSelectPage _deviceSelectPage;
        private readonly SettingPage _settingPage;
        private readonly UpdatePage _updatePage;
        private readonly AdvancedPage _advancedPage;
        private readonly UnsupportedFeaturePage _unsupportedFeaturePage;
        private readonly PopupSettingPage _popupSettingPage;

        public enum Pages
        {
            Home,
            FindMyGear,
            Touch,
            TouchCustomAction,
            AmbientSound,
            Equalizer,
            System,
            Credits,
            SelfTest,
            FactoryReset,
            NoConnection,
            Welcome,
            DeviceSelect,
            Settings,
            SettingsPopup,
            Advanced,
            UnsupportedFeature,
            UpdateAvailable
        }

        private BluetoothAddress _address;
        private bool _isManualUpdate;
        private bool _restrictOptionsMenu;
        private readonly TaskbarIcon _tbi;
        private WearStates _previousWearState = WearStates.Both;
        private int _previousTrayBL = -1;
        private int _previousTrayBR = -1;
        private int _previousTrayBC = -1;

        private bool _popupShownCurrentSession;
        private ArrayList _previousBudsPopups = new ArrayList();

        public CustomActionPage CustomActionPage => _customActionPage;

        public MainWindow()
        {
            if (Settings.Default.DarkMode2 == DarkMode.Unset)
            {
                Settings.Default.DarkMode2 = (DarkMode)Convert.ToInt32(Settings.Default.DarkMode);
                Settings.Default.Save();
            }

            DarkModeHelper.Update();

            _mainPage = new MainPage(this);
            _systemPage = new SystemPage(this);
            _selfTestPage = new SelfTestPage(this);
            _factoryResetPage = new FactoryResetPage(this);
            _findMyGearPage = new FindMyGearPage(this);
            _touchpadPage = new TouchpadPage(this);
            _customActionPage = new CustomActionPage(this);
            _ambientSoundPage = new AmbientSoundPage(this);
            _equalizerPage = new EqualizerPage(this);
            _connectionLostPage = new ConnectionLostPage(this);
            _deviceSelectPage = new DeviceSelectPage(this);
            _settingPage = new SettingPage(this);
            _updatePage = new UpdatePage(this);
            _advancedPage = new AdvancedPage(this);
            _unsupportedFeaturePage = new UnsupportedFeaturePage(this);
            _popupSettingPage = new PopupSettingPage(this);

            InitializeComponent();

            _tbi = new TaskbarIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/icon_white.ico"))?.Stream;
            _tbi.Icon = new Icon(iconStream);
            _tbi.ToolTipText = "Galaxy Buds Manager";
            _tbi.TrayLeftMouseDown += Tray_OnTrayLeftMouseDown;
            _tbi.TrayContextMenuOpen += TbiOnTrayContextMenuOpen;
            _tbi.TrayRightMouseDown += TbiOnTrayRightMouseDown;
            GenerateTrayContext(-1, -1, -1);

            SPPMessageHandler.Instance.AnyMessageReceived += InstanceOnAnyMessageReceived;
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += InstanceOnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption += InstanceOnOtherOption;
            SPPMessageHandler.Instance.GetAllDataResponse += Instance_GetAllDataResponse;
            BluetoothService.Instance.MessageReceived += SPPMessageHandler.Instance.MessageReceiver;
            BluetoothService.Instance.InvalidDataException += InstanceOnInvalidDataException;
            BluetoothService.Instance.SocketException += InstanceOnSocketException;
            BluetoothService.Instance.PlatformNotSupportedException += InstanceOnPlatformNotSupportedException;
            BluetoothService.Instance.CreateClient();

            Closing += OnClosing;
            OptionsClicked += OnOptionsClicked;
            _mainPage.MainMenuClicked += MainPageOnMainMenuClicked;
            _connectionLostPage.RetryRequested += ConnectionLostPageOnRetryRequested;

            if (Loc.IsTranslatorModeEnabled())
            {
                new TranslatorMode(this).Show();
            }

            BluetoothAddress savedAddress = GetRegisteredDevice();
            if (savedAddress == null || GetRegisteredDeviceModel() == Model.NULL)
            {
                PageControl.TransitionType = PageTransitionType.Fade;
                PageControl.ShowPage(new WelcomePage(this));
            }
            else
            {
                PageControl.TransitionType = PageTransitionType.Fade;
                PageControl.ShowPage(_mainPage);
                _mainPage.SetLoaderVisible(true);
                Task.Delay(100).ContinueWith((_) =>
                {
                    BluetoothService.Instance.Connect(savedAddress, GetRegisteredDeviceModel());
                    CheckForUpdates(manual: false);
                });
                _address = savedAddress;
            }

            try
            {
                BluetoothWin32Events.GetInstance().InRange +=
                    delegate (object sender, BluetoothWin32RadioInRangeEventArgs args)
                    {
                        if (GetRegisteredDevice() != null && GetRegisteredDevice() == args.Device.DeviceAddress)
                        {
                            if (!BluetoothService.Instance.IsConnected && _connectionLostPage != null)
                            {
                                //Reset popup flag since we just connected
                                _popupShownCurrentSession = false;
                                ConnectionLostPageOnRetryRequested(this, new EventArgs());
                            }
                        }
                    };
            }
            catch (Win32Exception e)
            {
                SentrySdk.AddBreadcrumb(e.Message, "bluetoothInRange", level: Sentry.Protocol.BreadcrumbLevel.Error);
                Console.WriteLine(@"CRITICAL: Unknown Win32 Bluetooth service error");
                Console.WriteLine(e);
            }
            catch (InvalidOperationException e)
            {
                SentrySdk.AddBreadcrumb(e.Message, "bluetoothInRange", level: Sentry.Protocol.BreadcrumbLevel.Error);
                Console.WriteLine(@"CRITICAL: Unknown Win32 Bluetooth service error");
                Console.WriteLine(e);
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Instance_GetAllDataResponse(object sender, DebugGetAllDataParser e)
        {
            Settings.Default.LastSwVersion = e.SoftwareVersion;
            Settings.Default.Save();
        }

        private void TbiOnTrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            GenerateTrayContext();
        }

        private void TbiOnTrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
            GenerateTrayContext();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());

            if (Settings.Default.MinimizeTray)
            {
                Hide();
                e.Cancel = true;
            }
            else
            {
                BluetoothService.Instance.Disconnect();
            }
        }

        /*
         * Device
         */
        public BluetoothAddress GetRegisteredDevice()
        {
            var isValid = BluetoothAddress.TryParse(Properties.Settings.Default.RegisteredDevice, out var savedAddress);
            return isValid ? savedAddress : null;
        }
        public Model GetRegisteredDeviceModel()
        {
            var model = Properties.Settings.Default.RegisteredDeviceModel;
            if (model == Model.NULL)
            {
                Console.WriteLine("Registered Model is null");
            }
            return model;
        }

        /*
         * Tray
         */
        private void GenerateTrayContext(int? bl = null, int? br = null, int? bcase = null)
        {
            if (bl != null)
            {
                _previousTrayBL = (int)bl;
            }
            if (br != null)
            {
                _previousTrayBR = (int)br;
            }
            if (bcase != null)
            {
                _previousTrayBC = (int)bcase;
            }

            Dispatcher.Invoke(() =>
            {
                ContextMenu ctxMenu = new ContextMenu();
                ctxMenu.Style = (Style)FindResource("SmallContextMenuStyle");

                int staticCount = 0;

                if (_previousTrayBL > 0)
                {
                    MenuItem left = new MenuItem();
                    left.Header = $"{Loc.GetString("left")}: {_previousTrayBL}%";
                    left.IsEnabled = false;
                    left.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(left);
                    staticCount++;
                }
                if (_previousTrayBR > 0)
                {
                    MenuItem right = new MenuItem();
                    right.Header = $"{Loc.GetString("right")}: {_previousTrayBR}%";
                    right.IsEnabled = false;
                    right.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(right);
                    staticCount++;
                }
                if (_previousTrayBC > 0)
                {
                    MenuItem c = new MenuItem();
                    c.Header = $"{Loc.GetString("case")}: {_previousTrayBC}%";
                    c.IsEnabled = false;
                    c.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(c);
                    staticCount++;
                }

                if (staticCount > 0)
                {

                    Menu_AddSeparator(ctxMenu);
                    MenuItem touchlockToggle = new MenuItem();
                    touchlockToggle.Header = _touchpadPage.LockToggle.IsChecked ? Loc.GetString("tray_unlock_touchpad") : Loc.GetString("tray_lock_touchpad");
                    touchlockToggle.Click += delegate
                    {
                        _touchpadPage.ToggleTouchlock();
                    };
                    touchlockToggle.Style = (Style)FindResource("SmallMenuItemStyle");
                    ctxMenu.Items.Add(touchlockToggle);

                    MenuItem equalizerToggle = new MenuItem();
                    equalizerToggle.Header = _equalizerPage.EQToggle.IsChecked ? Loc.GetString("tray_disable_eq") : Loc.GetString("tray_enable_eq");
                    equalizerToggle.Click += delegate
                    {
                        _equalizerPage.ToggleEqualizer();
                    };
                    equalizerToggle.Style = (Style)FindResource("SmallMenuItemStyle");
                    ctxMenu.Items.Add(equalizerToggle);

                    if (BluetoothService.Instance.ActiveModel != Model.BudsLive)
                    {
                        MenuItem ambientToggle = new MenuItem();
                        ambientToggle.Header = _ambientSoundPage.AmbientToggle.IsChecked ? Loc.GetString("tray_disable_ambient_sound") : Loc.GetString("tray_enable_ambient_sound");
                        ambientToggle.Click += delegate
                        {
                            _ambientSoundPage.ToggleAmbient();
                        };
                        ambientToggle.Style = (Style)FindResource("SmallMenuItemStyle");
                        ctxMenu.Items.Add(ambientToggle);
                    }
                    if (BluetoothService.Instance.ActiveModel == Model.BudsLive)
                    {
                        MenuItem ancToggle = new MenuItem();
                        ancToggle.Header = _mainPage.AncToggle.IsChecked ? Loc.GetString("tray_disable_anc") : Loc.GetString("tray_enable_anc");
                        ancToggle.Click += delegate
                        {
                            _mainPage.ToggleAnc();
                        };
                        ancToggle.Style = (Style)FindResource("SmallMenuItemStyle");
                        ctxMenu.Items.Add(ancToggle);
                    }

                    Menu_AddSeparator(ctxMenu);
                }

                MenuItem quit = new MenuItem();
                quit.Header = Loc.GetString("tray_quit");
                quit.Click += delegate
                {
                    var prev = Settings.Default.MinimizeTray;
                    Settings.Default.MinimizeTray = false;
                    this.Close();
                    Settings.Default.MinimizeTray = prev;
                };
                quit.Style = (Style)FindResource("SmallMenuItemStyle");
                ctxMenu.Items.Add(quit);

                _tbi.ContextMenu = ctxMenu;

            });
        }

        /*
         * Popup
         */
        public void ShowPopup(int bl, int br, int bc)
        {
            Dispatcher.Invoke(() =>
            {
                if (!this.IsActive && (bl > 0 || br > 0 || bc > 0)
                                   && Settings.Default.ConnectionPopupEnabled)
                {
                    ShowDemoPopup();
                }
            });
        }

        private void CloseAllPopups(object sender, EventArgs args)
        {
            foreach (BudsPopup popup in _previousBudsPopups)
            {
                Dispatcher.Invoke(() => { popup?.Quit(); });
                popup.ClickedEventHandler -= CloseAllPopups;
            }
            _previousBudsPopups.Clear();
        }

        private void NewPopup(Screen screen)
        {
            Dispatcher.Invoke(() =>
            {
                BudsPopup pop = new BudsPopup(screen,
                    BluetoothService.Instance.ActiveModel, _previousTrayBL,
                    _previousTrayBR, _previousTrayBC);
                pop.ClickedEventHandler += CloseAllPopups;
                pop.HideHeader = Settings.Default.ConnectionPopupCompact;
                pop.PopupPlacement = Settings.Default.ConnectionPopupPosition;
                pop.ShowWindowWithoutFocus();
                _previousBudsPopups.Add(pop);
            });
        }

        /**
         * Only for testing/demo use. Please call ShowPopup instead.
         */
        public void ShowDemoPopup()
        {
            CloseAllPopups(this, null);

            if (Settings.Default.ConnectionPopupPrimaryScreen)
            {
                NewPopup(Screen.PrimaryScreen);
            }
            else
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    NewPopup(screen);
                }
            }
        }

        private void Tray_OnTrayLeftMouseDown(object sender, RoutedEventArgs e)
        {

            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }

                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
                Show();
            }
        }

        /*
         * Events
         */
        private void InstanceOnStatusUpdate(object sender, StatusUpdateParser e)
        {
            GenerateTrayContext(e.BatteryL, e.BatteryR, e.BatteryCase);

            if (_previousWearState == WearStates.None && e.WearState != WearStates.None &&
                Settings.Default.ResumePlaybackOnSensor)
            {
                if (!AudioPlaybackDetection.IsWindowsPlayingSound())
                {
                    try
                    {
                        new InputSimulator().Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                    }
                    catch (Exception ex)
                    {
                        Sentry.SentrySdk.AddBreadcrumb(ex.Message, "inputsimulator", level: Sentry.Protocol.BreadcrumbLevel.Warning);
                    }
                    Console.WriteLine(@"[ResumePlaybackOnSensor] All criteria are met; emitting play/pause keypress");
                }
                else
                {
                    Console.WriteLine(@"[ResumePlaybackOnSensor] Windows appears to playback sound; do not emit a play/pause keypress");
                }
            }

            _previousWearState = e.WearState;
        }
        private void InstanceOnSocketException(object sender, SocketException e)
        {
            if (PageControl == null)
                return;
            if (_connectionLostPage == null)
                return;
            if (PageControl.CurrentPage != null && (PageControl.CurrentPage.GetType() == typeof(WelcomePage)
                                                    || PageControl.CurrentPage.GetType() == typeof(DeviceSelectPage)))
                return;

            if (e != null)
                try
                {
                    _connectionLostPage.SetInfo(e.Message);
                }
                catch (NullReferenceException)
                {
                    _connectionLostPage.SetInfo(Loc.GetString("connlost_noinfo"));
                }

            GenerateTrayContext(-1, -1, -1);

            _connectionLostPage.Reset();

            _popupShownCurrentSession = false;

            if (PageControl.CurrentPage == null)
            {
                Dispatcher.Invoke(() =>
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_connectionLostPage);
                });
            }
            else
            {
                if (PageControl.CurrentPage.GetType() != typeof(ConnectionLostPage)
                    && PageControl.CurrentPage.GetType() != typeof(WelcomePage)
                    && PageControl.CurrentPage.GetType() != typeof(DeviceSelectPage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        PageControl.TransitionType = PageTransitionType.Fade;
                        PageControl.ShowPage(_connectionLostPage);
                    });
                }
            }
        }
        private void InstanceOnPlatformNotSupportedException(object sender, PlatformNotSupportedException e)
        {
            Dispatcher.Invoke(() =>
            {
                var m = MessageBox.Show(Loc.GetString("nobluetoothdev"),
                        "Galaxy Buds Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            });
        }
        private void InstanceOnInvalidDataException(object sender, InvalidDataException e)
        {
            Dispatcher.Invoke(() =>
            {
                _popupShownCurrentSession = false;

                if (_mainPage == null)
                {
                    Task.Delay(500).ContinueWith(delegate
                    {
                        BluetoothService.Instance.Connect(GetRegisteredDevice(), GetRegisteredDeviceModel());
                    });
                    return;
                }

                GenerateTrayContext(-1, -1, -1);
                BluetoothService.Instance.Disconnect();
                _mainPage.SetWarning(true, $"{Loc.GetString("mainpage_corrupt_data")} ({e.Message})");
                Task.Delay(500).ContinueWith(delegate
                {
                    BluetoothService.Instance.Connect(GetRegisteredDevice(), GetRegisteredDeviceModel());
                    _mainPage.SetWarning(false);
                });
            });
        }
        private void ConnectionLostPageOnRetryRequested(object sender, EventArgs e)
        {
            if (PageControl == null)
                return;
            if (PageControl.CurrentPage != null && (PageControl.CurrentPage.GetType() == typeof(WelcomePage)
                || PageControl.CurrentPage.GetType() == typeof(DeviceSelectPage)))
                return;

            if (GetRegisteredDevice() == null)
            {
                Dispatcher.Invoke(() =>
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(new WelcomePage(this));
                });
                Console.WriteLine("ConnectionLostRetry: MAC is null");
                return;
            }

            GenerateTrayContext(-1, -1, -1);
            BluetoothService.Instance.Disconnect();
            BluetoothService.Instance.Connect(GetRegisteredDevice(), GetRegisteredDeviceModel());
            if (BluetoothService.Instance.IsConnected)
            {
                Dispatcher.Invoke(() =>
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_mainPage);
                });
            }
        }
        private void InstanceOnAnyMessageReceived(object sender, BaseMessageParser e)
        {
            _mainPage.SetWarning(false);
            _mainPage.SetLoaderVisible(false);
        }
        private void InstanceOnExtendedStatusUpdate(object sender, ExtendedStatusUpdateParser e)
        {
            //Debounce popup events
            if (!_popupShownCurrentSession)
            {
                ShowPopup(e.BatteryL, e.BatteryR, e.BatteryCase);
                _popupShownCurrentSession = true;
            }

            GenerateTrayContext(e.BatteryL, e.BatteryR, e.BatteryCase);
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetManagerInfo());
        }
        private void InstanceOnOtherOption(object sender, TouchOption.Universal e)
        {
            if (e == TouchOption.Universal.OtherL)
            {
                if (Settings.Default.LeftCustomAction == -1)
                    return;
                PerformCustomAction((CustomAction.Actions)Settings.Default.LeftCustomAction,
                    Settings.Default.LeftCustomActionParameter);
            }
            else if (e == TouchOption.Universal.OtherR)
            {
                if (Settings.Default.RightCustomAction == -1)
                    return;
                PerformCustomAction((CustomAction.Actions)Settings.Default.RightCustomAction,
                    Settings.Default.RightCustomActionParameter);
            }
        }

        private void PerformCustomAction(CustomAction.Actions action, String parameter)
        {
            switch (action)
            {
                case CustomAction.Actions.RunExternalProgram:
                    try
                    {
                        Process.Start(parameter);
                    }
                    catch (FileNotFoundException ex)
                    {
                        MessageBox.Show($"Failed to execute custom long-press action (launch external app).\nFile not found: '{ex.FileName}'",
                            "Galaxy Buds Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Win32Exception ex)
                    {
                        MessageBox.Show($"Failed to execute custom long-press action (launch external app). Detailed information:\n{ex.Message}",
                            "Galaxy Buds Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case CustomAction.Actions.Hotkey:
                    if (!parameter.Contains(";"))
                        return;

                    InputSimulator sim = new InputSimulator();
                    foreach (var keycode in parameter.Split(';')[1].Split(','))
                    {
                        sim.Keyboard.KeyDown((VirtualKeyCode)int.Parse(keycode));
                    }
                    foreach (var keycode in parameter.Split(';')[1].Split(','))
                    {
                        sim.Keyboard.KeyUp((VirtualKeyCode)int.Parse(keycode));
                    }
                    break;
            }
        }


        /*
         * Options
         */
        public void SetOptionsEnabled(bool e)
        {
            _restrictOptionsMenu = !e;
        }
        private void OnOptionsClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = btn;
            ctxMenu.Placement = PlacementMode.Bottom;

            if (!_restrictOptionsMenu)
            {
                MenuItem settingsDashboard = new MenuItem();
                settingsDashboard.Header = Loc.GetString("optionsmenu_settings");
                settingsDashboard.Click += delegate { GoToPage(Pages.Settings); };
                settingsDashboard.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(settingsDashboard);
                Menu_AddSeparator(ctxMenu);

                MenuItem refreshDashboard = new MenuItem();
                refreshDashboard.Header = Loc.GetString("optionsmenu_refresh");
                refreshDashboard.Click += delegate
                {
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
                };
                refreshDashboard.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(refreshDashboard);
                Menu_AddSeparator(ctxMenu);
            }

            MenuItem deregDevice = new MenuItem();
            deregDevice.Header = Loc.GetString("optionsmenu_deregister");
            deregDevice.Click += delegate
            {
                GenerateTrayContext(-1, -1, -1);
                _popupShownCurrentSession = false;
                BluetoothService.Instance.Disconnect();
                Properties.Settings.Default.RegisteredDevice = "";
                Properties.Settings.Default.RegisteredDeviceModel = Model.NULL;
                Properties.Settings.Default.Save();

                PageControl.TransitionType = PageTransitionType.Fade;
                PageControl.ShowPage(new WelcomePage(this));
            };
            deregDevice.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(deregDevice);

            Menu_AddSeparator(ctxMenu);
            MenuItem checkUpdateDashboard = new MenuItem();
            checkUpdateDashboard.Header = Loc.GetString("optionsmenu_update");
            checkUpdateDashboard.Click += delegate { CheckForUpdates(manual: true); };
            checkUpdateDashboard.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(checkUpdateDashboard);

            if (!_restrictOptionsMenu)
            {
                Menu_AddSeparator(ctxMenu);
                MenuItem credits = new MenuItem();
                credits.Header = Loc.GetString("optionsmenu_credits");
                credits.Click += delegate { GoToPage(Pages.Credits); };
                credits.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(credits);
            }

            ctxMenu.IsOpen = true;
        }
        private void Menu_AddSeparator(ContextMenu c)
        {
            Separator s = new Separator();
            s.Style = (Style)FindResource("SeparatorStyle");
            c.Items.Add(s);
        }

        /*
         * Navigation
         */
        public void GoToPage(Pages page, bool back = false)
        {
            PageControl.TransitionType = back ? PageTransitionType.SlideAndFadeReverse : PageTransitionType.SlideAndFade;
            if (Properties.Settings.Default.DisableSlideTransition)
                PageControl.TransitionType = PageTransitionType.Fade;
            switch (page)
            {
                case Pages.Home:
                    PageControl.ShowPage(_mainPage);
                    break;
                case Pages.System:
                    PageControl.ShowPage(_systemPage);
                    break;
                case Pages.SelfTest:
                    PageControl.ShowPage(_selfTestPage);
                    break;
                case Pages.FactoryReset:
                    PageControl.ShowPage(_factoryResetPage);
                    break;
                case Pages.FindMyGear:
                    PageControl.ShowPage(_findMyGearPage);
                    break;
                case Pages.Touch:
                    PageControl.ShowPage(_touchpadPage);
                    break;
                case Pages.AmbientSound:
                    PageControl.ShowPage(_ambientSoundPage);
                    break;
                case Pages.Equalizer:
                    PageControl.ShowPage(_equalizerPage);
                    break;
                case Pages.Credits:
                    PageControl.ShowPage(new CreditsPage(this));
                    break;
                case Pages.NoConnection:
                    PageControl.ShowPage(_connectionLostPage);
                    break;
                case Pages.Welcome:
                    PageControl.ShowPage(new WelcomePage(this));
                    break;
                case Pages.DeviceSelect:
                    PageControl.ShowPage(_deviceSelectPage);
                    break;
                case Pages.Settings:
                    PageControl.ShowPage(_settingPage);
                    break;
                case Pages.Advanced:
                    PageControl.ShowPage(_advancedPage);
                    break;
                case Pages.TouchCustomAction:
                    PageControl.ShowPage(_customActionPage);
                    break;
                case Pages.SettingsPopup:
                    PageControl.ShowPage(_popupSettingPage);
                    break;
                case Pages.UnsupportedFeature:
                    PageControl.ShowPage(_unsupportedFeaturePage);
                    break;
                case Pages.UpdateAvailable:
                    PageControl.ShowPage(_updatePage);
                    break;
            }
        }
        public void ShowUnsupportedFeaturePage(String requiredVersion)
        {
            _unsupportedFeaturePage.SetRequiredVersion(requiredVersion);
            PageControl.ShowPage(_unsupportedFeaturePage);
        }
        public void ReturnToHome(bool fade = false)
        {
            if (BluetoothService.Instance.IsConnected)
            {
                if (fade)
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_mainPage);
                }
                else
                    GoToPage(Pages.Home, true);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_connectionLostPage);
                });
            }
        }
        private void MainPageOnMainMenuClicked(object sender, MainPage.ClickEvents e)
        {
            switch (e)
            {
                case MainPage.ClickEvents.FindMyGear:
                    GoToPage(Pages.FindMyGear);
                    break;
                case MainPage.ClickEvents.Touch:
                    GoToPage(Pages.Touch);
                    break;
                case MainPage.ClickEvents.Ambient:
                    GoToPage(Pages.AmbientSound);
                    break;
                case MainPage.ClickEvents.System:
                    GoToPage(Pages.System);
                    break;
                case MainPage.ClickEvents.Equalizer:
                    GoToPage(Pages.Equalizer);
                    break;
                case MainPage.ClickEvents.Advanced:
                    GoToPage(Pages.Advanced);
                    break;

            }
        }
        public void FinalizeSetup()
        {
            BluetoothAddress savedAddress = GetRegisteredDevice();
            if (savedAddress != null || GetRegisteredDeviceModel() != Model.NULL)
            {
                _address = savedAddress;
                _popupShownCurrentSession = false;

                if (BluetoothService.Instance.IsConnected)
                    BluetoothService.Instance.Disconnect();

                if (_address != null)
                    BluetoothService.Instance.Connect(_address, GetRegisteredDeviceModel());

                PageControl.TransitionType = PageTransitionType.Fade;
                PageControl.ShowPage(_mainPage);
            }
        }

        /*
         * Updater
         */
        public void CheckForUpdates(bool manual)
        {
            AutoUpdater.Mandatory = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            _isManualUpdate = manual;
            AutoUpdater.Start("https://timschneeberger.me/updater/galaxybudsclient.xml");
        }
        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                if (args.IsUpdateAvailable)
                {

                    if (args.CurrentVersion == Settings.Default.SkippedUpdate && !_isManualUpdate)
                        return;

                    _updatePage.SetInfo(args);
                    var x = Properties.Settings.Default.SkippedUpdate;
                    Console.WriteLine(x);
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_updatePage);
                }
                else if (_isManualUpdate)
                {
                    MessageBox.Show(Loc.GetString("updater_noupdate"),
                        Loc.GetString("updater_noupdate_title"), MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _isManualUpdate = false;

            });
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetOptionsVisibility(Visibility.Visible);
        }
    }
}

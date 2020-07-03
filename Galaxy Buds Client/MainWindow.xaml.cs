using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AutoUpdaterDotNET;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.model;
using Galaxy_Buds_Client.model.Constants;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.Properties;
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.ui;
using Galaxy_Buds_Client.ui.basewindow;
using Hardcodet.Wpf.TaskbarNotification;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;

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
            Advanced
        }

        private BluetoothAddress _address;
        private bool _isManualUpdate;
        private bool _restrictOptionsMenu;
        private readonly TaskbarIcon _tbi;

        public CustomActionPage CustomActionPage => _customActionPage;

        public MainWindow()
        {
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

            InitializeComponent();

            BluetoothWin32Events.GetInstance().InRange +=
                delegate (object sender, BluetoothWin32RadioInRangeEventArgs args)
                {
                    if (GetRegisteredDevice() != null && GetRegisteredDevice() == args.Device.DeviceAddress)
                    {
                        if (!BluetoothService.Instance.IsConnected)
                            ConnectionLostPageOnRetryRequested(this, new EventArgs());
                    }
                };

            _tbi = new TaskbarIcon();
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/icon_white.ico"))?.Stream;
            _tbi.Icon = new Icon(iconStream);
            _tbi.ToolTipText = "Galaxy Buds Manager";
            _tbi.TrayLeftMouseDown += Tray_OnTrayLeftMouseDown;
            GenerateTrayContext();

            SPPMessageHandler.Instance.AnyMessageReceived += InstanceOnAnyMessageReceived;
            SPPMessageHandler.Instance.ExtendedStatusUpdate += InstanceOnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += InstanceOnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption += InstanceOnOtherOption;
            BluetoothService.Instance.MessageReceived += SPPMessageHandler.Instance.MessageReceiver;
            BluetoothService.Instance.InvalidDataException += InstanceOnInvalidDataException;
            BluetoothService.Instance.SocketException += InstanceOnSocketException;
            BluetoothService.Instance.PlatformNotSupportedException += InstanceOnPlatformNotSupportedException;
            BluetoothService.Instance.CreateClient();

            Closing += OnClosing;
            OptionsClicked += OnOptionsClicked;
            _mainPage.MainMenuClicked += MainPageOnMainMenuClicked;
            _connectionLostPage.RetryRequested += ConnectionLostPageOnRetryRequested;

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
        private void GenerateTrayContext(int bl = -1, int br = -1, int bcase = -1)
        {
            Dispatcher.Invoke(() =>
            {
                ContextMenu ctxMenu = new ContextMenu();
                ctxMenu.Style = (Style)FindResource("SmallContextMenuStyle");

                int staticCount = 0;

                if (bl > 0)
                {
                    MenuItem left = new MenuItem();
                    left.Header = $"Left: {bl}%";
                    left.IsEnabled = false;
                    left.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(left);
                    staticCount++;
                }
                if (br > 0)
                {
                    MenuItem right = new MenuItem();
                    right.Header = $"Right: {br}%";
                    right.IsEnabled = false;
                    right.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(right);
                    staticCount++;
                }
                if (bcase > 0)
                {
                    MenuItem c = new MenuItem();
                    c.Header = $"Case: {bcase}%";
                    c.IsEnabled = false;
                    c.Style = (Style)FindResource("SmallMenuItemStyle-Static");
                    ctxMenu.Items.Add(c);
                    staticCount++;
                }

                if (staticCount > 0)
                    Menu_AddSeparator(ctxMenu);

                MenuItem quit = new MenuItem();
                quit.Header = "Quit";
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
        }
        private void InstanceOnSocketException(object sender, SocketException e)
        {
            if (e != null)
                try
                {
                    _connectionLostPage.SetInfo(e.Message);
                }
                catch (NullReferenceException exception)
                {
                    _connectionLostPage.SetInfo("Failed to gather error information");
                }

            GenerateTrayContext();

            _connectionLostPage.Reset();
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
        private void InstanceOnPlatformNotSupportedException(object sender, PlatformNotSupportedException e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("No bluetooth adapter found.\nPlease enable bluetooth and try again.",
                        "Galaxy Buds Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            });
        }
        private void InstanceOnInvalidDataException(object sender, InvalidDataException e)
        {
            Dispatcher.Invoke(() =>
            {
                GenerateTrayContext();
                BluetoothService.Instance.Disconnect();
                _mainPage.SetWarning(true, $"Received corrupted data. Reconnecting... ({e.Message})");
                Task.Delay(500).ContinueWith(delegate
                {
                    BluetoothService.Instance.Connect(GetRegisteredDevice(), GetRegisteredDeviceModel());
                    _mainPage.SetWarning(false);
                });
            });
        }
        private void ConnectionLostPageOnRetryRequested(object sender, EventArgs e)
        {
            if (PageControl.CurrentPage.GetType() == typeof(WelcomePage)
                || PageControl.CurrentPage.GetType() == typeof(DeviceSelectPage))
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

            GenerateTrayContext();
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
            GenerateTrayContext(e.BatteryL, e.BatteryR, e.BatteryCase);
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.SetManagerInfo());
        }
        private void InstanceOnOtherOption(object sender, TouchOption.Universal e)
        {
            int action;
            if (e == TouchOption.Universal.OtherL)
            {
                if (Settings.Default.LeftCustomAction == -1)
                    return;
                action = Settings.Default.LeftCustomAction;
                switch ((CustomAction.Actions)action)
                {
                    case CustomAction.Actions.RunExternalProgram:
                        Process.Start(Settings.Default.LeftCustomActionParameter);
                        break;
                }
            }
            else if (e == TouchOption.Universal.OtherR)
            {
                if (Settings.Default.RightCustomAction == -1)
                    return;
                action = Settings.Default.RightCustomAction;

                switch ((CustomAction.Actions)action)
                {
                    case CustomAction.Actions.RunExternalProgram:
                        Process.Start(Settings.Default.RightCustomActionParameter);
                        break;
                }
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
                settingsDashboard.Header = "Settings";
                settingsDashboard.Click += delegate { GoToPage(Pages.Settings); };
                settingsDashboard.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(settingsDashboard);
                Menu_AddSeparator(ctxMenu);

                MenuItem refreshDashboard = new MenuItem();
                refreshDashboard.Header = "Refresh dashboard";
                refreshDashboard.Click += delegate
                {
                    BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData());
                };
                refreshDashboard.Style = (Style)FindResource("MenuItemStyle");
                ctxMenu.Items.Add(refreshDashboard);
                Menu_AddSeparator(ctxMenu);
            }

            MenuItem deregDevice = new MenuItem();
            deregDevice.Header = "Deregister device";
            deregDevice.Click += delegate
            {
                GenerateTrayContext();
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
            checkUpdateDashboard.Header = "Check for updates";
            checkUpdateDashboard.Click += delegate { CheckForUpdates(manual: true); };
            checkUpdateDashboard.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(checkUpdateDashboard);

            if (!_restrictOptionsMenu)
            {
                Menu_AddSeparator(ctxMenu);
                MenuItem credits = new MenuItem();
                credits.Header = "Credits";
                credits.Click += delegate { GoToPage(Pages.Credits); };
                credits.Style = (Style) FindResource("MenuItemStyle");
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
                    MessageBox.Show("No updates are available at the moment.\n" +
                                    "Please try again later or check and subscribe the GitHub page for updates.",
                        "No updates", MessageBoxButton.OK, MessageBoxImage.Information);
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

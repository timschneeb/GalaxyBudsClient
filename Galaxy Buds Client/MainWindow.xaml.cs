using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AutoUpdaterDotNET;
using Galaxy_Buds_Client.message;
using Galaxy_Buds_Client.parser;
using Galaxy_Buds_Client.transition;
using Galaxy_Buds_Client.ui;
using Galaxy_Buds_Client.ui.basewindow;
using Galaxy_Buds_Client.ui.devmode;
using Galaxy_Buds_Client.ui.window;
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

        public MainWindow()
        {
            _mainPage = new MainPage(this);
            _systemPage = new SystemPage(this);
            _selfTestPage = new SelfTestPage(this);
            _factoryResetPage = new FactoryResetPage(this);
            _findMyGearPage = new FindMyGearPage(this);
            _touchpadPage = new TouchpadPage(this);
            _ambientSoundPage = new AmbientSoundPage(this);
            _equalizerPage = new EqualizerPage(this);
            _connectionLostPage = new ConnectionLostPage(this);
            _deviceSelectPage = new DeviceSelectPage(this);
            _settingPage = new SettingPage(this);
            _updatePage = new UpdatePage(this);
            _advancedPage = new AdvancedPage(this);
            _unsupportedFeaturePage = new UnsupportedFeaturePage(this);

            InitializeComponent();

            SPPMessageHandler.Instance.AnyMessageReceived += InstanceOnAnyMessageReceived;
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
            if (savedAddress == null)
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
                    BluetoothService.Instance.Connect(savedAddress);
                    CheckForUpdates(manual: false);
                });
                _address = savedAddress;
            }
        }

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

                    if (args.CurrentVersion == Properties.Settings.Default.SkippedUpdate && !_isManualUpdate)
                        return;

                    _updatePage.SetInfo(args); 
                    var x = Properties.Settings.Default.SkippedUpdate;
                    Console.WriteLine(x);
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_updatePage);
                }
                else if(_isManualUpdate)
                {
                    /*SWMessageWindow w = new SWMessageWindow("No updates are available at the moment.\n" +
                                                            "Please try again later or check and subscribe the GitHub page for updates.")
                    {
                        OptionsLabel = {Visibility = Visibility.Hidden}
                    };
                    w.ShowDialog();*/
                    MessageBox.Show("No updates are available at the moment.\n" +
                                    "Please try again later or check and subscribe the GitHub page for updates.",
                        "No updates", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _isManualUpdate = false;

            });
        }

        private void InstanceOnAnyMessageReceived(object sender, BaseMessageParser e)
        {
            _mainPage.SetWarning(false);
            _mainPage.SetLoaderVisible(false);
        }

        public void FinalizeSetup()
        {
            BluetoothAddress savedAddress = GetRegisteredDevice();
            if (savedAddress != null)
            {
                _address = savedAddress;

                if (BluetoothService.Instance.IsConnected)
                    BluetoothService.Instance.Disconnect();
                if (_address != null)
                    BluetoothService.Instance.Connect(_address);

                PageControl.TransitionType = PageTransitionType.Fade;
                PageControl.ShowPage(_mainPage);
            }
        }

        public BluetoothAddress GetRegisteredDevice()
        {
            var isValid = BluetoothAddress.TryParse(Properties.Settings.Default.RegisteredDevice, out var savedAddress);
            return isValid ? savedAddress : null;
        }


        private void ConnectionLostPageOnRetryRequested(object sender, EventArgs e)
        {
            BluetoothService.Instance.Disconnect();
            BluetoothService.Instance.Connect(GetRegisteredDevice());
            if (BluetoothService.Instance.IsConnected)
            {
                Dispatcher.Invoke(() =>
                {
                    PageControl.TransitionType = PageTransitionType.Fade;
                    PageControl.ShowPage(_mainPage);
                });
            }
        }

        private void InstanceOnSocketException(object sender, SocketException e)
        {
            _connectionLostPage.SetInfo(e.Message);
            _connectionLostPage.Reset();
            if (PageControl.CurrentPage != _connectionLostPage)
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
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            });
        }

        private void OnOptionsClicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            ContextMenu ctxMenu = new ContextMenu();
            ctxMenu.Style = (Style)FindResource("ContextMenuStyle");
            ctxMenu.PlacementTarget = btn;
            ctxMenu.Placement = PlacementMode.Bottom;

            MenuItem settingsDashboard = new MenuItem();
            settingsDashboard.Header = "Settings";
            settingsDashboard.Click += delegate { GoToPage(Pages.Settings); };
            settingsDashboard.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(settingsDashboard);
            Menu_AddSeparator(ctxMenu);
            MenuItem refreshDashboard = new MenuItem();
            refreshDashboard.Header = "Refresh dashboard";
            refreshDashboard.Click += delegate { BluetoothService.Instance.SendAsync(SPPMessageBuilder.Info.GetAllData()); };
            refreshDashboard.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(refreshDashboard);
            Menu_AddSeparator(ctxMenu);
            MenuItem reconnectDashboard = new MenuItem();
            reconnectDashboard.Header = "Reconnect device";
            reconnectDashboard.Click += delegate
            {
                BluetoothService.Instance.Disconnect();
                BluetoothService.Instance.Connect(GetRegisteredDevice());
            };
            reconnectDashboard.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(reconnectDashboard);
            Menu_AddSeparator(ctxMenu);
            MenuItem deregDevice = new MenuItem();
            deregDevice.Header = "Deregister device";
            deregDevice.Click += delegate
            {
                BluetoothService.Instance.Disconnect();
                Properties.Settings.Default.RegisteredDevice = "";
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
            Menu_AddSeparator(ctxMenu);
            MenuItem credits = new MenuItem();
            credits.Header = "Credits";
            credits.Click += delegate { GoToPage(Pages.Credits); };
            credits.Style = (Style)FindResource("MenuItemStyle");
            ctxMenu.Items.Add(credits);

            ctxMenu.IsOpen = true;
        }

        private void Menu_AddSeparator(ContextMenu c)
        {
            Separator s = new Separator();
            s.Style = (Style)FindResource("SeparatorStyle");
            c.Items.Add(s);
        }

        private void InstanceOnInvalidDataException(object sender, InvalidDataException e)
        {
            Dispatcher.Invoke(() =>
            {
                BluetoothService.Instance.Disconnect();
                _mainPage.SetWarning(true, $"Received corrupted data. Reconnecting... ({e.Message})");
                Task.Delay(500).ContinueWith(delegate
                {
                    BluetoothService.Instance.Connect(GetRegisteredDevice());
                    _mainPage.SetWarning(false);
                });
            });
        }

        public void SetOptionsEnabled(bool e)
        {
            OptionsLabel.IsEnabled = e;
            OptionsLabel.Visibility = e ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            BluetoothService.Instance.SendAsync(SPPMessageBuilder.FindMyGear.Stop());
            BluetoothService.Instance.Disconnect();
        }

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetOptionsVisibility(Visibility.Visible);
        }
    }
}

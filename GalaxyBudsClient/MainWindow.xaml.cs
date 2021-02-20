using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Win32;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Developer;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Interface.Transition;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Windows;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using NetSparkleUpdater.Enums;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;
using MessageBox = GalaxyBudsClient.Interface.Dialogs.MessageBox;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient
{
    public sealed class MainWindow : Window
    {
        public readonly HomePage HomePage = new HomePage();
        public readonly UnsupportedFeaturePage UnsupportedFeaturePage = new UnsupportedFeaturePage();
        public readonly CustomTouchActionPage CustomTouchActionPage = new CustomTouchActionPage();
        public readonly ConnectionLostPage ConnectionLostPage = new ConnectionLostPage();
        public readonly UpdatePage UpdatePage = new UpdatePage();
        public readonly UpdateProgressPage UpdateProgressPage = new UpdateProgressPage();
        public readonly DeviceSelectionPage DeviceSelectionPage = new DeviceSelectionPage();
        
        private DevOptions? _devOptions;

        private readonly CustomTitleBar _titleBar;
        private BudsPopup _popup;
        private DateTime _lastPopupTime = DateTime.UtcNow;
        private WearStates _lastWearState = WearStates.Both;

        private bool _firstShow = true;
        
        public bool OverrideMinimizeTray { set; get; }
        public bool DisableApplicationExit { set; get; }
        
        public PageContainer Pager { get; }
        
        private static MainWindow? _instance;
        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    var platform = AvaloniaLocator.Current.GetService<IWindowingPlatform>();
                    if (platform == null)
                    {
                        throw new Exception("Could not CreateWindow(): IWindowingPlatform is not registered.");
                    }

                    IWindowImpl impl;
                    if (platform.GetType().Name == "Win32Platform")
                    {
                        Log.Debug("MainWindow.Instance: Initializing window with WndProc proxy");
                        impl = new Win32ProcWindowImpl();
                    }
                    else
                    {
                        Log.Debug("MainWindow.Instance: Initializing window with default WindowImpl");
                        impl = platform.CreateWindow();
                    }
                    
                    _instance = new MainWindow(impl);
                }

                return _instance;
            }
        }

        public static bool IsReady()
        {
            return _instance != null;
        }

        public static void Kill()
        {
            _instance = null;
        }

        /* Public constructor for XAMLIL only */
        public MainWindow()
        {
            /* Init with dummy objects */
            _titleBar = new CustomTitleBar();
            _popup = new BudsPopup();
            Pager = new PageContainer();
            Log.Error("MainWindow: Initialized without modified PlatformImpl. Features making use of legacy Win32 APIs may be unavailable.");
        }
        
        public MainWindow(IWindowImpl impl) : base(impl)
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
            
            Pager = this.FindControl<PageContainer>("Container");

            Pager.RegisterPages(HomePage, new AmbientSoundPage(), new FindMyGearPage(), new FactoryResetPage(),
                new CreditsPage(), new TouchpadPage(), new EqualizerPage(), new AdvancedPage(), new NoiseProPage(),
                new SystemPage(), new SelfTestPage(), new SettingsPage(), new PopupSettingsPage(),
                ConnectionLostPage, CustomTouchActionPage, DeviceSelectionPage, new SystemInfoPage(),
                new WelcomePage(), UnsupportedFeaturePage, UpdatePage, UpdateProgressPage, new SystemCoredumpPage(),
                new HotkeyPage());

            _titleBar = this.FindControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => PlatformImpl?.BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) => _titleBar.OptionsButton.ContextMenu.Open(_titleBar.OptionsButton);
            _titleBar.ClosePressed += (sender, args) =>
            {
                if (SettingsProvider.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
                {
                    Log.Information("MainWindow.TitleBar: Close requested, minimizing to tray bar");
                    Hide();
                }
                else
                { 
                    Log.Information("MainWindow.TitleBar: Close requested, exiting app");
                    Close();
                }
            };

                _popup = new BudsPopup();

            BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
            BluetoothImpl.Instance.Disconnected += OnDisconnected;
            BluetoothImpl.Instance.Connected += OnConnected;

            SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += OnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption += HandleOtherTouchOption;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            NotifyIconImpl.Instance.LeftClicked += TrayIcon_OnLeftClicked;
            TrayManager.Instance.Rebuild();
            
            Pager.PageSwitched += (sender, pages) => BuildOptionsMenu();
            Loc.LanguageUpdated += BuildOptionsMenu;
            BuildOptionsMenu();
            
            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                Task.Factory.StartNew(() => BluetoothImpl.Instance.ConnectAsync());
                Pager.SwitchPage(AbstractPage.Pages.Home);
            }
            else
            {
                Pager.SwitchPage(AbstractPage.Pages.Welcome);
            }
            
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Args.Contains("/StartMinimized") && PlatformUtils.SupportsTrayIcon)
                {
                    WindowState = WindowState.Minimized;
                }
            }
        }

        private async void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            switch (e)
            {
                case EventDispatcher.Event.PairingMode:
                    await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.UNK_PAIRING_MODE);
                    break;
                case EventDispatcher.Event.ToggleManagerVisibility:
                    if (!IsVisible)
                    {
                        BringToFront();
                    }
                    else
                    {
                        Hide();
                    }
                    break;
                case EventDispatcher.Event.Connect:
                    if (!BluetoothImpl.Instance.IsConnected)
                    {
                        await BluetoothImpl.Instance.ConnectAsync();
                    }
                    break;
                case EventDispatcher.Event.ShowBatteryPopup:
                    ShowPopup(ignoreRestrictions: true);
                    break;
            }
        }

        #region Window management
        protected override async void OnInitialized()
        {
            SingleInstanceWatcher.Activated += BringToFront;
            
            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                await Task.Delay(3000).ContinueWith((_) => UpdateManager.Instance.SilentCheck());
            }
            base.OnInitialized();
        }
        
        protected override async void OnClosing(CancelEventArgs e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            
            if (SettingsProvider.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
            {
                Hide();
                e.Cancel = true;
                Log.Debug("MainWindow.OnClosing: Termination cancelled");
            }
            else
            {
                Log.Debug("MainWindow.OnClosing: Now closing session");
            }
            
            base.OnClosing(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            HotkeyReceiverImpl.Reset();
            HotkeyReceiverImpl.Instance.Update(silent: true);
            
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Args.Contains("/StartMinimized") && PlatformUtils.SupportsTrayIcon && _firstShow)
                {
                    Log.Debug("MainWindow: Launched minimized.");
                    Hide();
                }
            }

            _firstShow = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            BluetoothImpl.Instance.BluetoothError -= OnBluetoothError;
            BluetoothImpl.Instance.Disconnected -= OnDisconnected;
            BluetoothImpl.Instance.Connected -= OnConnected;

            SPPMessageHandler.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate -= OnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption -= HandleOtherTouchOption;

            NotifyIconImpl.Instance.LeftClicked -= TrayIcon_OnLeftClicked;
            EventDispatcher.Instance.EventReceived -= OnEventReceived;

            Loc.LanguageUpdated -= BuildOptionsMenu;

            if (DisableApplicationExit)
            {
                return;
            }
            
            if(Application.Current.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Information("MainWindow: Shutting down normally");
                desktop.Shutdown();
            }
            else
            {
                Log.Information("MainWindow: Shutting down using Environment.Exit");
                Environment.Exit(0);
            }
        }

        public void BringToFront()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {  
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                
                if (PlatformUtils.IsLinux)
                {
                    Hide(); // Workaround for some Linux DMs
                }
                
                Show();
                
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            });
        }
        
        private void TrayIcon_OnLeftClicked(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsVisible)
                {
                    BringToFront();
                }
                else
                {
                    Hide();
                }
            });
        }
        #endregion

        #region Global Bluetooth callbacks
        private void OnStatusUpdate(object? sender, StatusUpdateParser e)
        {
            if (_lastWearState == WearStates.None &&
                e.WearState != WearStates.None && SettingsProvider.Instance.ResumePlaybackOnSensor)
            {
                MediaKeyRemoteImpl.Instance.Play();
            }

            _lastWearState = e.WearState;
        }

        private async void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            if (SettingsProvider.Instance.Popup.Enabled)
            {
                ShowPopup();
            }
            
            await MessageComposer.SetManagerInfo();
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            Pager.SwitchPage(AbstractPage.Pages.Home);
        }

        private void OnBluetoothError(object? sender, BluetoothException e)
        {
            switch (e.ErrorCode)
            {
                case BluetoothException.ErrorCodes.NoAdaptersAvailable:
                    new MessageBox()
                    {
                        Title = Loc.Resolve("error"),
                        Description = Loc.Resolve("nobluetoothdev")
                    }.ShowDialog(this);
                    break;
                default:
                    Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
                        ? AbstractPage.Pages.NoConnection
                        : AbstractPage.Pages.Welcome);
                    break;
            }
        }

        private void OnDisconnected(object? sender, string e)
        {
            Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
                ? AbstractPage.Pages.NoConnection
                : AbstractPage.Pages.Welcome);
        }

        private void HandleOtherTouchOption(object? sender, TouchOptions e)
        {
            ICustomAction action = e == TouchOptions.OtherL ?
                SettingsProvider.Instance.CustomActionLeft : SettingsProvider.Instance.CustomActionRight;

            switch (action.Action)
            {
                case CustomAction.Actions.Event:
                    EventDispatcher.Instance.Dispatch(Enum.Parse<EventDispatcher.Event>(action.Parameter), true);
                    break;
                case CustomAction.Actions.RunExternalProgram:
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = action.Parameter,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                    catch (FileNotFoundException ex)
                    {
                        new MessageBox()
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n" +
                                          $"File not found: '{ex.FileName}'"
                        }.Show(this);
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 13 && PlatformUtils.IsLinux)
                        {
                            new MessageBox()
                            {
                                Title = "Custom long-press action failed",
                                Description = $"Unable to launch external application.\n\n" +
                                              $"Insufficient permissions. Please add execute permissions for your user/group to this file.\n\n" +
                                              $"Run this command in a terminal: chmod +x \"{action.Parameter}\""
                            }.Show(this);
                        }
                        else
                        {
                            new MessageBox()
                            {
                                Title = "Custom long-press action failed",
                                Description = $"Unable to launch external application.\n\n" +
                                              $"Detailed information:\n\n" +
                                              $"{ex.Message}"
                            }.Show(this);
                        }
                    }

                    break;
                case CustomAction.Actions.TriggerHotkey:
                    var keys = new List<Key>();
                    try
                    {
                        keys.AddRange(action.Parameter.Split(',').Select(Enum.Parse<Key>));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("CustomAction.HotkeyBroadcast: Cannot parse saved key-combo: " + ex.Message);
                        Log.Error("CustomAction.HotkeyBroadcast: Caused by combo: " + action.Parameter);
                        return;
                    }

                    HotkeyBroadcastImpl.Instance.SendKeys(keys);
                    break;
            }
        }
        #endregion

        #region Options menu
        private void BuildOptionsMenu()
        {
            bool restricted = Pager.CurrentPage == AbstractPage.Pages.Welcome ||
                              Pager.CurrentPage == AbstractPage.Pages.DeviceSelect ||
                              !BluetoothImpl.Instance.RegisteredDeviceValid;

            var options = new Dictionary<string, EventHandler<RoutedEventArgs>?>()
            {
                [Loc.Resolve("optionsmenu_settings")] =
                    (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Settings),
                [Loc.Resolve("optionsmenu_refresh")] = async (sender, args) =>
                    await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.DEBUG_GET_ALL_DATA),
                [Loc.Resolve("optionsmenu_deregister")] = (sender, args) => BluetoothImpl.Instance.UnregisterDevice()
                    .ContinueWith((_) => Pager.SwitchPage(AbstractPage.Pages.Welcome))
            };

            if (restricted)
            {
                options.Clear();
            }

            options[Loc.Resolve("optionsmenu_update")] = async (sender, args) =>
            {
                var result = await UpdateManager.Instance.DoManualCheck();
                if (result != UpdateStatus.UpdateAvailable)
                {
                    await new MessageBox()
                    {
                        Title = Loc.Resolve("updater_noupdate_title"),
                        Description = Loc.Resolve("updater_noupdate"),
                    }.ShowDialog(this);
                }
            };
            options[Loc.Resolve("optionsmenu_credits")] = (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Credits);


            _titleBar.OptionsButton.ContextMenu = MenuFactory.BuildContextMenu(options);
        }
        #endregion

        #region Pages utils
        public void ShowDevTools()
        {
            _devOptions ??= new DevOptions();
            try
            {
                _devOptions.Show(this);
            }
            catch (InvalidOperationException)
            {
                _devOptions = new DevOptions();
                _devOptions.Show(this);
            }
        }

        public void ShowUnsupportedFeaturePage(string assertion)
        {
            UnsupportedFeaturePage.RequiredVersion = assertion;
            Pager.SwitchPage(AbstractPage.Pages.UnsupportedFeature);
        }

        public void ShowCustomActionSelection(Devices device)
        {
            CustomTouchActionPage.CurrentSide = device;
            Pager.SwitchPage(AbstractPage.Pages.TouchCustomAction);
        }
        #endregion

        public void ShowPopup(bool ignoreRestrictions = false)
        {
            DateTime now = DateTime.UtcNow;
            if ((now.Subtract(_lastPopupTime).TotalSeconds >= 5 && !IsActive) || ignoreRestrictions)
            {
                if (_popup.IsVisible)
                {
                    _popup.UpdateSettings();
                    _popup.RearmTimer();
                }

                try
                {
                    _popup.Show();
                }
                catch (InvalidOperationException)
                {
                    /* Window already closed down */
                    _popup = new BudsPopup();
                    _popup.Show();
                }
                _lastPopupTime = now;
            }

        }
    }
}
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
using Avalonia.Media.Imaging;
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
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
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
        private BudsPopup? _popup;
        private bool _popupShown = false;
        private WearStates _lastWearState = WearStates.Both;
        
        private bool _firstShow = true;
        
        public bool OverrideMinimizeTray { set; get; }
        public bool DisableApplicationExit { set; get; }

        public bool IsOptionsButtonVisible
        {
            get => _titleBar.OptionsButton.IsVisible;
            set => _titleBar.OptionsButton.IsVisible = value;
        } 
        
        public PageContainer Pager { get; }
        
        private static MainWindow? _instance;
        public static MainWindow Instance => _instance ??= new MainWindow();

        public static bool IsReady()
        {
            return _instance != null;
        }

        public static void Kill()
        {
            _instance = null;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            // Weird OSX hack to fix graphical corruptions and hide decorations
            if (PlatformUtils.IsOSX)
            {
                SystemDecorations = SystemDecorations.Full;
                ExtendClientAreaToDecorationsHint = true;
            }
            
            Pager = this.GetControl<PageContainer>("Container");

            // Allocate essential pages immediately
            Pager.RegisterPages(HomePage, ConnectionLostPage, new WelcomePage());
            
            // Defer the rest of the page registration
            Dispatcher.UIThread.Post(() => Pager.RegisterPages(new AmbientSoundPage(), new FindMyGearPage(), new FactoryResetPage(),
                new CreditsPage(), new TouchpadPage(), new EqualizerPage(), new AdvancedPage(), new NoiseProPage(),
                new SystemPage(), new SelfTestPage(), new SettingsPage(), new PopupSettingsPage(),
                CustomTouchActionPage, DeviceSelectionPage, new SystemInfoPage(), UnsupportedFeaturePage,
                UpdatePage, UpdateProgressPage, new SystemCoredumpPage(), new HotkeyPage(), new FirmwareSelectionPage(),
                new FirmwareTransferPage(), new SpatialTestPage(), new BixbyRemapPage(),
                new CrowdsourcingSettingsPage(), new BudsAppDetectedPage(), new TouchpadGesturePage(), 
                new NoiseProAmbientPage(), new GearFitPage()), DispatcherPriority.ApplicationIdle);
            
            _titleBar = this.GetControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) =>
            {
                _titleBar.OptionsButton.ContextMenu?.Open(_titleBar.OptionsButton);
            };
            _titleBar.ClosePressed += (sender, args) =>
            {
                if (SettingsProvider.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
                {
                    Log.Information("MainWindow.TitleBar: Close requested, minimizing to tray bar");
                    BringToTray();
                }
                else
                { 
                    Log.Information("MainWindow.TitleBar: Close requested, exiting app");
                    Close();
                }
            };
            
            BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
            BluetoothImpl.Instance.Disconnected += OnDisconnected;
            BluetoothImpl.Instance.Connected += OnConnected;

            SPPMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
            SPPMessageHandler.Instance.StatusUpdate += OnStatusUpdate;
            SPPMessageHandler.Instance.OtherOption += HandleOtherTouchOption;
            SPPMessageHandler.Instance.AnyMessageReceived += OnAnyMessageReceived;

            EventDispatcher.Instance.EventReceived += OnEventReceived;
            (Application.Current as App)!.TrayIconClicked += TrayIcon_OnLeftClicked;
            _ = TrayManager.Instance.RebuildAsync();
            
            Pager.PageSwitched += (sender, pages) => BuildOptionsMenu();
            Loc.LanguageUpdated += LocOnLanguageUpdated;
            BuildOptionsMenu();
            
            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                Task.Run(() => BluetoothImpl.Instance.ConnectAsync());
                Pager.SwitchPage(AbstractPage.Pages.Home);
            }
            else
            {
                Pager.SwitchPage(AbstractPage.Pages.Welcome);
            }
            
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon)
                {
                    WindowState = WindowState.Minimized;
                }
            }
        }

        private void LocOnLanguageUpdated()
        {
            BuildOptionsMenu();
            FlowDirection = Loc.ResolveFlowDirection();
        }

        private void OnAnyMessageReceived(object? sender, BaseMessageParser? e)
        {
            if (e is VoiceWakeupEventParser wakeup)
            {
                if (wakeup.ResultCode == 1)
                {
                    Log.Debug("MainWindow.OnAnyMessageReceived: Voice wakeup event received");
                    
                    EventDispatcher.Instance.Dispatch(SettingsProvider.Instance.BixbyRemapEvent);
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
                        BringToTray();
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
                await Task.Delay(6000).ContinueWith((_) => UpdateManager.Instance.SilentCheck());
            }
            base.OnInitialized();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
        }
        
        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            
            if (SettingsProvider.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
            {
                // check if the cause of the termination is due to shutdown or application close request
                if (e.CloseReason == WindowCloseReason.OSShutdown || e.CloseReason == WindowCloseReason.ApplicationShutdown)
                {
                    Log.Debug("MainWindow.OnClosing: closing event, continuing termination");
                }
                else
                {
                    BringToTray();
                    e.Cancel = true;
                    Log.Debug("MainWindow.OnClosing: Termination cancelled");
                }
            }
            else
            {
                Log.Debug("MainWindow.OnClosing: Now closing session");
            }
            
            base.OnClosing(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            if (_firstShow)
            {
                HotkeyReceiverImpl.Reset();
                HotkeyReceiverImpl.Instance.Update(silent: true);
            }

            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon && _firstShow)
                {
                    Log.Debug("MainWindow: Launched minimized");
                    BringToTray();
                }
            }

            if(_firstShow)
                Log.Information("Startup time: {Time}",  Stopwatch.GetElapsedTime(Program.StartedAt));
            
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

            (Application.Current as App)!.TrayIconClicked -= TrayIcon_OnLeftClicked;
            EventDispatcher.Instance.EventReceived -= OnEventReceived;

            Loc.LanguageUpdated -= LocOnLanguageUpdated;

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

                #if OSX
                ThePBone.OSX.Native.Unmanaged.AppUtils.setHideInDock(false);
                #endif
                Show();
                
                Activate();
                Topmost = true;
                Topmost = false;
                Focus();
            });
        }

        private void BringToTray()
        {
#if OSX
            ThePBone.OSX.Native.Unmanaged.AppUtils.setHideInDock(true);
#endif
            Hide();
        }

        public override void Show()
        {
            base.Show();
            Pager.Suspended = false;
        }

        public override void Hide()
        {
            base.Hide();
            Pager.Suspended = true;
        }

        private void TrayIcon_OnLeftClicked()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsVisible)
                {
                    BringToFront();
                }
                else
                {
                    BringToTray();
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
            
            // Update dynamic tray icon
            if (e is IBasicStatusUpdate status)
            {
                WindowIconRenderer.UpdateDynamicIcon(status);
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
            _popupShown = false;
            Pager.SwitchPage(AbstractPage.Pages.Home);
        }

        private void OnBluetoothError(object? sender, BluetoothException e)
        {
            WindowIconRenderer.ResetIconToDefault();
            
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
                    _popupShown = false;
                    break;
            }
        }

        private void OnDisconnected(object? sender, string e)
        {
            WindowIconRenderer.ResetIconToDefault();
            
            Pager.SwitchPage(BluetoothImpl.Instance.RegisteredDeviceValid
                ? AbstractPage.Pages.NoConnection
                : AbstractPage.Pages.Welcome);
            _popupShown = false;
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
               
                [Loc.Resolve("optionsmenu_deregister")] = (sender, args) =>
                {
                    BluetoothImpl.Instance.UnregisterDevice();
                    Pager.SwitchPage(AbstractPage.Pages.Welcome);
                }
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

            _titleBar.OptionsButton.ContextMenu = MenuFactory.BuildContextMenu(options, _titleBar.OptionsButton);
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
            Log.Debug($"MainWindow.ShowPopup: {(_popupShown ? "Popup already shown" : "Popup not yet shown")}; Ignore conditional check: {ignoreRestrictions}");
            if (_popupShown && !ignoreRestrictions)
                return;
            
            _popup ??= new BudsPopup();
                
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
            
            if (!BluetoothImpl.Instance.IsConnected)
            {
                Log.Warning("MainWindow.ShowPopup: Not connected");
            }
            else
            {
                _popupShown = true;
            }
        }
    }
}
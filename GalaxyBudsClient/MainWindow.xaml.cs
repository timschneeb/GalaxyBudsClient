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
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.InterfaceOld;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.InterfaceOld.Pages;
using GalaxyBudsClient.InterfaceOld.Transition;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using NetSparkleUpdater.Enums;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;
using MessageBox = GalaxyBudsClient.InterfaceOld.Dialogs.MessageBox;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using Window = Avalonia.Controls.Window;

namespace GalaxyBudsClient
{
    public sealed class MainWindow : Window
    {
        public readonly HomePage HomePage = new();
        public readonly CustomTouchActionPage CustomTouchActionPage = new();
        public readonly ConnectionLostPage ConnectionLostPage = new();
        public readonly UpdatePage UpdatePage = new();
        public readonly UpdateProgressPage UpdateProgressPage = new();
        public readonly DeviceSelectionPage DeviceSelectionPage = new();
        
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
        public static MainWindow Instance
        {
            get
            {
                Log.Warning("LegacyMainWindow: Instance requested from {Ctx}", new StackTrace().ToString());
                return _instance ??= new MainWindow();
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
            Dispatcher.UIThread.Post(() => Pager.RegisterPages(new FindMyGearPage(),
                new TouchpadPage(), new AdvancedPage(),
                CustomTouchActionPage, DeviceSelectionPage,
                UpdatePage, UpdateProgressPage, new HotkeyPage(), new BixbyRemapPage(),
                new BudsAppDetectedPage(), new TouchpadGesturePage(), 
                new GearFitPage()), DispatcherPriority.ApplicationIdle);
            
            _titleBar = this.GetControl<CustomTitleBar>("TitleBar");
            _titleBar.PointerPressed += (i, e) => BeginMoveDrag(e);
            _titleBar.OptionsPressed += (i, e) =>
            {
                _titleBar.OptionsButton.ContextMenu?.Open(_titleBar.OptionsButton);
            };
            _titleBar.ClosePressed += (sender, args) =>
            {
                if (Settings.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
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

            SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
            SppMessageHandler.Instance.StatusUpdate += OnStatusUpdate;
            SppMessageHandler.Instance.OtherOption += HandleOtherTouchOption;
            SppMessageHandler.Instance.AnyMessageReceived += OnAnyMessageReceived;

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
            if (e is VoiceWakeupEventParser { ResultCode: 1 })
            {
                Log.Debug("MainWindow.OnAnyMessageReceived: Voice wakeup event received");
                    
                EventDispatcher.Instance.Dispatch(Settings.Instance.BixbyRemapEvent);
            }
        }

        private async void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            switch (e)
            {
                case EventDispatcher.Event.PairingMode:
                    await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.UNK_PAIRING_MODE);
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
                    if (!BluetoothImpl.Instance.IsConnectedLegacy)
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
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            
            if (Settings.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
            {
                // check if the cause of the termination is due to shutdown or application close request
                if (e.CloseReason is WindowCloseReason.OSShutdown or WindowCloseReason.ApplicationShutdown)
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

            SppMessageHandler.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
            SppMessageHandler.Instance.StatusUpdate -= OnStatusUpdate;
            SppMessageHandler.Instance.OtherOption -= HandleOtherTouchOption;

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
                e.WearState != WearStates.None && Settings.Instance.ResumePlaybackOnSensor)
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

        private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateParser e)
        {
            if (Settings.Instance.Popup.Enabled)
            {
                ShowPopup();
            }
            
            // Update dynamic tray icon
            if (e is IBasicStatusUpdate status)
            {
                WindowIconRenderer.UpdateDynamicIcon(status);
            }
            
            // Reply manager info and request & cache SKU info
            _ = MessageComposer.SetManagerInfo();
            if(BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugSku))
                _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SKU);
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
            var action = e == TouchOptions.OtherL ?
                Settings.Instance.CustomActionLeft : Settings.Instance.CustomActionRight;

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
                        Log.Error("CustomAction.HotkeyBroadcast: Cannot parse saved key-combo: {Message}", ex.Message);
                        Log.Error("CustomAction.HotkeyBroadcast: Caused by combo: {Param}", action.Parameter);
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
            var restricted = Pager.CurrentPage == AbstractPage.Pages.Welcome ||
                             Pager.CurrentPage == AbstractPage.Pages.DeviceSelect ||
                             !BluetoothImpl.Instance.RegisteredDeviceValid;

            var options = new Dictionary<string, EventHandler<RoutedEventArgs>?>()
            {
                [Loc.Resolve("optionsmenu_settings")] =
                    (sender, args) => Pager.SwitchPage(AbstractPage.Pages.Settings),
                [Loc.Resolve("optionsmenu_refresh")] = async (sender, args) =>
                    await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA),
               
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
        public void ShowCustomActionSelection(Devices device)
        {
            CustomTouchActionPage.CurrentSide = device;
            Pager.SwitchPage(AbstractPage.Pages.TouchCustomAction);
        }
        #endregion

        public void ShowPopup(bool ignoreRestrictions = false)
        {
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
            
            if (!BluetoothImpl.Instance.IsConnectedLegacy)
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
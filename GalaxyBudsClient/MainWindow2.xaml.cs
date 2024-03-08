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
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Services;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.InterfaceOld.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;
using MessageBox = GalaxyBudsClient.InterfaceOld.Dialogs.MessageBox;

namespace GalaxyBudsClient
{
    public sealed class MainWindow2 : AppWindow
    {
        private BudsPopup? _popup;
        
        private bool _popupShown = false;
        private bool _firstShow = true;
        private WearStates _lastWearState = WearStates.Both;
        
        public bool OverrideMinimizeTray { set; get; }
        
        private static MainWindow2? _instance;
        public static MainWindow2 Instance => _instance ??= new MainWindow2();

        // ReSharper disable once MemberCanBePrivate.Global
        public MainWindow2()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();
            
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
            
            Loc.LanguageUpdated += OnLanguageUpdated;
            
            if (BluetoothImpl.Instance.RegisteredDeviceValid)
            {
                Task.Run(() => BluetoothImpl.Instance.ConnectAsync());
                //Pager.SwitchPage(AbstractPage.Pages.Home);
            }
            else
            {
                //Pager.SwitchPage(AbstractPage.Pages.Welcome);
            }
            
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon)
                {
                    WindowState = WindowState.Minimized;
                }
            }
            
            TitleBar.ExtendsContentIntoTitleBar = true;
            TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        
            Application.Current.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        }
  
        private void OnLanguageUpdated()
        {
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
                    if (IsVisible)
                        BringToTray();
                    else
                        BringToFront();
                    break;
                case EventDispatcher.Event.Connect:
                    if (!BluetoothImpl.Instance.IsConnectedLegacy)
                    {
                        await BluetoothImpl.Instance.ConnectAsync();
                    }
                    break;
                case EventDispatcher.Event.ShowBatteryPopup:
                    ShowPopup(noDebounce: true);
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

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
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
            
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);
            base.OnClosing(e);
        }
        
    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (IsWindows11)
        {
            if (ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                TryEnableMicaEffect();
            }
            else
            {
                ClearValue(BackgroundProperty);
                ClearValue(TransparencyBackgroundFallbackProperty);
            }
        }
    }

    private void TryEnableMicaEffect()
    {
        return;
       // TransparencyBackgroundFallback = Brushes.Transparent;
       // TransparencyLevelHint = WindowTransparencyLevel.Mica;

        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Dark, out var value) ? (Color2)(Avalonia.Media.Color)value : new Color2(32, 32, 32);

            color = color.LightenPercent(-0.8f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
        else if (ActualThemeVariant == ThemeVariant.Light)
        {
            // Similar effect here
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Light, out var value) ? (Color2)(Avalonia.Media.Color)value : new Color2(243, 243, 243);

            color = color.LightenPercent(0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
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
            
            
        var thm = ActualThemeVariant;
        if (IsWindows11 && thm != FluentAvaloniaTheme.HighContrastTheme)
        {
            TryEnableMicaEffect();
        }
            
            base.OnOpened(e);
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

            Loc.LanguageUpdated -= OnLanguageUpdated;

            if(Application.Current.ApplicationLifetime is IControlledApplicationLifetime lifetime)
            {
                Log.Information("MainWindow: Shutting down normally");
                lifetime.Shutdown();
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
            if(BluetoothImpl.Instance.DeviceSpec.Supports(IDeviceSpec.Feature.DebugSku))
                _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SKU);
        }

        private void OnConnected(object? sender, EventArgs e)
        {
            _popupShown = false;
            // Pager.SwitchPage(AbstractPage.Pages.Home);
        }

        private async void OnBluetoothError(object? sender, BluetoothException e)
        {
            WindowIconRenderer.ResetIconToDefault();
            
            switch (e.ErrorCode)
            {
                case BluetoothException.ErrorCodes.NoAdaptersAvailable:
                    await new GalaxyBudsClient.Interface.Dialogs.MessageBox()
                    {
                        Title = Loc.Resolve("error"),
                        Description = Loc.Resolve("nobluetoothdev")
                    }.ShowAsync();
                    break;
                default:
                    _popupShown = false;
                    break;
            }
        }

        private void OnDisconnected(object? sender, string e)
        {
            WindowIconRenderer.ResetIconToDefault();
            _popupShown = false;
        }

        private async void HandleOtherTouchOption(object? sender, TouchOptions e)
        {
            ICustomAction action = e == TouchOptions.OtherL ?
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
                        await new GalaxyBudsClient.Interface.Dialogs.MessageBox()
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n" +
                                          $"File not found: '{ex.FileName}'"
                        }.ShowAsync();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 13 && PlatformUtils.IsLinux)
                        {
                            await new GalaxyBudsClient.Interface.Dialogs.MessageBox()
                            {
                                Title = "Custom long-press action failed",
                                Description = $"Unable to launch external application.\n\n" +
                                              $"Insufficient permissions. Please add execute permissions for your user/group to this file.\n\n" +
                                              $"Run this command in a terminal: chmod +x \"{action.Parameter}\""
                            }.ShowAsync();
                        }
                        else
                        {
                            await new GalaxyBudsClient.Interface.Dialogs.MessageBox()
                            {
                                Title = "Custom long-press action failed",
                                Description = $"Unable to launch external application.\n\n" +
                                              $"Detailed information:\n\n" +
                                              $"{ex.Message}"
                            }.ShowAsync();
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
        
        public void ShowPopup(bool noDebounce = false)
        {
            if (_popupShown && !noDebounce)
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
            finally
            {
                _popupShown = true;
            }
        }
    }
}
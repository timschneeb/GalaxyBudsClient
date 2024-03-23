using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Dialogs;
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

namespace GalaxyBudsClient;

public partial class MainWindow : AppWindow
{
    private BudsPopup? _popup;

    private bool _popupShown = false;
    private bool _firstShow = true;
    private LegacyWearStates _lastWearState = LegacyWearStates.Both;

    private static App App => Application.Current as App ?? throw new InvalidOperationException();
    public bool OverrideMinimizeTray { set; get; }
        
    private static MainWindow? _instance;
    public static MainWindow Instance => _instance ??= new MainWindow();

    // ReSharper disable once MemberCanBePrivate.Global
    public MainWindow()
    {
        InitializeComponent();
            
        BluetoothService.Instance.BluetoothError += OnBluetoothError;
        BluetoothService.Instance.Disconnected += OnDisconnected;
        BluetoothService.Instance.Connected += OnConnected;

        SppMessageHandler.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageHandler.Instance.StatusUpdate += OnStatusUpdate;
        SppMessageHandler.Instance.OtherOption += HandleOtherTouchOption;

        EventDispatcher.Instance.EventReceived += OnEventReceived;
        App.TrayIconClicked += TrayIcon_OnLeftClicked;
        _ = TrayManager.Instance.RebuildAsync();
            
        Loc.LanguageUpdated += OnLanguageUpdated;
            
        if (BluetoothService.RegisteredDeviceValid)
        {
            Task.Run(() => BluetoothService.Instance.ConnectAsync());
        }
            
        if (App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon)
            {
                WindowState = WindowState.Minimized;
            }
        }
            
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName is nameof(Settings.Instance.DarkMode) or nameof(Settings.Instance.BlurStrength))
        {
            if (Settings.Instance.DarkMode == DarkModes.Dark)
            {
                TryEnableMicaEffect();
            }
            else
            {
                TransparencyLevelHint = Array.Empty<WindowTransparencyLevel>();
                ClearValue(BackgroundProperty);
                ClearValue(TransparencyBackgroundFallbackProperty);
            }
        }
    }

    private void OnLanguageUpdated()
    {
        FlowDirection = Loc.ResolveFlowDirection();
    }

    private async void OnEventReceived(Event e, object? arg)
    {
        switch (e)
        {
            case Event.PairingMode:
                await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.UNK_PAIRING_MODE);
                break;
            case Event.ToggleManagerVisibility:
                if (IsVisible)
                    BringToTray();
                else
                    BringToFront();
                break;
            case Event.Connect:
                if (!BluetoothService.Instance.IsConnected)
                {
                    await BluetoothService.Instance.ConnectAsync();
                }
                break;
            case Event.ShowBatteryPopup:
                ShowPopup(true);
                break;
        }
    }

    #region Window management
    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (Settings.Instance.MinimizeToTray && !OverrideMinimizeTray && PlatformUtils.SupportsTrayIcon)
        {
            // check if the cause of the termination is due to shutdown or application close request
            if (e.CloseReason is not (WindowCloseReason.OSShutdown or WindowCloseReason.ApplicationShutdown))
            {
                BringToTray();
                e.Cancel = true;
                Log.Debug("MainWindow.OnClosing: Termination cancelled");
                base.OnClosing(e);
                return;
            }

            Log.Debug("MainWindow.OnClosing: closing event, continuing termination");
        }
        else
        {
            Log.Debug("MainWindow.OnClosing: Now closing session");
        }
            
        await BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.FIND_MY_EARBUDS_STOP);
        await BluetoothService.Instance.DisconnectAsync();
        base.OnClosing(e);
    }
    
    private void TryEnableMicaEffect()
    {
        // TODO test on Windows
        
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = new[]
            { WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur };
        
        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        var color = this.TryFindResource("SolidBackgroundFillColorBase",
            ThemeVariant.Dark, out var value) ? (Color2)(Color)value! : new Color2(32, 32, 32);

        color = color.LightenPercent(-0.8f);
        color = color.WithAlpha(Settings.Instance.BlurStrength);

        Background = new ImmutableSolidColorBrush(color);
    } 
        
    protected override void OnOpened(EventArgs e)
    {
        if (_firstShow)
        {
            HotkeyReceiver.Reset();
            HotkeyReceiver.Instance.Update(true);
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
        if (/* TODO IsWindows11 &&*/ Settings.Instance.DarkMode == DarkModes.Dark)
        {
            TryEnableMicaEffect();
        }
                
        base.OnOpened(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        BluetoothService.Instance.BluetoothError -= OnBluetoothError;
        BluetoothService.Instance.Disconnected -= OnDisconnected;
        BluetoothService.Instance.Connected -= OnConnected;
            
        SppMessageHandler.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        SppMessageHandler.Instance.StatusUpdate -= OnStatusUpdate;
        SppMessageHandler.Instance.OtherOption -= HandleOtherTouchOption;

        App.TrayIconClicked -= TrayIcon_OnLeftClicked;
        EventDispatcher.Instance.EventReceived -= OnEventReceived;

        Loc.LanguageUpdated -= OnLanguageUpdated;

        if(App.ApplicationLifetime is IControlledApplicationLifetime lifetime)
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
        if (_lastWearState == LegacyWearStates.None &&
            e.WearState != LegacyWearStates.None && Settings.Instance.ResumePlaybackOnSensor)
        {
            MediaKeyRemote.Instance.Play();
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
        if(BluetoothService.Instance.DeviceSpec.Supports(Features.DebugSku))
            _ = BluetoothService.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_SKU);
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
                await new MessageBox()
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
        var action = e == TouchOptions.OtherL ?
            Settings.Instance.CustomActionLeft : Settings.Instance.CustomActionRight;

        switch (action.Action)
        {
            case CustomAction.Actions.Event:
                EventDispatcher.Instance.Dispatch(Enum.Parse<Event>(action.Parameter), true);
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
                    await new MessageBox()
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
                        await new MessageBox()
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n\n" +
                                          $"Insufficient permissions. Please add execute permissions for your user/group to this file.\n\n" +
                                          $"Run this command in a terminal: chmod +x \"{action.Parameter}\""
                        }.ShowAsync();
                    }
                    else
                    {
                        await new MessageBox()
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

                HotkeyBroadcast.Instance.SendKeys(keys);
                break;
        }
    }
    #endregion

    private void ShowPopup(bool noDebounce = false)
    {
        // TODO apply blur to popup too
        if (_popupShown && !noDebounce)
            return;
            
        _popup ??= new BudsPopup();
        _popup.RequestedThemeVariant = ThemeUtils.GetThemeVariant();
                
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
            _popup = new BudsPopup
            {
                RequestedThemeVariant = ThemeUtils.GetThemeVariant()
            };
            _popup.Show();
        }
        finally
        {
            _popupShown = true;
        }
    }
}
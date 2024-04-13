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
using Avalonia.Threading;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Interface.StyledWindow;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using Serilog;
using Application = Avalonia.Application;
using Environment = System.Environment;

namespace GalaxyBudsClient.Interface;

public partial class MainWindow : StyledAppWindow
{
    private BudsPopup? _popup;
    private bool _popupShown;
    
    private bool _firstShow = true;
    private LegacyWearStates _lastWearState = LegacyWearStates.Both;

    private static App App => Application.Current as App ?? throw new InvalidOperationException();
        
    private static MainWindow? _instance;
    public static MainWindow Instance => _instance ??= new MainWindow();

    public MainWindow()
    {
        InitializeComponent();
            
        BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
        BluetoothImpl.Instance.Connected += OnConnected;

        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.StatusUpdate += OnStatusUpdate;
        SppMessageReceiver.Instance.OtherOption += HandleOtherTouchOption;

        EventDispatcher.Instance.EventReceived += OnEventReceived;
        App.TrayIconClicked += TrayIcon_OnLeftClicked;
        _ = TrayManager.Instance.RebuildAsync();
            
        Loc.LanguageUpdated += OnLanguageUpdated;
            
        if (BluetoothImpl.IsRegisteredDeviceValid)
        {
            Task.Run(() => BluetoothImpl.Instance.ConnectAsync());
        }
            
        if (App.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if ((desktop.Args?.Contains("/StartMinimized") ?? false) && PlatformUtils.SupportsTrayIcon)
            {
                WindowState = WindowState.Minimized;
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
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.UNK_PAIRING_MODE);
                break;
            case Event.ToggleManagerVisibility:
                if (IsVisible)
                    BringToTray();
                else
                    BringToFront();
                break;
            case Event.Connect:
                if (!BluetoothImpl.Instance.IsConnected)
                {
                    await BluetoothImpl.Instance.ConnectAsync();
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
        if (Settings.Data.MinimizeToTray && PlatformUtils.SupportsTrayIcon)
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
            
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.FIND_MY_EARBUDS_STOP);
        await BluetoothImpl.Instance.DisconnectAsync();
        base.OnClosing(e);
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
        
        base.OnOpened(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        BluetoothImpl.Instance.BluetoothError -= OnBluetoothError;
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
        BluetoothImpl.Instance.Connected -= OnConnected;
            
        SppMessageReceiver.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.StatusUpdate -= OnStatusUpdate;
        SppMessageReceiver.Instance.OtherOption -= HandleOtherTouchOption;

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
    private void OnStatusUpdate(object? sender, StatusUpdateDecoder e)
    {
        if (_lastWearState == LegacyWearStates.None &&
            e.WearState != LegacyWearStates.None && Settings.Data.ResumePlaybackOnSensor)
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

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        if (Settings.Data.PopupEnabled)
        {
            ShowPopup();
        }
            
        // Update dynamic tray icon
        if (e is IBasicStatusUpdate status)
        {
            WindowIconRenderer.UpdateDynamicIcon(status);
        }
            
        // Reply manager info and request & cache SKU info
        _ = BluetoothImpl.Instance.SendAsync(new ManagerInfoEncoder());
        if(BluetoothImpl.Instance.DeviceSpec.Supports(Features.DebugSku))
            _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_SKU);
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
                await new MessageBox
                {
                    Title = Strings.Error,
                    Description = Strings.Nobluetoothdev
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
            Settings.Data.CustomActionLeft : Settings.Data.CustomActionRight;

        switch (action.Action)
        {
            case CustomActions.Event:
                if (EventExtensions.TryParse(action.Parameter, out var result, true))
                {
                    EventDispatcher.Instance.Dispatch(result);
                }
                break;
            case CustomActions.RunExternalProgram:
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
                    await new MessageBox
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
                        await new MessageBox
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n\n" +
                                          $"Insufficient permissions. Please add execute permissions for your user/group to this file.\n\n" +
                                          $"Run this command in a terminal: chmod +x \"{action.Parameter}\""
                        }.ShowAsync();
                    }
                    else
                    {
                        await new MessageBox
                        {
                            Title = "Custom long-press action failed",
                            Description = $"Unable to launch external application.\n\n" +
                                          $"Detailed information:\n\n" +
                                          $"{ex.Message}"
                        }.ShowAsync();
                    }
                }

                break;
            case CustomActions.TriggerHotkey:
                var keys = new List<Key>();
                try
                {
                    Key? Parse(string s)
                    {
                        if (!Enum.TryParse<Key>(s, out var key)) return null;
                        return key;
                    }

                    keys.AddRange(action.Parameter.Split(',')
                        .Select(Parse)
                        .Where(x => x is not null)
                        .Cast<Key>());
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
        if (_popupShown && !noDebounce)
            return;
        
        if (_popup is { IsVisible: true })
        {
            _popup.UpdateSettings();
            _popup.RearmTimer();
        }
        
        WindowLauncher.ShowAsSingleInstance(ref _popup); 
        _popupShown = true;
    }
}
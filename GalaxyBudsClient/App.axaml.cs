#if OSX
using AppKit;
#endif
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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
#if !Android
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Scripting.Experiment;
#endif
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface;
using Serilog;
using Application = Avalonia.Application;
using MainWindow = GalaxyBudsClient.Interface.MainWindow;

namespace GalaxyBudsClient;

public class App : Application
{
    public FluentAvaloniaTheme FluentTheme => (FluentAvaloniaTheme)Styles.Single(x => x is FluentAvaloniaTheme);

    public bool StartMinimized =>
        ((ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .Args?.Contains("/StartMinimized") ?? false) 
        && PlatformUtils.SupportsTrayIcon;
    
    public static readonly StyledProperty<NativeMenu> TrayMenuProperty =
        AvaloniaProperty.Register<App, NativeMenu>(nameof(TrayMenu),
            defaultBindingMode: BindingMode.OneWay, defaultValue: []);
    public NativeMenu TrayMenu => GetValue(TrayMenuProperty);
    
#if !Android
    private readonly ExperimentManager _experimentManager = new();
#endif
    
    private BudsPopup? _popup;
    private bool _popupShown;
    private LegacyWearStates _lastWearState = LegacyWearStates.Both;
    
    public override void Initialize()
    {
        DataContext = this;
            
#if OSX
        NSApplication.Init();
        // For menu bar applications (LSUIElement=true), hide the dock icon immediately at startup.
        // The dock icon will only appear when the settings window is explicitly opened.
        GalaxyBudsClient.Platform.OSX.AppUtils.setHideInDock(true);
#endif

        AvaloniaXamlLoader.Load(this);
            
        if (Loc.IsTranslatorModeEnabled)
        {
            Settings.Data.Locale = Locales.custom;
        }
            
        Dispatcher.UIThread.Post(() =>
        { 
            LoadThemeProperties();
            Loc.Load();
        }, DispatcherPriority.Render);
        
        Log.Information("Translator mode file location: {File}", Loc.TranslatorModeFile);
#if !Android
        ScriptManager.Instance.RegisterUserHooks();
        Log.Debug("Environment: {Env}", _experimentManager.CurrentEnvironment());
#endif
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // FluentAvalonia 2.4.1's ItemsRepeaterAutomationPeer.GetChildrenCore() can throw an
        // unguarded NullReferenceException when the macOS accessibility bridge walks the automation
        // tree while a repeater is mid-virtualization (e.g. during a navigation/relayout triggered
        // by changing a setting). Upstream never guarded this, and the exception otherwise
        // propagates out of the dispatcher loop and kills the whole app. Swallow only that specific
        // accessibility-tree failure — it is benign and affects an assistive-technology query, not
        // the UI itself — while letting every other exception crash and report as before.
        Dispatcher.UIThread.UnhandledException += OnDispatcherUnhandledException;

        if (BluetoothImpl.HasValidDevice)
        {
            Task.Run(() => BluetoothImpl.Instance.ConnectAsync());
            _ = TrayManager.Instance.RebuildAsync();
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Initialize MainWindow singleton
            var mainWindow = MainWindow.Instance;
            
#if OSX
            // Show the main window on launch (unless started with /StartMinimized); the menu bar
            // icon stays available either way. When we show it, restore the dock icon that
            // Initialize() hid for the menu-bar-app case.
            desktop.MainWindow = StartMinimized ? null : mainWindow;
            mainWindow.IsVisible = !StartMinimized;
            if (!StartMinimized)
                GalaxyBudsClient.Platform.OSX.AppUtils.setHideInDock(false);
#else
            // Stay initially minimized: don't attach a main window
            desktop.MainWindow = StartMinimized ? null : mainWindow;
#endif
            
            TrayManager.Init();
            BatteryHistoryManager.Init();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
        }
        
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        EventDispatcher.Instance.EventReceived += OnEventReceived;
        
        BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
        BluetoothImpl.Instance.Connected += OnConnected;
        SppMessageReceiver.Instance.StatusUpdate += OnStatusUpdate;
        SppMessageReceiver.Instance.OtherOption += HandleOtherTouchOption;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        
        DeviceMessageCache.Init();
        
        if (Loc.IsTranslatorModeEnabled)
        {
            Dialogs.ShowTranslatorTools();
        }
            
        base.OnFrameworkInitializationCompleted();
    }
    
    private async void OnEventReceived(Event e, object? arg)
    {
        switch (e)
        {
            case Event.PairingMode:
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.PAIRING_MODE);
                break;
            case Event.ToggleManagerVisibility:
                if (!PlatformUtils.IsDesktop)
                    break;
                
                MainWindow.Instance.ToggleVisibility();
                break;
            case Event.ShowBatteryPopup:
                ShowPopup(true);
                break;
        }
    }
    
    private void ShowPopup(bool noDebounce = false)
    {
        if (!PlatformUtils.IsDesktop || (_popupShown && !noDebounce))
            return;
        
        if (_popup is { IsVisible: true })
        {
            _popup.UpdateSettings();
            _popup.RearmTimer();
            return;
        }
        
        Dialogs.ShowAsSingleInstanceOnDesktop(ref _popup); 
        _popupShown = true;
    }
    
    private bool _customEqReapplied;

    private void OnConnected(object? sender, EventArgs e)
    {
        _popupShown = false;
        _customEqReapplied = false;
    }

    private void OnBluetoothError(object? sender, BluetoothException e)
    {
        WindowIconRenderer.ResetIconToDefault();
        _popupShown = false;
    }
    
    private void OnDisconnected(object? sender, string e)
    {
        WindowIconRenderer.ResetIconToDefault();
        _popupShown = false;
        _customEqReapplied = false;
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

        // Re-apply the saved custom EQ ONCE per connection. ExtendedStatusUpdate also arrives on
        // routine state changes (case open/close, on-head, noise-control switches); re-pushing the
        // EQ on every one caused an audible mid-session re-application. The firmware drops the
        // custom table on power-cycle, so a single re-push per connect is enough.
        if (!_customEqReapplied)
        {
            _customEqReapplied = true;
            _ = ReapplyCustomEqualizerAsync();
        }
    }

    // The custom EQ band table is not retained by the firmware across power cycles, so the values
    // are persisted per-device (see EqualizerPageViewModel.PersistCustomEq) and re-pushed here on
    // every connect, mirroring what the official Galaxy Wearable app does.
    private static async Task ReapplyCustomEqualizerAsync()
    {
        if (!BluetoothImpl.Instance.DeviceSpec.Supports(Features.CustomEqualizer))
            return;
        if (BluetoothImpl.Instance.Device.Current is not { CustomEqualizerEnabled: true } device ||
            device.CustomEqualizerBands is not { Length: 9 } bands)
            return;

        await BluetoothImpl.Instance.SendAsync(new SetCustomEqualizerEncoder
        {
            BandGains =
            [
                (sbyte)bands[0], (sbyte)bands[1], (sbyte)bands[2],
                (sbyte)bands[3], (sbyte)bands[4], (sbyte)bands[5],
                (sbyte)bands[6], (sbyte)bands[7], (sbyte)bands[8]
            ]
        });
        // Samsung preset index 6 = custom; the EQUALIZER message carries index + 1 (7)
        await BluetoothImpl.Instance.SendAsync(new SetEqualizerEncoder { IsEnabled = true, Preset = 6 });
    }

    private static void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Swallow ONLY FluentAvalonia's ItemsRepeaterAutomationPeer.GetChildrenCore() NRE, raised by
        // the macOS accessibility bridge while walking the automation tree. Match precisely (specific
        // type + method, unwrapping inner/aggregate exceptions) so genuine bugs still crash/report.
        for (Exception? ex = e.Exception; ex != null; ex = ex.InnerException)
        {
            if (ex is NullReferenceException &&
                ex.StackTrace is { } trace &&
                trace.Contains("ItemsRepeaterAutomationPeer") &&
                trace.Contains("GetChildrenCore"))
            {
                Log.Warning(e.Exception,
                    "Suppressed non-fatal automation-peer exception raised by the accessibility bridge");
                e.Handled = true;
                return;
            }
        }
    }

    private void OnStatusUpdate(object? sender, StatusUpdateDecoder e)
    {
        if (_lastWearState == LegacyWearStates.None &&
            e.WearState != LegacyWearStates.None && Settings.Data.ResumePlaybackOnSensor)
        {
            PlatformImpl.MediaKeyRemote.Play();
        }
        else if (_lastWearState != LegacyWearStates.None &&
            e.WearState == LegacyWearStates.None && Settings.Data.PausePlaybackOnSensor)
        {
            PlatformImpl.MediaKeyRemote.Pause();
        }
            
        // Update dynamic tray icon
        if (e is IBasicStatusUpdate status)
        {
            WindowIconRenderer.UpdateDynamicIcon(status);
        }
            
        _lastWearState = e.WearState;
    }
    
    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.Data.Theme) or nameof(Settings.Data.AccentColor):
                LoadThemeProperties();
                break;
            case nameof(Settings.Data.Locale):
                Loc.Load();
                break;
            case nameof(Settings.Data.DynamicTrayIconMode):
            {
                var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
                if (Settings.Data.DynamicTrayIconMode != DynamicTrayIconModes.Disabled && BluetoothImpl.Instance.IsConnected && cache != null)
                    WindowIconRenderer.UpdateDynamicIcon(cache);
                else
                    WindowIconRenderer.ResetIconToDefault();
                break;
            }
        }
    }

    private void LoadThemeProperties()
    {
        FluentTheme.PreferSystemTheme = Settings.Data.Theme == Themes.System;
        var color = Settings.Data.AccentColor;
        if(Color.FromUInt32(color).A == 0)
        {
            color = Settings.Data.AccentColor = Colors.Orange.ToUInt32();
        }
        FluentTheme.CustomAccentColor = Color.FromUInt32(color);
        Resources["AccentColor"] = FluentTheme.CustomAccentColor;
    }
        
    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        MainWindow.Instance.ToggleVisibility();
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
                
                Platform.PlatformImpl.HotkeyBroadcast.SendKeys(keys);
                break;
        }
    }
}

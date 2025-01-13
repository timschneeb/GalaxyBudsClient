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
        NSApplication.Notifications.ObserveDidBecomeActive((_, _) =>
        {
            Dispatcher.UIThread.InvokeAsync(delegate
            {
                MainWindow.Instance.BringToFront();
            });
        });
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
        if (BluetoothImpl.HasValidDevice)
        {
            Task.Run(() => BluetoothImpl.Instance.ConnectAsync());
            _ = TrayManager.Instance.RebuildAsync();
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Initialize MainWindow singleton
            var mainWindow = MainWindow.Instance;
            // Stay initially minimized: don't attach a main window
            desktop.MainWindow = StartMinimized ? null : mainWindow;
            
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
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.UNK_PAIRING_MODE);
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
        }
        
        Dialogs.ShowAsSingleInstanceOnDesktop(ref _popup); 
        _popupShown = true;
    }
    
    private void OnConnected(object? sender, EventArgs e)
    {
        _popupShown = false;
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
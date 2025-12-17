using System.Collections.ObjectModel;
using System.ComponentModel;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI;
using System.Text.Json.Serialization;

namespace GalaxyBudsClient.Model.Config;

// FIXME: Replace these classes with records.
public partial class TouchAction : ReactiveObject
{
    [Reactive]
    [property: JsonPropertyName("action")]
    private CustomActions _action;
    
    [Reactive]
    [property: JsonPropertyName("parameter")]
    private string _parameter = string.Empty;
}

public partial class Device : ReactiveObject
{
    [Reactive]
    [property: JsonPropertyName("model")]
    private Models _model = Models.NULL;
    
    [Reactive]
    [property: JsonPropertyName("macAddress")]
    private string _macAddress = string.Empty;
    
    [Reactive]
    [property: JsonPropertyName("name")]
    private string _name = string.Empty;
    
    [Reactive]
    [property: JsonPropertyName("deviceColor")]
    private DeviceIds? _deviceColor;
}
    
/*
 * Properties annotated with [ReadOnly] should not be re-assigned after initialization.
 * They cannot be marked as init-only because the JSON source generator doesn't support it.
 */
public partial class SettingsData : ReactiveObject
{
    /* Appearance */
    [Reactive] private Themes _theme = Settings.DefaultTheme;
    [Reactive] private uint _accentColor = Avalonia.Media.Colors.Orange.ToUInt32();
    [Reactive] private int _blurStrength = 144;
    [Reactive] private Locales _locale = Locales.en;
    [Reactive] private TemperatureUnits _temperatureUnit = TemperatureUnits.Celsius;
    [Reactive] private bool _realisticEarbudImages = true;
    [Reactive] private bool _showSidebar = Settings.DefaultShowSidebar;
    [Reactive] private DeviceIds? _colorOverride;
    
    /* Connections */
    [Reactive] private bool _useBluetoothWinRt = true;
    [ReadOnly(true)] public ObservableCollection<Device> Devices { set; get; } = [];
    [Reactive] private string? _lastDeviceMac;

    
    /* Touch actions */
    [ReadOnly(true)] public TouchAction CustomActionLeft { set; get; } = new();
    [ReadOnly(true)] public TouchAction CustomActionRight { set; get; } = new();

    /* Tray icon */
    [Reactive] private bool _minimizeToTray = true;
    [Reactive] private DynamicTrayIconModes _dynamicTrayIconMode;

    /* Connection popup */
    [Reactive] private bool _popupEnabled;
    [Reactive] private bool _popupCompact;
    [Reactive] private PopupPlacement _popupPlacement = PopupPlacement.BottomRight;
    
    /* Hotkey actions */
    [ReadOnly(true)] public ObservableCollection<Hotkey> Hotkeys { set; get; } = [];
    
    /* Crowdsourcing */
    [Reactive] private bool _experimentsDisabled;
    [ReadOnly(true)] public ObservableCollection<long> ExperimentsFinishedIds { set; get; } = [];
    [Reactive] private bool _disableCrashReporting;
    
    /* Developer */
    [Reactive] private bool _openDevToolsOnStartup;
    
    /* Battery stats */
    [Reactive] private bool _collectBatteryHistory = true;

    /* Other */
    [Reactive] private bool _firmwareWarningAccepted;
    [Reactive] private Event _bixbyRemapEvent;
    [Reactive] private bool _resumePlaybackOnSensor;
    [Reactive] private bool _pausePlaybackOnSensor;
    [Reactive] private bool _isUsageReportHintHidden;
    [Reactive] private bool _isBatteryHistoryHintHidden;
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI;

namespace GalaxyBudsClient.Model.Config;

/*
 * These types are serialized by the System.Text.Json source generator (SettingsSerializerContext)
 * for AOT support (Android). ReactiveUI.SourceGenerators output is not visible to the STJ source
 * generator (source generators cannot chain), and ReactiveUI.Fody is incompatible with ReactiveUI 26.
 * The reactive properties are therefore implemented manually with RaiseAndSetIfChanged so the STJ
 * source generator sees them as regular properties.
 */


public class TouchAction : ReactiveObject
{
    private CustomActions _action;
    public CustomActions Action { get => _action; set => this.RaiseAndSetIfChanged(ref _action, value); }
    private string _parameter = string.Empty;
    public string Parameter { get => _parameter; set => this.RaiseAndSetIfChanged(ref _parameter, value); }
}

public class Device : ReactiveObject
{
    private Models _model = Models.NULL;
    public Models Model { get => _model; set => this.RaiseAndSetIfChanged(ref _model, value); }
    private string _macAddress = string.Empty;
    public string MacAddress { get => _macAddress; set => this.RaiseAndSetIfChanged(ref _macAddress, value); }
    private string _name = string.Empty;
    public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
    private DeviceIds? _deviceColor;
    public DeviceIds? DeviceColor { get => _deviceColor; set => this.RaiseAndSetIfChanged(ref _deviceColor, value); }

    // Custom EQ band table is not retained by the firmware across power cycles, so persist it
    // here and re-apply on connect (see App.OnExtendedStatusUpdate). CustomEqualizerBands holds the
    // currently-active slot's curve (what gets re-pushed). The firmware has only one custom table,
    // so multiple "Custom" slots are stored app-side here and the active one is pushed on selection.
    private bool _customEqualizerEnabled;
    public bool CustomEqualizerEnabled { get => _customEqualizerEnabled; set => this.RaiseAndSetIfChanged(ref _customEqualizerEnabled, value); }
    private int[]? _customEqualizerBands;
    public int[]? CustomEqualizerBands { get => _customEqualizerBands; set => this.RaiseAndSetIfChanged(ref _customEqualizerBands, value); }
    private int _customEqualizerActiveSlot;
    public int CustomEqualizerActiveSlot { get => _customEqualizerActiveSlot; set => this.RaiseAndSetIfChanged(ref _customEqualizerActiveSlot, value); }
    private int[][]? _customEqualizerSlots;
    public int[][]? CustomEqualizerSlots { get => _customEqualizerSlots; set => this.RaiseAndSetIfChanged(ref _customEqualizerSlots, value); }
}
    
/*
 * Properties annotated with [ReadOnly] should not be re-assigned after initialization.
 * They cannot be marked as init-only because the JSON source generator doesn't support it.
 */
public class SettingsData : ReactiveObject
{
    /* Appearance */
    private Themes _theme = Settings.DefaultTheme;
    public Themes Theme { get => _theme; set => this.RaiseAndSetIfChanged(ref _theme, value); }
    private uint _accentColor = Avalonia.Media.Colors.Orange.ToUInt32();
    public uint AccentColor { get => _accentColor; set => this.RaiseAndSetIfChanged(ref _accentColor, value); }
    private int _blurStrength = 144;
    public int BlurStrength { get => _blurStrength; set => this.RaiseAndSetIfChanged(ref _blurStrength, value); }
    private Locales _locale = Locales.en;
    public Locales Locale { get => _locale; set => this.RaiseAndSetIfChanged(ref _locale, value); }
    private TemperatureUnits _temperatureUnit = TemperatureUnits.Celsius;
    public TemperatureUnits TemperatureUnit { get => _temperatureUnit; set => this.RaiseAndSetIfChanged(ref _temperatureUnit, value); }
    private bool _realisticEarbudImages = true;
    public bool RealisticEarbudImages { get => _realisticEarbudImages; set => this.RaiseAndSetIfChanged(ref _realisticEarbudImages, value); }
    private bool _showSidebar = Settings.DefaultShowSidebar;
    public bool ShowSidebar { get => _showSidebar; set => this.RaiseAndSetIfChanged(ref _showSidebar, value); }
    private DeviceIds? _colorOverride;
    public DeviceIds? ColorOverride { get => _colorOverride; set => this.RaiseAndSetIfChanged(ref _colorOverride, value); }
    
    /* Connections */
    private bool _useBluetoothWinRt = true;
    public bool UseBluetoothWinRt { get => _useBluetoothWinRt; set => this.RaiseAndSetIfChanged(ref _useBluetoothWinRt, value); }
    [ReadOnly(true)] public ObservableCollection<Device> Devices { set; get; } = [];
    private string? _lastDeviceMac;
    public string? LastDeviceMac { get => _lastDeviceMac; set => this.RaiseAndSetIfChanged(ref _lastDeviceMac, value); }
    private bool _autoEnableMultipoint;
    public bool AutoEnableMultipoint { get => _autoEnableMultipoint; set => this.RaiseAndSetIfChanged(ref _autoEnableMultipoint, value); }

    
    /* Touch actions */
    [ReadOnly(true)] public TouchAction CustomActionLeft { set; get; } = new();
    [ReadOnly(true)] public TouchAction CustomActionRight { set; get; } = new();

    /* Tray icon */
    private bool _minimizeToTray = true;
    public bool MinimizeToTray { get => _minimizeToTray; set => this.RaiseAndSetIfChanged(ref _minimizeToTray, value); }
    private DynamicTrayIconModes _dynamicTrayIconMode;
    public DynamicTrayIconModes DynamicTrayIconMode { get => _dynamicTrayIconMode; set => this.RaiseAndSetIfChanged(ref _dynamicTrayIconMode, value); }

    /* Connection popup */
    private bool _popupEnabled;
    public bool PopupEnabled { get => _popupEnabled; set => this.RaiseAndSetIfChanged(ref _popupEnabled, value); }
    private bool _popupCompact;
    public bool PopupCompact { get => _popupCompact; set => this.RaiseAndSetIfChanged(ref _popupCompact, value); }
    private PopupPlacement _popupPlacement = PopupPlacement.BottomRight;
    public PopupPlacement PopupPlacement { get => _popupPlacement; set => this.RaiseAndSetIfChanged(ref _popupPlacement, value); }
    
    /* Hotkey actions */
    [ReadOnly(true)] public ObservableCollection<Hotkey> Hotkeys { set; get; } = [];
    
    /* Crowdsourcing */
    private bool _experimentsDisabled;
    public bool ExperimentsDisabled { get => _experimentsDisabled; set => this.RaiseAndSetIfChanged(ref _experimentsDisabled, value); }
    [ReadOnly(true)] public ObservableCollection<long> ExperimentsFinishedIds { set; get; } = [];
    private bool _disableCrashReporting;
    public bool DisableCrashReporting { get => _disableCrashReporting; set => this.RaiseAndSetIfChanged(ref _disableCrashReporting, value); }
    
    /* Developer */
    private bool _openDevToolsOnStartup;
    public bool OpenDevToolsOnStartup { get => _openDevToolsOnStartup; set => this.RaiseAndSetIfChanged(ref _openDevToolsOnStartup, value); }
    
    /* Battery stats */
    private bool _collectBatteryHistory = true;
    public bool CollectBatteryHistory { get => _collectBatteryHistory; set => this.RaiseAndSetIfChanged(ref _collectBatteryHistory, value); }

    /* Other */
    private bool _firmwareWarningAccepted;
    public bool FirmwareWarningAccepted { get => _firmwareWarningAccepted; set => this.RaiseAndSetIfChanged(ref _firmwareWarningAccepted, value); }
    private Event _bixbyRemapEvent;
    public Event BixbyRemapEvent { get => _bixbyRemapEvent; set => this.RaiseAndSetIfChanged(ref _bixbyRemapEvent, value); }
    private bool _resumePlaybackOnSensor;
    public bool ResumePlaybackOnSensor { get => _resumePlaybackOnSensor; set => this.RaiseAndSetIfChanged(ref _resumePlaybackOnSensor, value); }
    private bool _pausePlaybackOnSensor;
    public bool PausePlaybackOnSensor { get => _pausePlaybackOnSensor; set => this.RaiseAndSetIfChanged(ref _pausePlaybackOnSensor, value); }
    private bool _isUsageReportHintHidden;
    public bool IsUsageReportHintHidden { get => _isUsageReportHintHidden; set => this.RaiseAndSetIfChanged(ref _isUsageReportHintHidden, value); }
    private bool _isBatteryHistoryHintHidden;
    public bool IsBatteryHistoryHintHidden { get => _isBatteryHistoryHintHidden; set => this.RaiseAndSetIfChanged(ref _isBatteryHistoryHintHidden, value); }
}

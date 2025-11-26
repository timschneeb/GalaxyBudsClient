using System.Collections.ObjectModel;
using System.ComponentModel;
using GalaxyBudsClient.Model.Constants;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Model.Config;

// FIXME: Replace these classes with records. ReactiveUI.Fody doesn't support IL weaving for records yet.
//        https://github.com/reactiveui/ReactiveUI/issues/2831
public class TouchAction : ReactiveObject
{
    [Reactive] public CustomActions Action { set; get; }
    [Reactive] public string Parameter { set; get; } = string.Empty;
}

public class Device : ReactiveObject
{
    [Reactive] public Models Model { set; get; } = Models.NULL;
    [Reactive] public string MacAddress { set; get; } = string.Empty;
    [Reactive] public string Name { set; get; } = string.Empty;
    [Reactive] public DeviceIds? DeviceColor { set; get; }
}
    
/*
 * Properties annotated with [ReadOnly] should not be re-assigned after initialization.
 * They cannot be marked as init-only because the JSON source generator doesn't support it.
 */
public class SettingsData : ReactiveObject
{
    /* Appearance */
    [Reactive] public Themes Theme { set; get; } = Settings.DefaultTheme;
    [Reactive] public uint AccentColor { set; get; } = Avalonia.Media.Colors.Orange.ToUInt32();
    [Reactive] public int BlurStrength { set; get; } = 144;
    [Reactive] public Locales Locale { set; get; } = Locales.en;
    [Reactive] public TemperatureUnits TemperatureUnit { set; get; } = TemperatureUnits.Celsius;
    [Reactive] public bool RealisticEarbudImages { set; get; } = true;
    [Reactive] public bool ShowSidebar { set; get; } = Settings.DefaultShowSidebar;
    [Reactive] public DeviceIds? ColorOverride { set; get; }
    
    /* Connections */
    [Reactive] public bool UseBluetoothWinRt { set; get; } = true;
    [ReadOnly(true)] public ObservableCollection<Device> Devices { set; get; } = [];
    [Reactive] public string? LastDeviceMac { set; get; }

    
    /* Touch actions */
    [ReadOnly(true)] public TouchAction CustomActionLeft { set; get; } = new();
    [ReadOnly(true)] public TouchAction CustomActionRight { set; get; } = new();

    /* Tray icon */
    [Reactive] public bool MinimizeToTray { set; get; } = true;
    [Reactive] public DynamicTrayIconModes DynamicTrayIconMode { set; get; }

    /* Connection popup */
    [Reactive] public bool PopupEnabled { set; get; }
    [Reactive] public bool PopupCompact { set; get; }
    [Reactive] public PopupPlacement PopupPlacement { set; get; } = PopupPlacement.BottomRight;
    
    /* Hotkey actions */
    [ReadOnly(true)] public ObservableCollection<Hotkey> Hotkeys { set; get; } = [];
    
    /* Crowdsourcing */
    [Reactive] public bool ExperimentsDisabled { set; get; }
    [ReadOnly(true)] public ObservableCollection<long> ExperimentsFinishedIds { set; get; } = [];
    [Reactive] public bool DisableCrashReporting { set; get; }
    
    /* Developer */
    [Reactive] public bool OpenDevToolsOnStartup { set; get; }
    
    /* Battery stats */
    [Reactive] public bool CollectBatteryHistory { set; get; } = true;

    /* Other */
    [Reactive] public bool FirmwareWarningAccepted { set; get; }
    [Reactive] public Event BixbyRemapEvent { set; get; }
    [Reactive] public bool ResumePlaybackOnSensor { set; get; }
    [Reactive] public bool PausePlaybackOnSensor { set; get; }
    [Reactive] public bool IsUsageReportHintHidden { set; get; }
    [Reactive] public bool IsBatteryHistoryHintHidden { set; get; }
}
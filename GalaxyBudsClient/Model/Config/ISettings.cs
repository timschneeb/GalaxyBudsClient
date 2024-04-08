using System;
using System.ComponentModel;
using Config.Net;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Hotkeys;
using Color = Avalonia.Media.Color;

namespace GalaxyBudsClient.Model.Config;

public interface ICustomAction : INotifyPropertyChanged
{
    CustomAction.Actions Action { set; get; }
    [Option(DefaultValue = "")] string Parameter { set; get; }
}
    
public interface IDevice : INotifyPropertyChanged
{
    [Option(DefaultValue = Models.NULL)] Models Model { set; get; }
    [Option(DefaultValue = "")] string MacAddress { set; get; }
    [Option(DefaultValue = "")] string Name { set; get; }
    Colors? DeviceColor { set; get; }
}
    
public interface IPopup : INotifyPropertyChanged
{
    bool Enabled { set; get; }
    bool Compact { set; get; }
    [Option(DefaultValue = PopupPlacement.BottomRight)] PopupPlacement Placement  { set; get; }
    [Obsolete("Setting removed")] bool ShowWearableState { set; get; }
    [Obsolete("Setting removed"), Option(DefaultValue = "")]  string CustomTitle { set; get; }
}
    
public interface IExperiments : INotifyPropertyChanged
{
    bool Disabled { set; get; }
    long[] FinishedIds { set; get; }
}
    
public interface ISettings : INotifyPropertyChanged
{
    /* Appearance */
    Themes? Theme { set; get; }
    Color AccentColor { set; get; }
    [Option(DefaultValue = 144)] int BlurStrength { set; get; }
    [Option(DefaultValue = Locales.en)] Locales Locale { set; get; }
    [Option(DefaultValue = TemperatureUnits.Celsius)] TemperatureUnits TemperatureUnit { set; get; }
    [Option(DefaultValue = true)] bool RealisticEarbudImages { set; get; }
    
    /* Connections */
    [Option(DefaultValue = true)] bool UseBluetoothWinRT { set; get; }
    [Obsolete("Setting removed"), Option(Alias = "RegisteredDevice")] IDevice DeviceLegacy { set; get; }
    IDevice[] Devices { set; get; }
    
    /* Touch actions */
    ICustomAction CustomActionLeft { set; get; }
    ICustomAction CustomActionRight { set; get; }

    /* Tray icon */    
    [Option(DefaultValue = true)] bool MinimizeToTray { set; get; }
    [Option(DefaultValue = DynamicTrayIconModes.Disabled)] DynamicTrayIconModes DynamicTrayIconMode { set; get; }

    /* Connection popup */
    IPopup Popup { set; get; }
    
    /* Hotkey actions */ 
    Hotkey[] Hotkeys { set; get; }
    
    /* Crowdsourcing */
    IExperiments Experiments { set; get; }
    bool DisableCrashReporting { set; get; }
    
    /* Other */
    [Option(DefaultValue = true)] bool FirstLaunch { set; get; }
    bool FirmwareWarningAccepted { set; get; }
    Event BixbyRemapEvent { set; get; }
    bool ResumePlaybackOnSensor { set; get; }
    
    /* Obsolete */
    [Obsolete("Setting removed"), Option(DefaultValue = "")] string UpdateSkippedVersion { set; get; }
}
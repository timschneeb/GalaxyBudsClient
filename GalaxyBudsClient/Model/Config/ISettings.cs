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
    [Option(DefaultValue = "")]
    string Parameter { set; get; }
}
    
public interface IDevice : INotifyPropertyChanged
{
    [Option(DefaultValue = Models.NULL)]
    Models Model { set; get; }
    [Option(DefaultValue = "")]
    string MacAddress { set; get; }
}
    
public interface IPopup : INotifyPropertyChanged
{
    bool Enabled { set; get; }
    bool Compact { set; get; }
    [Option(DefaultValue = PopupPlacement.BottomRight)]
    PopupPlacement Placement  { set; get; }
    [Obsolete("Setting removed")]
    bool ShowWearableState { set; get; }
    [Obsolete("Setting removed")]
    [Option(DefaultValue = "")]
    string CustomTitle { set; get; }
}
    
public interface IExperiments : INotifyPropertyChanged
{
    bool Disabled { set; get; }
    long[] FinishedIds { set; get; }
}
    
public interface ISettings : INotifyPropertyChanged
{
    [Option(DefaultValue = DarkModes.Dark)]
    DarkModes DarkMode { set; get; }
    Color AccentColor { set; get; }
    [Option(DefaultValue = Locales.en)]
        
    Locales Locale { set; get; }
    [Option(DefaultValue = TemperatureUnits.Celsius)]
        
    TemperatureUnits TemperatureUnit { set; get; }
    IDevice RegisteredDevice { set; get; }
        
    ICustomAction CustomActionLeft { set; get; }
    ICustomAction CustomActionRight { set; get; }

    IPopup Popup { set; get; }
        
    [Option(DefaultValue = "")]
    string UpdateSkippedVersion { set; get; }
    [Option(DefaultValue = true)]
    bool MinimizeToTray { set; get; }
    [Option(DefaultValue = DynamicTrayIconModes.Disabled)]
        
    DynamicTrayIconModes DynamicTrayIconMode { set; get; }
    bool ResumePlaybackOnSensor { set; get; }
        
    IExperiments Experiments { set; get; }
        
    Hotkey[] Hotkeys { set; get; }
        
    [Option(DefaultValue = true)]
    bool FirstLaunch { set; get; }
        
    [Option(DefaultValue = true)]
    bool UseBluetoothWinRT { set; get; }
        
    bool FirmwareWarningAccepted { set; get; }
        
    Event BixbyRemapEvent { set; get; }
        
    bool DisableCrashReporting { set; get; }
}
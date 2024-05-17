using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Config.Net;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Config.Legacy;

public interface ICustomAction : INotifyPropertyChanged
{
    CustomActions Action { set; get; }
    [Option(DefaultValue = "")] string Parameter { set; get; }
}
    
public interface IDevice : INotifyPropertyChanged
{
    [Option(DefaultValue = Models.NULL)] Models Model { set; get; }
    [Option(DefaultValue = "")] string MacAddress { set; get; }
    [Option(DefaultValue = "")] string Name { set; get; }
}
    
public interface IPopup : INotifyPropertyChanged
{
    bool Enabled { set; get; }
    bool Compact { set; get; }
    [Option(DefaultValue = PopupPlacement.BottomRight)] PopupPlacement Placement { set; get; }
}
    
public interface IExperiments : INotifyPropertyChanged
{
    bool Disabled { set; get; }
    long[] FinishedIds { set; get; }
}
    
[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface ILegacySettings : INotifyPropertyChanged
{
    /* Appearance */
    [Option(DefaultValue = Locales.en)] Locales Locale { set; get; }
    [Option(DefaultValue = TemperatureUnits.Celsius)] TemperatureUnits TemperatureUnit { set; get; }
    
    /* Connections */
    [Option(DefaultValue = true)] bool UseBluetoothWinRT { set; get; }
    IDevice RegisteredDevice { set; get; }
    
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
    bool FirmwareWarningAccepted { set; get; }
    Event BixbyRemapEvent { set; get; }
    bool ResumePlaybackOnSensor { set; get; }
}
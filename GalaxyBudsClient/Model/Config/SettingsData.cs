using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace GalaxyBudsClient.Model.Config;

public static class Settings
{
    public static SettingsData Data { get; } = Load();
    private static SettingsSerializerContext Context { get; } = new();
    private static string Path => PlatformUtils.CombineDataPath("settings.json");

    public static event PropertyChangedEventHandler? MainSettingsPropertyChanged;
    public static event PropertyChangedEventHandler? DevicePropertyChanged;
    public static event NotifyCollectionChangedEventHandler? DeviceCollectionChanged;
    
    static Settings()
    {
        Data.PropertyChanged += OnRootPropertyChanged;
        Data.Devices.CollectionChanged += OnDeviceCollectionChanged;
        Data.CustomActionLeft.PropertyChanged += OnTouchActionPropertyChanged;
        Data.CustomActionRight.PropertyChanged += OnTouchActionPropertyChanged;
    }

    private static void OnTouchActionPropertyChanged(object? sender, PropertyChangedEventArgs e) => Save();

    private static void OnRootPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Save();
        MainSettingsPropertyChanged?.Invoke(sender, e);
    }

    private static void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Save();
        DevicePropertyChanged?.Invoke(sender, e);
    }

    private static void OnDeviceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null) foreach (var f in e.NewItems.OfType<Device>()) f.PropertyChanged += OnDevicePropertyChanged;
        if (e.OldItems != null) foreach (var f in e.OldItems.OfType<Device>()) f.PropertyChanged -= OnDevicePropertyChanged;
        DeviceCollectionChanged?.Invoke(sender, e);
    }

    private static void OnHotkeyPropertyChanged(object? sender, PropertyChangedEventArgs e) => Save();

    private static void OnHotkeyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null) foreach (var f in e.NewItems.OfType<Hotkey>()) f.PropertyChanged += OnHotkeyPropertyChanged;
        if (e.OldItems != null) foreach (var f in e.OldItems.OfType<Hotkey>()) f.PropertyChanged -= OnHotkeyPropertyChanged;
    }


    private static void Save()
    {
        try
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            
            File.WriteAllText(Path, JsonSerializer.Serialize(Data, Data.GetType(), Context));
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to save settings");
        }
    }
    
    private static SettingsData Load()
    {
        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize(json, Data.GetType(), Context) as SettingsData ?? new SettingsData();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load settings");
            return new SettingsData();
        }
    }
}

public record TouchAction : ReactiveRecord
{
    [Reactive] public CustomAction.Actions Action { set; get; }
    [Reactive] public string Parameter { set; get; } = string.Empty;
}

public record Device : ReactiveRecord
{
    [Reactive] public Models Model { set; get; } = Models.NULL;
    [Reactive] public string MacAddress { set; get; } = string.Empty;
    [Reactive] public string Name { set; get; } = string.Empty;
    [Reactive] public Colors? DeviceColor { set; get; }
}
    
/*
 * Properties annotated with [ReadOnly] should not be re-assigned after initialization.
 * They cannot be marked as init-only because the JSON source generator doesn't support it.
 */
public record SettingsData : ReactiveRecord
{
    /* Appearance */
    [Reactive] public Themes Theme { set; get; } = PlatformUtils.SupportsMicaTheme ? Themes.DarkMica : Themes.DarkBlur;
    [Reactive] public uint AccentColor { set; get; } = Avalonia.Media.Colors.Orange.ToUInt32();
    [Reactive] public int BlurStrength { set; get; } = 144;
    [Reactive] public Locales Locale { set; get; } = Locales.en;
    [Reactive] public TemperatureUnits TemperatureUnit { set; get; } = TemperatureUnits.Celsius;
    [Reactive] public bool RealisticEarbudImages { set; get; } = true;
    
    /* Connections */
    [Reactive] public bool UseBluetoothWinRt { set; get; } = true;
    [ReadOnly(true)] public ObservableCollection<Device> Devices { set; get; } = [];
    
    /* Touch actions */
    [ReadOnly(true)] public TouchAction CustomActionLeft { set; get; } = new();
    [ReadOnly(true)] public TouchAction CustomActionRight { set; get; } = new();

    /* Tray icon */
    [Reactive] public bool MinimizeToTray { set; get; } = true;
    [Reactive] public DynamicTrayIconModes DynamicTrayIconMode { set; get; }

    /* Connection popup */
    [Reactive] public bool Enabled { set; get; }
    [Reactive] public bool Compact { set; get; }
    [Reactive] public PopupPlacement Placement { set; get; } = PopupPlacement.BottomRight;
    
    /* Hotkey actions */
    [ReadOnly(true)] public ObservableCollection<Hotkey> Hotkeys { set; get; } = [];
    
    /* Crowdsourcing */
    [Reactive] public bool ExperimentsDisabled { set; get; }
    [ReadOnly(true)] public List<int> ExperimentsFinishedIds { set; get; } = [];
    [Reactive] public bool DisableCrashReporting { set; get; }
    
    /* Other */
    [Reactive] public bool FirstLaunch { set; get; } = true;
    [Reactive] public bool FirmwareWarningAccepted { set; get; }
    [Reactive] public Event BixbyRemapEvent { set; get; }
    [Reactive] public bool ResumePlaybackOnSensor { set; get; }
}
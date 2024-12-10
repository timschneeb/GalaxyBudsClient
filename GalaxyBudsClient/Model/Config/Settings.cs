using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Json;
using Avalonia.Threading;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.Dialogs;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Model.Config;

public static class Settings
{
    public static SettingsData Data {  get; }
    private static SettingsSerializerContext Context { get; } = new();
    private static string Path => PlatformUtils.CombineDataPath("settings.json");

    public static event PropertyChangedEventHandler? MainSettingsPropertyChanged;
    public static event PropertyChangedEventHandler? DevicePropertyChanged;
    public static event PropertyChangedEventHandler? TouchActionPropertyChanged;
    public static event NotifyCollectionChangedEventHandler? DeviceCollectionChanged;
    public static event NotifyCollectionChangedEventHandler? HotkeyCollectionChanged;
    
    static Settings()
    {
        Log.Information("Using settings file: {Path}", Path);
        Data = Load();
        
        Data.PropertyChanged += OnRootPropertyChanged;
        Data.Devices.CollectionChanged += OnDeviceCollectionChanged;
        Data.Hotkeys.CollectionChanged += OnHotkeyCollectionChanged;
        Data.ExperimentsFinishedIds.CollectionChanged += (_, _) => Save();
        Data.CustomActionLeft.PropertyChanged += OnTouchActionPropertyChanged;
        Data.CustomActionRight.PropertyChanged += OnTouchActionPropertyChanged;
    }
    
    public static Themes DefaultTheme => PlatformUtils.SupportsMicaTheme ? Themes.DarkMica : 
        PlatformUtils.SupportsBlurTheme ? Themes.DarkBlur : Themes.Dark;
    
    public static bool DefaultShowSidebar => PlatformUtils.IsDesktop ? true : false;

    private static void OnTouchActionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Save();
        TouchActionPropertyChanged?.Invoke(sender, e);
    }

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
        Save();
    }

    private static void OnHotkeyPropertyChanged(object? sender, PropertyChangedEventArgs e) => Save();

    private static void OnHotkeyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null) foreach (var f in e.NewItems.OfType<Hotkey>()) f.PropertyChanged += OnHotkeyPropertyChanged;
        if (e.OldItems != null) foreach (var f in e.OldItems.OfType<Hotkey>()) f.PropertyChanged -= OnHotkeyPropertyChanged;
        HotkeyCollectionChanged?.Invoke(sender, e);
        Save();
    }
    
    private static void Save()
    {
        try
        {
            File.WriteAllText(Path, JsonSerializer.Serialize(Data, Data.GetType(), Context));
        }
        catch (Exception e)
        {
            if ((!PlatformUtils.IsDesktop || MainWindow.Instance.IsVisible) && e is UnauthorizedAccessException or SecurityException)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _ = new MessageBox
                    {
                        Title = Strings.Error,
                        Description = string.Format(Strings.SettingsSaveFailNoAccess, Path)
                    }.ShowAsync();
                });
            }
            
            Log.Error(e, "Failed to save settings");
        }
    }
    
    private static SettingsData Load()
    {
        try
        {
            var json = File.ReadAllText(Path);
            return JsonSerializer.Deserialize(json, typeof(SettingsData), Context) as SettingsData 
                   ?? throw new InvalidOperationException("Deserializer returned null");
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load settings");
            return new SettingsData();
        }
    }
}

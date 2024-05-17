using System;
using System.IO;
using Config.Net;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Model.Config.Legacy;

public static class LegacySettings
{
    public static ILegacySettings? Instance { get; }
    public static string Path => PlatformUtils.CombineDataPath("config.json");

    static LegacySettings()
    {
        if (!File.Exists(Path)) 
            return;
        
        Instance = new ConfigurationBuilder<ILegacySettings>()
            .UseJsonFile(Path)
            .UseTypeParser(new ConfigArrayParser<long>())
            .UseTypeParser(new HotkeyArrayParser())
            .Build();
    }

    public static void BeginMigration()
    {
        if(Instance == null)
            return;

        Log.Information("Migrating old settings file: {SettingsPath}", Path);
        Settings.Data.Locale = Instance.Locale;
        Settings.Data.TemperatureUnit = Instance.TemperatureUnit;
        Settings.Data.UseBluetoothWinRt = Instance.UseBluetoothWinRT;
        
        Settings.Data.CustomActionLeft.Action = Instance.CustomActionLeft.Action;
        Settings.Data.CustomActionLeft.Parameter = Instance.CustomActionLeft.Parameter;
        Settings.Data.CustomActionRight.Action = Instance.CustomActionRight.Action;
        Settings.Data.CustomActionRight.Parameter = Instance.CustomActionRight.Parameter;
        
        Settings.Data.MinimizeToTray = Instance.MinimizeToTray;
        Settings.Data.DynamicTrayIconMode = Instance.DynamicTrayIconMode;
        
        Settings.Data.PopupEnabled = Instance.Popup.Enabled;
        Settings.Data.PopupCompact = Instance.Popup.Compact;
        Settings.Data.PopupPlacement = Instance.Popup.Placement;

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        foreach (var id in Instance.Hotkeys ?? Array.Empty<Hotkey>())
        {
            Settings.Data.Hotkeys.Add(id);
        }
        
        Settings.Data.DisableCrashReporting = Instance.DisableCrashReporting;
        Settings.Data.ExperimentsDisabled = Instance.Experiments.Disabled;
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        foreach (var id in Instance.Experiments.FinishedIds ?? Array.Empty<long>())
        {
            Settings.Data.ExperimentsFinishedIds.Add(id);
        }
        
        Settings.Data.FirmwareWarningAccepted = Instance.FirmwareWarningAccepted;
        Settings.Data.BixbyRemapEvent = Instance.BixbyRemapEvent;
        Settings.Data.ResumePlaybackOnSensor = Instance.ResumePlaybackOnSensor;
        

        Settings.Data.Devices.Add(new Device
        {
            Model = Instance.RegisteredDevice.Model,
            MacAddress = Instance.RegisteredDevice.MacAddress,
            Name = Instance.RegisteredDevice.Name
        });
        
        File.Delete(Path);
    }
}
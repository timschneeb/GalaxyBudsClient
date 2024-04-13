using System.ComponentModel;
using Config.Net;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Serilog;

namespace GalaxyBudsClient.Utils;

public static class LegacySettings
{
    public static ISettings Instance { get; }
    private static string SettingsPath => PlatformUtils.CombineDataPath("config.json");

    static LegacySettings()
    {
        Log.Information("Using settings file at: {SettingsPath}", SettingsPath);
        Instance = new ConfigurationBuilder<ISettings>()
            .UseJsonFile(SettingsPath)
            .UseTypeParser(new ConfigArrayParser<long>())
            .UseTypeParser(new HotkeyArrayParser())
            .UseTypeParser(new DeviceArrayParser())
            .UseTypeParser(new AccentColorParser())
            .Build();
            
        Instance.PropertyChanged += OnMainSettingsChanged;

        // Load default theme based on platform
        Instance.Theme ??= PlatformUtils.IsWindows ? Themes.DarkMica : Themes.DarkBlur;
    }

    private static void OnMainSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISettings.Locale):
                Loc.Load();
                break;
            case nameof(ISettings.DynamicTrayIconMode):
            {
                var cache = DeviceMessageCache.Instance.BasicStatusUpdate;
                if (Instance.DynamicTrayIconMode != DynamicTrayIconModes.Disabled && BluetoothImpl.Instance.IsConnected && cache != null)
                    WindowIconRenderer.UpdateDynamicIcon(cache);
                else
                    WindowIconRenderer.ResetIconToDefault();
                break;
            }
        }
    }
}
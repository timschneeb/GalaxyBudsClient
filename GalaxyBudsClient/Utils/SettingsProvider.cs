using Config.Net;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Utils
{
    public static class SettingsProvider
    {
        public static ISettings Instance { get; }
        static SettingsProvider()
        {
            Log.Information("Using settings file at: {SettingsPath}", SettingsPath);
            Instance = new ConfigurationBuilder<ISettings>()
                .UseJsonFile(SettingsPath)
                .UseTypeParser(new ConfigArrayParser<long>())
                .UseTypeParser(new HotkeyArrayParser())
                .Build();
        }

        private static string SettingsPath => PlatformUtils.CombineDataPath("config.json");
    }
}
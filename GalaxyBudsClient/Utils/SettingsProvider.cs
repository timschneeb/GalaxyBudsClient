using Config.Net;
using GalaxyBudsClient.Model;
using Serilog;

namespace GalaxyBudsClient.Utils
{
    public static class SettingsProvider
    {
        public static ISettings Instance { get; }
        static SettingsProvider()
        {
            Log.Information($"Using settings file at: {SettingsPath}");
            Instance = new ConfigurationBuilder<ISettings>()
                .UseJsonFile(SettingsPath)
                .Build();
        }

        public static string SettingsPath
        {
            get
            {
                if (PlatformUtils.IsPlatformSupported())
                {
                    return PlatformUtils.CombineDataPath("config.json");
                }
                else 
                {
                    Log.Fatal("SettingsProvider: Unsupported OS");
                }
                return "";
            }
        }
    }
}
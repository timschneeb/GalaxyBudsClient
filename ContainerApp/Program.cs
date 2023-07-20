using System;
using Avalonia;
using Avalonia.Controls;
using Serilog;

namespace ContainerApp
{
    internal static class Program
    {
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();

        private static void Main(string[] args)
        {
            
            var config = new LoggerConfiguration()
                .WriteTo.Console();

            config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ? 
                config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
            Log.Logger = config.CreateLogger();
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        }
    }
}
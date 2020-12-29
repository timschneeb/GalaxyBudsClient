using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Threading;
using Config.Net;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Serilog;
using Serilog.Filters;

namespace GalaxyBudsClient
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static async Task Main(string[] args)
        {
            var config = new LoggerConfiguration()
                .WriteTo.File(PlatformUtils.CombineDataPath("application.log"));

            config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ? 
                config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
            config = PlatformUtils.IsWindows ? 
                config.WriteTo.Debug() : config.WriteTo.Console();
            Log.Logger =  config.CreateLogger();
            
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Trace.Listeners.Add(new ConsoleTraceListener());

            await SingleInstanceWatcher.Setup();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
        } 

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();


    }
}
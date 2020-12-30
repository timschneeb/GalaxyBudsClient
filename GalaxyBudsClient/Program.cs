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
using CSScriptLib;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Scripting;
using GalaxyBudsClient.Utils;
using Sentry;
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
                .WriteTo.Sentry(o =>
                {
                    o.MinimumBreadcrumbLevel = Serilog.Events.LogEventLevel.Debug;
                    o.MinimumEventLevel = Serilog.Events.LogEventLevel.Fatal;
                })
                .WriteTo.File(PlatformUtils.CombineDataPath("application.log"));

            config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ? 
                config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
            config = PlatformUtils.IsWindows ? 
                config.WriteTo.Debug() : config.WriteTo.Console();
            Log.Logger =  config.CreateLogger();
            
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            Trace.Listeners.Add(new ConsoleTraceListener());

            SentrySdk.Init(o =>
            {
                o.Dsn = "https://4591394c5fd747b0ab7f5e81297c094d@o456940.ingest.sentry.io/5462682";
                o.MaxBreadcrumbs = 120;
                o.SendDefaultPii = true;
#if DEBUG
                o.Environment = "staging";
#else
                o.Environment = "beta";
#endif
                o.BeforeSend = sentryEvent =>
                {
                    sentryEvent.SetTag("bluetooth-mac", SettingsProvider.Instance.RegisteredDevice.MacAddress);
                    sentryEvent.SetTag("bluetooth-model", BluetoothImpl.Instance.ActiveModel.ToString());
                    sentryEvent.SetTag("sw-version", DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");
                    
                    sentryEvent.SetExtra("bluetooth-mac", SettingsProvider.Instance.RegisteredDevice.MacAddress);
                    sentryEvent.SetExtra("bluetooth-model", BluetoothImpl.Instance.ActiveModel);
                    sentryEvent.SetExtra("bluetooth-model-saved", SettingsProvider.Instance.RegisteredDevice.Model);
                    sentryEvent.SetExtra("bluetooth-connected", BluetoothImpl.Instance.IsConnected);
                    sentryEvent.SetExtra("custom-locale", SettingsProvider.Instance.Locale);
                    sentryEvent.SetExtra("sw-version", DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");

                    sentryEvent.SetExtra("current-page", MainWindow.Instance.Pager.CurrentPage);

                    return sentryEvent;
                };
            });

            try
            {
                await SingleInstanceWatcher.Setup();
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Log.Error(ex.ToString());
            }
        } 

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();


    }
}
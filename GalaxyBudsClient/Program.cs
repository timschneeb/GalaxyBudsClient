using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GalaxyBudsClient.Cli;
using GalaxyBudsClient.Cli.Ipc;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Sentry;
using Serilog;

namespace GalaxyBudsClient
{
    internal static class Program
    {
        public static long StartedAt = 0;
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            StartedAt = Stopwatch.GetTimestamp();
            
            var config = new LoggerConfiguration()
                .WriteTo.Sentry(o =>
                {
                    o.MinimumBreadcrumbLevel = Serilog.Events.LogEventLevel.Debug;
                    o.MinimumEventLevel = Serilog.Events.LogEventLevel.Fatal;
                })
                .WriteTo.File(PlatformUtils.CombineDataPath("application.log"))
                .WriteTo.Console();

            config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ?
                config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
            // Divert program startup flow if the app was started with arguments (except /StartMinimized)
            var cliMode = args.Length > 0 && !args.Contains("/StartMinimized");
            if (cliMode)
            {
                // Disable excessive logging in CLI mode
                config = config.MinimumLevel.Warning();
            }
            
            Log.Logger = config.CreateLogger();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Trace.Listeners.Add(new ConsoleTraceListener());
            
            if (!SettingsProvider.Instance.DisableCrashReporting)
            {
                SentrySdk.Init(o =>
                {
                    o.Dsn = "https://4591394c5fd747b0ab7f5e81297c094d@o456940.ingest.sentry.io/5462682";
                    o.MaxBreadcrumbs = 120;
                    o.SendDefaultPii = true;
                    o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
                    o.Environment = "staging";
#else
                    o.Environment = "production";
#endif
                    o.BeforeSend = sentryEvent =>
                    {
                        try
                        {
                            sentryEvent.SetTag("arch",
                                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
                            sentryEvent.SetTag("bluetooth-mac", SettingsProvider.Instance.RegisteredDevice.MacAddress);
                            sentryEvent.SetTag("sw-version",
                                DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");

                            sentryEvent.SetExtra("arch",
                                System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
                            sentryEvent.SetExtra("bluetooth-mac",
                                SettingsProvider.Instance.RegisteredDevice.MacAddress);
                            sentryEvent.SetExtra("bluetooth-model-saved",
                                SettingsProvider.Instance.RegisteredDevice.Model);
                            sentryEvent.SetExtra("custom-locale", SettingsProvider.Instance.Locale);
                            sentryEvent.SetExtra("sw-version",
                                DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");

                            if (MainWindow.IsReady())
                            {
                                sentryEvent.SetExtra("current-page", MainWindow.Instance.Pager.CurrentPage);
                            }
                            else
                            {
                                sentryEvent.SetExtra("current-page", "instance_not_initialized");
                            }

                            sentryEvent.SetTag("bluetooth-model", BluetoothImpl.Instance.ActiveModel.ToString());
                            sentryEvent.SetExtra("bluetooth-model", BluetoothImpl.Instance.ActiveModel);
                            sentryEvent.SetExtra("bluetooth-sku", DeviceMessageCache.Instance.DebugSku?.LeftSku ?? "null");
                            sentryEvent.SetExtra("bluetooth-connected", BluetoothImpl.Instance.IsConnected);
                        }
                        catch (Exception ex)
                        {
                            sentryEvent.SetExtra("beforesend-error", ex);
                            Log.Error("Sentry.BeforeSend: Error while adding attachments: " + ex.Message);
                        }

                        return sentryEvent;
                    };
                });
            }
            
            if (cliMode)
            {
                CliHandler.ProcessArguments(args);
                return;
            }
            
            try
            {
                /* OSX: Graphics must be drawn on the main thread.
                 * Awaiting this call would implicitly cause the next code to run as a async continuation task.
                 * 
                 * In general: Don't await this call to shave off about 1000ms of startup time.
                 * The IpcService will terminate the app in time if another instance is already running.
                 */
                _ = Task.Run(IpcService.Setup);
                
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnExplicitShutdown);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                Log.Error(ex, "Unhandled exception in main thread");
            }
        } 

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .With(new MacOSPlatformOptions
                {
                    // https://github.com/AvaloniaUI/Avalonia/issues/14577
                    DisableSetProcessName = true
                })
                .With(new FontManagerOptions()
                {
                    // https://github.com/AvaloniaUI/Avalonia/issues/4427#issuecomment-1295012860
                    DefaultFamilyName = "avares://GalaxyBudsClient/Resources/fonts#Noto Sans"
                })
                .UsePlatformDetect()
                .LogToTrace();

    }
}

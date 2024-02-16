using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Sentry;
using Serilog;

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
                .WriteTo.File(PlatformUtils.CombineDataPath("application.log"))
                .WriteTo.Console();

            config = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VERBOSE")) ?
                config.MinimumLevel.Verbose() : config.MinimumLevel.Debug();
            
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
            else
            {
                Log.Information("App crash reports disabled by user");
            }

            /* Fix Avalonia font issue */
            // TODO implement an actual fix
            if (PlatformUtils.IsLinux)
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                }
                catch (CultureNotFoundException ex)
                {
                    
                }
            }

            try
            {
                // OSX: Graphics must be drawn on the main thread.
                // Awaiting this call would implicitly cause the next code to run as a async continuation task
                if (PlatformUtils.IsOSX)
                {
                    SingleInstanceWatcher.Setup().Wait();
                }
                else
                {
                    await SingleInstanceWatcher.Setup();
                }

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
                .With(new MacOSPlatformOptions
                {
                    // https://github.com/AvaloniaUI/Avalonia/issues/14577
                    DisableSetProcessName = true
                })
                .UsePlatformDetect()
                .LogToTrace();

    }
}

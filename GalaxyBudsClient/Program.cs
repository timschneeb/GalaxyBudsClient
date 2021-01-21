using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
using ThePBone.Interop.Win32;

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

            SentrySdk.Init(o =>
            {
                o.Dsn = "https://4591394c5fd747b0ab7f5e81297c094d@o456940.ingest.sentry.io/5462682";
                o.MaxBreadcrumbs = 120;
                o.SendDefaultPii = true;
#if DEBUG
                o.Environment = "staging";
#else
                o.Environment = "production";
#endif
                o.BeforeSend = sentryEvent =>
                {
                    try
                    {
                        sentryEvent.SetTag("arch", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
                        sentryEvent.SetTag("bluetooth-mac", SettingsProvider.Instance.RegisteredDevice.MacAddress);
                        sentryEvent.SetTag("sw-version",
                            DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");
                        
                        sentryEvent.SetExtra("arch", System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString());
                        sentryEvent.SetExtra("bluetooth-mac", SettingsProvider.Instance.RegisteredDevice.MacAddress);
                        sentryEvent.SetExtra("bluetooth-model-saved", SettingsProvider.Instance.RegisteredDevice.Model);
                        sentryEvent.SetExtra("custom-locale", SettingsProvider.Instance.Locale);          
                        sentryEvent.SetExtra("sw-version",
                            DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");                 
                                          
                        sentryEvent.SetExtra("current-page", MainWindow.Instance.Pager.CurrentPage);
                        
                        sentryEvent.SetTag("bluetooth-model", BluetoothImpl.Instance.ActiveModel.ToString());
                        sentryEvent.SetExtra("bluetooth-model", BluetoothImpl.Instance.ActiveModel);
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

            /* Fix Avalonia font issue */
            if (PlatformUtils.IsLinux)
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                }
                catch (CultureNotFoundException ex)
                {
                    Log.Warning("Startup: Culture en-US unavailable. Falling back to C. " + ex);
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("C");
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("C");
                }
            }

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
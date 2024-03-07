using System;
using System.Reflection;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using Sentry;
using Serilog;

namespace GalaxyBudsClient.Utils;

public static class CrashReports
{
    public static void SetupCrashHandler()
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

                    if (MainWindow2.IsReady())
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
                    Log.Error(ex, "Sentry.BeforeSend: Error while adding attachments");
                }

                return sentryEvent;
            };
        });
    }
}
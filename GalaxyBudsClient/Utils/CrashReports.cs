using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
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
            o.SendDefaultPii = false;
            o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
#if DEBUG
            o.Environment = "staging";
#else
            o.Environment = "production";
#endif
            o.SetBeforeSend(sentryEvent =>
            {
                try
                {
                    sentryEvent.SetTag("arch",
                        RuntimeInformation.ProcessArchitecture.ToString());
                    sentryEvent.SetTag("sw-version",
                        DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");

                    sentryEvent.SetExtra("arch",
                        RuntimeInformation.ProcessArchitecture.ToString());
                    sentryEvent.SetExtra("bluetooth-mac",
                        BitConverter.ToString(SHA1.HashData(Encoding.ASCII.GetBytes(LegacySettings.Instance.DeviceLegacy.MacAddress))).Replace("-", ""));
                    sentryEvent.SetExtra("bluetooth-model-saved",
                        LegacySettings.Instance.DeviceLegacy.Model);
                    sentryEvent.SetExtra("custom-locale", LegacySettings.Instance.Locale);
                    sentryEvent.SetExtra("sw-version",
                        DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "null");
                    
                    sentryEvent.SetTag("bluetooth-model", BluetoothImpl.ActiveModel.ToString());
                    sentryEvent.SetExtra("bluetooth-model", BluetoothImpl.ActiveModel);
                    sentryEvent.SetExtra("bluetooth-sku", DeviceMessageCache.Instance.DebugSku?.LeftSku ?? "null");
                    sentryEvent.SetExtra("bluetooth-connected", BluetoothImpl.Instance.IsConnected);
                }
                catch (Exception ex)
                {
                    sentryEvent.SetExtra("beforesend-error", ex);
                    Log.Error(ex, "Sentry.BeforeSend: Error while adding attachments");
                }

                return sentryEvent;
            });
        });
    }
}
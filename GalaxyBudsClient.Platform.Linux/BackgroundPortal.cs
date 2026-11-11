using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Serilog;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace GalaxyBudsClient.Platform.Linux;

[DBusInterface("org.freedesktop.portal.Background")]
interface IBackground : IDBusObject
{
    Task<ObjectPath> RequestBackgroundAsync(string parentWindow, IDictionary<string, object> options);
}

[DBusInterface("org.freedesktop.portal.Request")]
interface IRequest : IDBusObject
{
    Task<IDisposable> WatchResponseAsync(Action<(uint response, IDictionary<string, object> results)> handler, Action<Exception>? onError = null);
}

/* XDG Background portal; used to manage the autostart entry from within the Flatpak sandbox */
public static class BackgroundPortal
{
    private const string ServiceName = "org.freedesktop.portal.Desktop";
    private const string ObjectPath = "/org/freedesktop/portal/desktop";

    public static async Task RequestAutoStartAsync(bool enable)
    {
        try
        {
            using var connection = new Connection(Address.Session);
            var info = await connection.ConnectAsync();

            // Subscribe to the predicted request path before calling the portal to avoid missing the response
            var token = $"galaxybudsclient_{Guid.NewGuid():N}";
            var sender = info.LocalName.TrimStart(':').Replace('.', '_');
            var request = connection.CreateProxy<IRequest>(ServiceName, $"{ObjectPath}/request/{sender}/{token}");

            var result = new TaskCompletionSource<(uint response, IDictionary<string, object> results)>();
            using var watcher = await request.WatchResponseAsync(r => result.TrySetResult(r), ex => result.TrySetException(ex));

            var portal = connection.CreateProxy<IBackground>(ServiceName, ObjectPath);
            await portal.RequestBackgroundAsync(string.Empty, new Dictionary<string, object>
            {
                { "handle_token", token },
                { "reason", "Launch Galaxy Buds Manager minimized on login" },
                { "autostart", enable },
                { "commandline", new[] { "GalaxyBudsClient", "/StartMinimized" } },
                { "dbus-activatable", false }
            });

            if (await Task.WhenAny(result.Task, Task.Delay(TimeSpan.FromMinutes(1))) != result.Task)
            {
                Log.Warning("Linux.BackgroundPortal: No response received for autostart request (enable={Enable})", enable);
                return;
            }

            var (response, results) = await result.Task;
            results.TryGetValue("autostart", out var autostart);
            // A denied request (response=1) still leaves autostart disabled, so only compare the resulting state
            if (autostart is bool enabled && enabled == enable)
                Log.Information("Linux.BackgroundPortal: Autostart {State}", enable ? "enabled" : "disabled");
            else
                Log.Warning("Linux.BackgroundPortal: Autostart request (enable={Enable}) not fulfilled; response={Response}, autostart={Autostart}",
                    enable, response, autostart);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Linux.BackgroundPortal: Autostart request failed");
        }
    }
}

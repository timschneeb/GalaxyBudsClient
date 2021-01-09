using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace ThePBone.MprisClient
{
    [DBusInterface("org.freedesktop.DBus")]
    interface IDBus : IDBusObject
    {
        Task<string> HelloAsync();
        Task<uint> RequestNameAsync(string arg0, uint arg1);
        Task<uint> ReleaseNameAsync(string arg0);
        Task<uint> StartServiceByNameAsync(string arg0, uint arg1);
        Task UpdateActivationEnvironmentAsync(IDictionary<string, string> arg0);
        Task<bool> NameHasOwnerAsync(string arg0);
        Task<string[]> ListNamesAsync();
        Task<string[]> ListActivatableNamesAsync();
        Task AddMatchAsync(string arg0);
        Task RemoveMatchAsync(string arg0);
        Task<string> GetNameOwnerAsync(string arg0);
        Task<string[]> ListQueuedOwnersAsync(string arg0);
        Task<uint> GetConnectionUnixUserAsync(string arg0);
        Task<uint> GetConnectionUnixProcessIDAsync(string arg0);
        Task<byte[]> GetAdtAuditSessionDataAsync(string arg0);
        Task<byte[]> GetConnectionSELinuxSecurityContextAsync(string arg0);
        Task ReloadConfigAsync();
        Task<string> GetIdAsync();
        Task<IDictionary<string, object>> GetConnectionCredentialsAsync(string arg0);
        Task<IDisposable> WatchNameOwnerChangedAsync(Action<(string, string, string)> handler, Action<Exception>? onError = null);
        Task<IDisposable> WatchNameLostAsync(Action<string> handler, Action<Exception>? onError = null);
        Task<IDisposable> WatchNameAcquiredAsync(Action<string> handler, Action<Exception>? onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<DBusProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class DBusProperties
    {
        private string[]? _Features = default(string[]);
        public string[]? Features
        {
            get
            {
                return _Features;
            }

            set
            {
                _Features = (value);
            }
        }

        private string[]? _Interfaces = default(string[]);
        public string[]? Interfaces
        {
            get
            {
                return _Interfaces;
            }

            set
            {
                _Interfaces = (value);
            }
        }
    }

    static class DBusExtensions
    {
        public static Task<string[]> GetFeaturesAsync(this IDBus o) => o.GetAsync<string[]>("Features");
        public static Task<string[]> GetInterfacesAsync(this IDBus o) => o.GetAsync<string[]>("Interfaces");
    }

    [DBusInterface("org.freedesktop.DBus.Monitoring")]
    interface IMonitoring : IDBusObject
    {
        Task BecomeMonitorAsync(string[] arg0, uint arg1);
    }

    [DBusInterface("org.freedesktop.DBus.Debug.Stats")]
    interface IStats : IDBusObject
    {
        Task<IDictionary<string, object>> GetStatsAsync();
        Task<IDictionary<string, object>> GetConnectionStatsAsync(string arg0);
        Task<IDictionary<string, string[]>> GetAllMatchRulesAsync();
    }
}
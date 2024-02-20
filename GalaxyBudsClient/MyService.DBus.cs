using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace MyService.DBus
{
    [DBusInterface("me.timschneeberger.galaxybudsclient.Application")]
    interface IApplication : IDBusObject
    {
        Task ActivateAsync();
        Task ShowBatteryPopupAsync();
        
        Task<T> GetAsync<T>(string prop);
        Task<ApplicationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class ApplicationProperties
    {
        private string _AppVersion = default(string);
        public string AppVersion
        {
            get
            {
                return _AppVersion;
            }

            set
            {
                _AppVersion = (value);
            }
        }

        private bool _HasActiveRegistration = default(bool);
        public bool HasActiveRegistration
        {
            get
            {
                return _HasActiveRegistration;
            }

            set
            {
                _HasActiveRegistration = (value);
            }
        }
    }

    static class ApplicationExtensions
    {
        public static Task<string> GetAppVersionAsync(this IApplication o) => o.GetAsync<string>("AppVersion");
        public static Task<bool> GetHasActiveRegistrationAsync(this IApplication o) => o.GetAsync<bool>("HasActiveRegistration");
    }
}
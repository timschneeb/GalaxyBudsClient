using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public static class BlueZManager
    {
        // Keep old method name for backward compatibility with InTheHand.Net.Personal.Core
        public static async Task<bool> isBlueZ5()
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            try
            {
                _ = await objectManager.GetManagedObjectsAsync().ConfigureAwait(false);
            }
            catch (DBusException)
            {
                return false;
            }

            return true;
        }
        
        public static async Task<Adapter> GetAdapterAsync(string adapterName)
        {
            var adapterObjectPath = $"/org/bluez/{adapterName}";
            var adapter = Connection.System.CreateProxy<IAdapter1>(BluezConstants.DbusService, adapterObjectPath);

            try
            {
                await adapter.GetAliasAsync().ConfigureAwait(false);
            }
            catch (DBusException ex)
            {
                throw new BlueZException(ex);
            }

            return await Adapter.CreateAsync(adapter).ConfigureAwait(false);
        }

        public static async Task<IReadOnlyList<Adapter>> GetAdaptersAsync()
        {
            var adapters = await GetProxiesAsync<IAdapter1>(BluezConstants.AdapterInterface, rootObject: null).ConfigureAwait(false);

            return await Task.WhenAll(adapters.Select(Adapter.CreateAsync)).ConfigureAwait(false);
        }

        internal static async Task<IReadOnlyList<T>> GetProxiesAsync<T>(string interfaceName, IDBusObject? rootObject)
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            var objects = await objectManager.GetManagedObjectsAsync().ConfigureAwait(false);

            var matchingObjectPaths = objects
                .Where(obj => IsMatch(interfaceName, obj.Key, obj.Value, rootObject))
                .Select(obj => obj.Key);

            var proxies = matchingObjectPaths
                .Select(objectPath => Connection.System.CreateProxy<T>(BluezConstants.DbusService, objectPath))
                .ToList();

            return proxies;
        }

        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces, IDBusObject? rootObject)
        {
            return IsMatch(interfaceName, objectPath, interfaces.Keys, rootObject);
        }

        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, ICollection<string> interfaces, IDBusObject? rootObject)
        {
            if (rootObject != null && !objectPath.ToString().StartsWith($"{rootObject.ObjectPath}/", StringComparison.Ordinal))
            {
                return false;
            }

            return interfaces.Contains(interfaceName);
        }
    }
}
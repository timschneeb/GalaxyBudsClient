using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public static class BlueZManager
    {
        public static async Task<bool> isBlueZ5()
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            try
            {
                var objects = await objectManager.GetManagedObjectsAsync();
            }
            catch(DBusException)
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
                await adapter.GetAliasAsync();
            }
            catch (DBusException ex)
            {
                throw new BlueZException(ex);
            }

            return await Adapter.CreateAsync(adapter);
        }

        public static async Task<IReadOnlyList<Adapter>> GetAdaptersAsync()
        {
            var adapters = await GetProxiesAsync<IAdapter1>(BluezConstants.AdapterInterface, rootObject: null);

            return await Task.WhenAll(adapters.Select(Adapter.CreateAsync));
        }

        internal static async Task<IReadOnlyList<T>> GetProxiesAsync<T>(string interfaceName, IDBusObject rootObject)
        {
            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            var objects = await objectManager.GetManagedObjectsAsync();

            var matchingObjectPaths = objects
                .Where(obj => IsMatch(interfaceName, obj.Key, obj.Value, rootObject))
                .Select(obj => obj.Key);

            var proxies = matchingObjectPaths
                .Select(objectPath => Connection.System.CreateProxy<T>(BluezConstants.DbusService, objectPath))
                .ToList();

            return proxies;
        }

        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces, IDBusObject rootObject)
        {
            return IsMatch(interfaceName, objectPath, interfaces.Keys, rootObject);
        }

        internal static bool IsMatch(string interfaceName, ObjectPath objectPath, ICollection<string> interfaces, IDBusObject rootObject)
        {
            if (rootObject != null && !objectPath.ToString().StartsWith($"{rootObject.ObjectPath}/"))
            {
                return false;
            }

            return interfaces.Contains(interfaceName);
        }
    }
}
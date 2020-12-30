using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public delegate Task DeviceChangeEventHandlerAsync(Adapter sender, DeviceFoundEventArgs eventArgs);

    public delegate Task AdapterEventHandlerAsync(Adapter sender, BlueZEventArgs eventArgs);
    
    public class Adapter : IAdapter1, IDisposable
    {
        ~Adapter()
        {
            Dispose();
        }

        internal static async Task<Adapter> CreateAsync(IAdapter1 proxy)
        {
            var adapter = new Adapter
            {
                _proxy = proxy,
            };

            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            adapter._interfacesWatcher = await objectManager.WatchInterfacesAddedAsync(adapter.OnDeviceAdded);
            adapter._propertyWatcher = await proxy.WatchPropertiesAsync(adapter.OnPropertyChanges);

            return adapter;
        }

        public void Dispose()
        {
            _interfacesWatcher?.Dispose();
            _interfacesWatcher = null;

            GC.SuppressFinalize(this);
        }

        public event DeviceChangeEventHandlerAsync DeviceFound
        {
            add
            {
                _deviceFound += value;
                FireEventForExistingDevicesAsync();
            }
            remove
            {
                _deviceFound -= value;
            }
        }

        public event AdapterEventHandlerAsync PoweredOn
        {
            add
            {
                _poweredOn += value;
                FireEventIfPropertyAlreadyTrueAsync(_poweredOn, "Powered");
            }
            remove
            {
                _poweredOn -= value;
            }
        }

        public event AdapterEventHandlerAsync PoweredOff;

        public ObjectPath ObjectPath => _proxy.ObjectPath;

        public Task<Adapter1Properties> GetAllAsync()
        {
            return _proxy.GetAllAsync();
        }

        public Task<T> GetAsync<T>(string prop)
        {
            return _proxy.GetAsync<T>(prop);
        }

        public Task<string[]> GetDiscoveryFiltersAsync()
        {
            return _proxy.GetDiscoveryFiltersAsync();
        }

        public Task RemoveDeviceAsync(ObjectPath device)
        {
            return _proxy.RemoveDeviceAsync(device);
        }

        public Task SetAsync(string prop, object val)
        {
            return _proxy.SetAsync(prop, val);
        }

        public Task SetDiscoveryFilterAsync(IDictionary<string, object> properties)
        {
            return _proxy.SetDiscoveryFilterAsync(properties);
        }

        public Task StartDiscoveryAsync()
        {
            return _proxy.StartDiscoveryAsync();
        }

        public Task StopDiscoveryAsync()
        {
            return _proxy.StopDiscoveryAsync();
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return _proxy.WatchPropertiesAsync(handler);
        }

        private async void FireEventForExistingDevicesAsync()
        {
            var devices = await this.GetDevicesAsync();
            foreach (var device in devices)
            {
                _deviceFound?.Invoke(this, new DeviceFoundEventArgs(device, isStateChange: false));
            }
        }

        private async void OnDeviceAdded((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
        {
            if (BlueZManager.IsMatch(BluezConstants.DeviceInterface, args.objectPath, args.interfaces, this))
            {
                var device = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, args.objectPath);

                var dev = await Device.CreateAsync(device);
                _deviceFound?.Invoke(this, new DeviceFoundEventArgs(dev));
            }
        }

        private async void FireEventIfPropertyAlreadyTrueAsync(AdapterEventHandlerAsync handler, string prop)
        {
            try
            {
                var value = await _proxy.GetAsync<bool>(prop);
                if (value)
                {
                    // TODO: Suppress duplicate event from OnPropertyChanges.
                    handler?.Invoke(this, new BlueZEventArgs(isStateChange: false));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if '{prop}' is already true: {ex}");
            }
        }

        private void OnPropertyChanges(PropertyChanges changes)
        {
            foreach (var pair in changes.Changed)
            {
                switch (pair.Key)
                {
                    case "Powered":
                        if (true.Equals(pair.Value))
                        {
                            _poweredOn?.Invoke(this, new BlueZEventArgs());
                        }
                        else
                        {
                            PoweredOff?.Invoke(this, new BlueZEventArgs());
                        }
                        break;
                }
            }
        }

        private IAdapter1 _proxy;
        private IDisposable _interfacesWatcher;
        private IDisposable _propertyWatcher;
        private DeviceChangeEventHandlerAsync _deviceFound;
        private AdapterEventHandlerAsync _poweredOn;
    }
}
using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public delegate Task DeviceEventHandlerAsync(Device sender, BlueZEventArgs eventArgs);

    /// <summary>
    /// Adds events to IDevice1.
    /// </summary>
    public class Device : IDevice1, IDisposable
    {
        ~Device()
        {
            Dispose();
        }

        internal static async Task<Device> CreateAsync(IDevice1 proxy)
        {
            var device = new Device
            {
                m_proxy = proxy,
            };
            device.m_propertyWatcher = await proxy.WatchPropertiesAsync(device.OnPropertyChanges);

            return device;
        }

        public void Dispose()
        {
            m_propertyWatcher?.Dispose();
            m_propertyWatcher = null;

            GC.SuppressFinalize(this);
        }

        public event DeviceEventHandlerAsync Connected
        {
            add
            {
                m_connected += value;
                FireEventIfPropertyAlreadyTrueAsync(m_connected, "Connected");
            }
            remove
            {
                m_connected -= value;
            }
        }

        public event DeviceEventHandlerAsync Disconnected;
        public event DeviceEventHandlerAsync ServicesResolved
        {
            add
            {
                m_resolved += value;
                FireEventIfPropertyAlreadyTrueAsync(m_resolved, "ServicesResolved");
            }
            remove
            {
                m_resolved -= value;
            }
        }

        public ObjectPath ObjectPath => m_proxy.ObjectPath;

        public Task CancelPairingAsync()
        {
            return m_proxy.CancelPairingAsync();
        }

        public Task ConnectAsync()
        {
            return m_proxy.ConnectAsync();
        }

        public Task ConnectProfileAsync(string UUID)
        {
            return m_proxy.ConnectProfileAsync(UUID);
        }

        public Task DisconnectAsync()
        {
            return m_proxy.DisconnectAsync();
        }

        public Task DisconnectProfileAsync(string UUID)
        {
            return m_proxy.DisconnectProfileAsync(UUID);
        }

        public Task<Device1Properties> GetAllAsync()
        {
            return m_proxy.GetAllAsync();
        }

        public Task<T> GetAsync<T>(string prop)
        {
            return m_proxy.GetAsync<T>(prop);
        }

        public Task PairAsync()
        {
            return m_proxy.PairAsync();
        }

        public Task SetAsync(string prop, object val)
        {
            return m_proxy.SetAsync(prop, val);
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return m_proxy.WatchPropertiesAsync(handler);
        }

        private async void FireEventIfPropertyAlreadyTrueAsync(DeviceEventHandlerAsync handler, string prop)
        {
            try
            {
                var value = await m_proxy.GetAsync<bool>(prop);
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
                    case "Connected":
                        if (true.Equals(pair.Value))
                        {
                            m_connected?.Invoke(this, new BlueZEventArgs());
                        }
                        else
                        {
                            Disconnected?.Invoke(this, new BlueZEventArgs());
                        }
                        break;

                    case "ServicesResolved":
                        if (true.Equals(pair.Value))
                        {
                            m_resolved?.Invoke(this, new BlueZEventArgs());
                        }
                        break;
                }
            }
        }

        private IDevice1 m_proxy;
        private IDisposable m_propertyWatcher;
        private event DeviceEventHandlerAsync m_connected;
        private event DeviceEventHandlerAsync m_resolved;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThePBone.BlueZNet.Utils;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public static class Extensions
    {
        public static async Task<IReadOnlyList<Device>> GetDevicesAsync(this IAdapter1 adapter)
        {
            var devices = await BlueZManager.GetProxiesAsync<IDevice1>(BluezConstants.DeviceInterface, adapter);

            return await Task.WhenAll(devices.Select(Device.CreateAsync));
        }

        public static async Task<Device> GetDeviceAsync(this IAdapter1 adapter, string deviceAddress)
        {
            var devices = await BlueZManager.GetProxiesAsync<IDevice1>(BluezConstants.DeviceInterface, adapter);

            var matches = new List<IDevice1>();
            foreach (var device in devices)
            {
                if (String.Equals(await device.GetAddressAsync(), deviceAddress, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(device);
                }
            }

            if (matches.Count > 1)
            {
                throw new Exception($"{matches.Count} devices found with the address {deviceAddress}!");
            }

            var dev = matches.FirstOrDefault();
            if (dev != null)
            {
                return await Device.CreateAsync(dev);
            }
            return null;
        }

        public static Task<IDisposable> WatchDevicesAddedAsync(this IAdapter1 adapter, Action<Device> handler)
        {
            async void OnDeviceAdded((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
            {
                if (BlueZManager.IsMatch(BluezConstants.DeviceInterface, args.objectPath, args.interfaces, adapter))
                {
                    var device = Connection.System.CreateProxy<IDevice1>(BluezConstants.DbusService, args.objectPath);

                    var dev = await Device.CreateAsync(device);
                    handler(dev);
                }
            }

            var objectManager = Connection.System.CreateProxy<IObjectManager>(BluezConstants.DbusService, "/");
            return objectManager.WatchInterfacesAddedAsync(OnDeviceAdded);
        }
        
        public static IDisposable WatchForPropertyChangeAsync<T,T2>(this T2 obj, string propertyName, T currentValue, Action<T> callback)
        {
            IDisposable watcher = null;
            T previousValue = currentValue;

            void InnerCallback(PropertyChanges propertyChanges)
            {
                try
                {
                    if (propertyChanges.Changed.Any(kvp => kvp.Key == propertyName))
                    {
                        var pair = propertyChanges.Changed.Single(kvp => kvp.Key == propertyName);
                       // if (!EqualityComparer<T>.Default.Equals(previousValue,(T) pair.Value))
                        {
                            //previousValue = (T) pair.Value;
                            callback?.Invoke((T) pair.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                    watcher?.Dispose();
                }
            }

            var ret = obj.SimpleInvoke("WatchPropertiesAsync", (Action<PropertyChanges>) InnerCallback);
            if (ret is IDisposable disposable)
            {
                watcher = disposable;
            }
            else
            {
                throw new InvalidCastException("Return value is not IDisposable"); 
            }
            
            return watcher;
        }

        public static async Task WaitForPropertyValueAsync<T,T2>(this T2 obj, string propertyName, T value, TimeSpan timeout)
        {
            var (watchTask, watcher) = WatchPropertyValueAsync<T,T2>(obj, propertyName, value);

            T currentValue;
            var ret = obj.GenericInvoke<T>("GetAsync", propertyName);
            if (ret is Task<T> task)
            {
                currentValue = await task;
            }
            else
            {
                throw new InvalidCastException("Return value is not Task"); 
            }
            
            if (EqualityComparer<T>.Default.Equals(currentValue, value))
            {
                watcher.Dispose();
                return;
            }

            await Task.WhenAny(new Task[] { watchTask, Task.Delay(timeout) });
            if (!watchTask.IsCompleted)
            {
                throw new TimeoutException($"Timed out waiting for '{propertyName}' to change to '{value}'.");
            }
            
            await watchTask;
        }

        private static (Task, IDisposable) WatchPropertyValueAsync<T,T2>(T2 obj, string propertyName, T value)
        {
            var taskSource = new TaskCompletionSource<bool>();

            IDisposable watcher = null;
      
            Action<PropertyChanges> callback = propertyChanges => {
                try
                {
                    if (propertyChanges.Changed.Any(kvp => kvp.Key == propertyName))
                    {
                        var pair = propertyChanges.Changed.Single(kvp => kvp.Key == propertyName);
                        if (pair.Value.Equals(value))
                        {
                            taskSource.SetResult(true);
                            watcher?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                    taskSource.SetException(ex);
                    watcher?.Dispose();
                }
            };
            
            var ret = obj.SimpleInvoke("WatchPropertiesAsync", callback);
            if (ret is IDisposable disposable)
            {
                watcher = disposable;
            }
            else
            {
                throw new InvalidCastException("Return value is not IDisposable"); 
            }
            
            return (taskSource.Task, watcher);
        }
        
    }
}
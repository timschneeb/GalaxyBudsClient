using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
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
                Log.Error("{MatchesCount} devices found with the address {DeviceAddress}!", matches.Count, deviceAddress);
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

            watcher = AsSubscription(obj.SimpleInvoke("WatchPropertiesAsync", (Action<PropertyChanges>) InnerCallback));
            
            return watcher;
        }

        // WatchPropertiesAsync returns Task<IDisposable>, and Task is itself IDisposable, so a plain
        // `is IDisposable` check accepts the task and disposing it never removes the D-Bus match.
        private static IDisposable AsSubscription(object ret)
        {
            return ret switch
            {
                Task<IDisposable> pending => new PendingSubscription(pending),
                IDisposable disposable => disposable,
                _ => throw new InvalidCastException("Return value is not IDisposable")
            };
        }

        private sealed class PendingSubscription(Task<IDisposable> pending) : IDisposable
        {
            private int _disposed;

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                    return;
                pending.ContinueWith(t => t.Result.Dispose(), CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            }
        }

        public static async Task WaitForPropertyValueAsync<T,T2>(this T2 obj, string propertyName, T value, TimeSpan timeout, CancellationToken cancelToken)
        {
            var (watchTask, watcher) = WatchPropertyValueAsync<T,T2>(obj, propertyName, value);

            T currentValue;
            var ret = obj.GenericInvoke<T>("GetAsync", propertyName);
            if (ret is Task<T> task)
            {
                currentValue = await task.ConfigureAwait(false);
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

            await Task.WhenAny([watchTask, Task.Delay(timeout, cancelToken)]).ConfigureAwait(false);
            if (!watchTask.IsCompleted)
            {
                watcher.Dispose();
                throw new TimeoutException($"Timed out waiting for '{propertyName}' to change to '{value}'.");
            }
            
            await watchTask.ConfigureAwait(false);
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
                            // BlueZ can repeat the change before the watcher is gone; a second SetResult
                            // would throw out of the signal handler and starve the other watchers.
                            taskSource.TrySetResult(true);
                            watcher?.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                    taskSource.TrySetException(ex);
                    watcher?.Dispose();
                }
            };
            
            watcher = AsSubscription(obj.SimpleInvoke("WatchPropertiesAsync", callback));
            
            return (taskSource.Task, watcher);
        }
        
    }
}
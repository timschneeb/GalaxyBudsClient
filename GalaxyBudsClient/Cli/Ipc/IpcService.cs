using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GalaxyBudsClient.Cli.Ipc.Objects;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform;
using Serilog;
using Tmds.DBus;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Cli.Ipc
{
    public static class IpcService
    {
        public static string ServiceName => "me.timschneeberger.galaxybudsclient";
        private static string TcpAddress => "tcp:host=localhost,port=54533";
        private static DeviceObject? _deviceObject;
        
        public static async Task<Connection> OpenClientConnectionAsync()
        {
            var clientOptions = new ClientConnectionOptions(TcpAddress);
            var client = PlatformUtils.IsLinux ? new Connection(Address.Session) : new Connection(clientOptions);
            await client.ConnectAsync();
            return client;
        }
        
        private static async Task UpdateDeviceObjectAsync(this IConnection connection)
        {
            if (!BluetoothImpl.Instance.IsConnected)
                connection.UnregisterObject(DeviceObject.Path);
            else if (_deviceObject != null)
                await connection.RegisterObjectAsync(_deviceObject);
        }
        
        public static async Task Setup()
        {
            var server = new ServerConnectionOptions();
            // On Linux, we use the regular session bus. On other platforms, we host our own d-bus server.
            var useSessionBus = PlatformUtils.IsLinux;
            var connection = useSessionBus ? new Connection(Address.Session) : new Connection(server);
            
            try
            {
                // Linux: connect to existing bus
                if (useSessionBus)
                {
                    await connection.ConnectAsync();
                }
                
                await connection.RegisterServiceAsync(ServiceName, ServiceRegistrationOptions.None);
                await connection.RegisterObjectAsync(new ApplicationObject());
                _deviceObject = new DeviceObject();
                await connection.UpdateDeviceObjectAsync();

                BluetoothImpl.Instance.Connected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
                BluetoothImpl.Instance.Disconnected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
                
                if (!useSessionBus)
                {
                    var boundAddress = await server.StartAsync(TcpAddress);
                    Log.Information("IpcService: Server listening at {BoundAddress}", boundAddress);
                }
                else
                {
                    Log.Information("IpcService: Service listening on the session bus");
                    while (true)
                    {
                        await Task.Delay(int.MaxValue);
                    }
                }

                return;
            }
            catch (Exception e)
            {
                Log.Information("IpcService: Unable to register server: {Message}", e.Message);
            }
            
            // At this point, we failed to register the server. This means that another instance is already running.
            Connection client;
            try
            {
                client = await OpenClientConnectionAsync();
            }
            catch (Exception e)
            {
                Log.Warning("IpcService: Unable to connect to interface: {Message}", e.Message);
                Log.Warning($"IpcService: Continuing without any instance restrictions. " +
                            $"This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
                return;
            }

            Log.Information("IpcService: Client connected to active instance");
            try
            {
                var proxy = client.CreateProxy<IApplicationObject>(ServiceName, ApplicationObject.Path);
                await proxy.ActivateAsync();
                Log.Information("IpcService: Activation request to other instance sent. Shutting down now");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Log.Warning("IpcService: Unable to invoke activation method via proxy: {Message}", e.Message);
            }
        }
    }
}
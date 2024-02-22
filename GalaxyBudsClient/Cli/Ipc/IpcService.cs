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

        public static async Task<Connection> OpenClientConnection()
        {
            var clientOptions = new ClientConnectionOptions(TcpAddress);
            var client = PlatformUtils.IsLinux ? new Connection(Address.Session) : new Connection(clientOptions);
            await client.ConnectAsync();
            return client;
        }
        
        public static async Task Setup()
        {
            var server = new ServerConnectionOptions();
            // On Linux, we use the regular session bus. On other platforms, we host our own d-bus server.
            var useSessionBus = PlatformUtils.IsLinux;
            using var connection = useSessionBus ? new Connection(Address.Session) : new Connection(server);
            
            try
            {
                // Linux: connect to existing bus
                if (useSessionBus)
                {
                    await connection.ConnectAsync();
                }
                
                await connection.RegisterServiceAsync(ServiceName, ServiceRegistrationOptions.None);
                await connection.RegisterObjectAsync(new ApplicationObject());
                
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
                client = await OpenClientConnection();
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
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog;
using Tmds.DBus;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Utils
{
    public static class SingleInstanceWatcher
    {
        [DBusInterface("GalaxyBudsClient.Activator")]
        public interface IInstanceActivator : IDBusObject
        {
            Task ActivateAsync();
        }

        class InstanceActivator : IInstanceActivator
        {
            public Task ActivateAsync()
            {
                Activated?.Invoke();
                return Task.CompletedTask;
            }

            public static readonly ObjectPath Path = new ObjectPath("/activator");
            public ObjectPath ObjectPath => Path;
        }
        
        private static string TcpAddress => "tcp:host=localhost,port=54532";
        public static event Action? Activated;

        static SingleInstanceWatcher()
        {
            Activated += () => Log.Information($"SingleInstanceWatcher: Activation request received.");
        }
        
        public static async Task Setup()
        {
            var server = new ServerConnectionOptions();
            using var connection = new Connection(server);
            
            await connection.RegisterObjectAsync(new InstanceActivator());
            try
            {
                var boundAddress = await server.StartAsync(TcpAddress);
                Log.Information($"SingleInstanceWatcher: Server listening at {boundAddress}");
                return;
            }
            catch (Exception e)
            {
                Log.Information($"SingleInstanceWatcher: Unable to register server: {e.Message}. Attempting to connect to existing instance...");
            }

            var clientOptions = new ClientConnectionOptions(TcpAddress);
            using var client = new Connection(clientOptions);
            try
            {
                await client.ConnectAsync();
            }
            catch (Exception e)
            {
                Log.Warning($"SingleInstanceWatcher: Unable to connect to interface: {e.Message}");
                Log.Warning($"SingleInstanceWatcher: Continuing without any instance restrictions. This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
                return;
            }
                    
            Log.Information("SingleInstanceWatcher: Client connected to active instance");
            try
            {
                var proxy = client.CreateProxy<IInstanceActivator>("any.service", InstanceActivator.Path);
                await proxy.ActivateAsync();
                Log.Information(
                    "SingleInstanceWatcher: Activation request to other instance sent. Shutting down now.");
                Environment.Exit(0);
                return;
            }
            catch (Exception e)
            {
                Log.Warning($"SingleInstanceWatcher: Unable to invoke activation method via proxy: {e.Message}");
                Log.Warning($"SingleInstanceWatcher: Continuing without any instance restrictions. This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
                return;
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using GalaxyBudsClient.Cli.Ipc.Objects;
using GalaxyBudsClient.Platform;
using Serilog;
using Tmds.DBus;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Cli.Ipc;

public static class IpcService
{
    public static string ServiceName => "me.timschneeberger.GalaxyBudsClient";
    private static string TcpAddress => "tcp:host=localhost,port=54533";
    private static string MutexName => "Global\\GalaxyBudsClient_SingleInstance_Mutex";
    private static DeviceObject? _deviceObject;
    private static Mutex? _singleInstanceMutex;
        
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
        {
            connection.UnregisterObject(_deviceObject);
            await connection.RegisterObjectAsync(_deviceObject);
        }
    }
        
    /// <summary>
    /// Check if another instance is already running. If so, activate it and exit.
    /// This method should be called synchronously before starting the UI.
    /// Uses both IPC (for activation) and Mutex (as fallback for detection).
    /// </summary>
    /// <returns>True if this is the first instance and should continue, False if another instance exists (and this instance will exit)</returns>
    public static async Task<bool> CheckSingleInstanceAsync()
    {
        if(Design.IsDesignMode)
            return true;
        
        // First, try to acquire the mutex to ensure only one instance can proceed
        // This works even when IPC/TCP fails due to permissions
        bool createdNew;
        try
        {
            _singleInstanceMutex = new Mutex(true, MutexName, out createdNew);
        }
        catch (Exception e)
        {
            Log.Warning("IpcService: Unable to create mutex: {Message}", e.Message);
            createdNew = true; // Fall back to allowing this instance if mutex fails
        }
        
        if (!createdNew)
        {
            // Another instance is already running, try to activate it via IPC
            Log.Information("IpcService: Another instance detected via mutex");
            try
            {
                using var client = await OpenClientConnectionAsync();
                Log.Information("IpcService: Found existing instance, attempting to activate it");
                try
                {
                    var proxy = client.CreateProxy<IApplicationObject>(ServiceName, ApplicationObject.Path);
                    await proxy.ActivateAsync();
                    Log.Information("IpcService: Activation request to other instance sent. Shutting down now");
                }
                catch (Exception e)
                {
                    Log.Warning("IpcService: Unable to invoke activation method via proxy: {Message}", e.Message);
                }
            }
            catch (Exception e)
            {
                Log.Warning("IpcService: Unable to connect to existing instance for activation: {Message}", e.Message);
                Log.Warning("IpcService: Another instance is running but cannot be activated. Exiting anyway to prevent conflicts.");
            }
            
            // Exit regardless of whether activation succeeded
            // This prevents multiple instances even when IPC server failed to start
            Environment.Exit(0);
            return false; // This line won't be reached, but needed for compiler
        }
        
        // This is the first instance - mutex was successfully created
        Log.Debug("IpcService: No existing instance found (mutex acquired), continuing as first instance");
        return true;
    }
    
    /// <summary>
    /// Start the IPC server to allow future instances to detect and activate this one.
    /// This should be called asynchronously after the UI has started.
    /// </summary>
    public static async Task StartServerAsync()
    {
        if(Design.IsDesignMode)
            return;
        
        Connection? connection = null;
        try
        {
            var server = new ServerConnectionOptions();
            // On Linux, we use the regular session bus. On other platforms, we host our own d-bus server.
            var useSessionBus = PlatformUtils.IsLinux;
            // Don't use 'using' here - we need the connection to stay alive for the lifetime of the server
            connection = useSessionBus ? new Connection(Address.Session) : new Connection(server);

            
            string? boundAddress = null;
            // Linux: connect to existing bus
            if (useSessionBus)
            {
                await connection.ConnectAsync();
                await connection.RegisterServiceAsync(ServiceName, ServiceRegistrationOptions.None);
            }
                
            await connection.RegisterObjectAsync(new ApplicationObject());
            _deviceObject = new DeviceObject();
            await connection.UpdateDeviceObjectAsync();
            
            if(!useSessionBus)
            {
                boundAddress = await server.StartAsync(TcpAddress);
            }
            
            // Only register event handlers after server is successfully started
            // This prevents ObjectDisposedException if server fails and connection is disposed
            BluetoothImpl.Instance.Connected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
            BluetoothImpl.Instance.Disconnected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
            
            if (!useSessionBus)
            {
                Log.Information("IpcService: Server listening at {BoundAddress}", boundAddress);
            }
            else
            {
                Log.Information("IpcService: Service listening on the session bus");
            }
                
            while (true)
            {
                await Task.Delay(int.MaxValue);
            }
        }
        catch (Exception e)
        {
            Log.Warning("IpcService: Unable to register server: {Message}", e.Message);
            Log.Warning("IpcService: Other instances will not be able to detect this one. " +
                        "This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
            // Cleanup connection on failure
            connection?.Dispose();
        }
    }
    
    [Obsolete("Use CheckSingleInstanceAsync and StartServerAsync instead")]
    public static async Task Setup()
    {
        if(Design.IsDesignMode)
            return;
        
        try
        {
            var server = new ServerConnectionOptions();
            // On Linux, we use the regular session bus. On other platforms, we host our own d-bus server.
            var useSessionBus = PlatformUtils.IsLinux;
            using var connection = useSessionBus ? new Connection(Address.Session) : new Connection(server);

            
            string? boundAddress = null;
            // Linux: connect to existing bus
            if (useSessionBus)
            {
                await connection.ConnectAsync();
                await connection.RegisterServiceAsync(ServiceName, ServiceRegistrationOptions.None);
            }
                
            await connection.RegisterObjectAsync(new ApplicationObject());
            _deviceObject = new DeviceObject();
            await connection.UpdateDeviceObjectAsync();
            
            BluetoothImpl.Instance.Connected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
            BluetoothImpl.Instance.Disconnected += (sender, args) => _ = connection.UpdateDeviceObjectAsync();
            
            if(!useSessionBus)
            {
                boundAddress = await server.StartAsync(TcpAddress);
            }
            
            if (!useSessionBus)
            {
                Log.Information("IpcService: Server listening at {BoundAddress}", boundAddress);
            }
            else
            {
                Log.Information("IpcService: Service listening on the session bus");
            }
                
            while (true)
            {
                await Task.Delay(int.MaxValue);
            }
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
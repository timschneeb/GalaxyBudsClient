using System;
using System.IO;
using System.IO.Pipes;
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
    private static string PipeName => "GalaxyBudsClient_IPC_Pipe";
    // Use Local namespace instead of Global - Global requires admin privileges in some cases
    private static string MutexName => "Local\\GalaxyBudsClient_SingleInstance_Mutex";
    private static DeviceObject? _deviceObject;
    private static Mutex? _singleInstanceMutex;
    private static CancellationTokenSource? _pipeServerCancellation;
        
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
        bool isFirstInstance;
        try
        {
            // Create mutex with initial ownership
            // IMPORTANT: The mutex object must be kept alive for the entire application lifetime
            // We store it in a static field to prevent garbage collection
            _singleInstanceMutex = new Mutex(true, MutexName, out bool createdNew);
            
            Log.Debug("IpcService: Mutex check - createdNew={CreatedNew}", createdNew);
            
            if (!createdNew)
            {
                // Mutex already exists, try to acquire it with a timeout to confirm another instance is running
                // If we can acquire it immediately, the other instance may have terminated
                isFirstInstance = _singleInstanceMutex.WaitOne(TimeSpan.Zero);
                Log.Debug("IpcService: Mutex existed, WaitOne result={Result}", isFirstInstance);
                if (isFirstInstance)
                {
                    // We got the mutex, so the previous instance must have terminated
                    Log.Debug("IpcService: Mutex existed but was released, acquiring ownership");
                }
            }
            else
            {
                // We created the mutex, we're the first instance
                isFirstInstance = true;
                Log.Debug("IpcService: Created new mutex, we are the first instance");
            }
            
            // Keep the mutex alive to prevent garbage collection
            GC.KeepAlive(_singleInstanceMutex);
        }
        catch (Exception e)
        {
            Log.Warning("IpcService: Unable to create mutex: {Message}", e.Message);
            isFirstInstance = true; // Fall back to allowing this instance if mutex fails
        }
        
        if (!isFirstInstance)
        {
            // Another instance is already running, try to activate it via IPC
            Log.Information("IpcService: Another instance detected via mutex");
            
            bool activated = false;
            
            // Try D-Bus/TCP first
            try
            {
                using var client = await OpenClientConnectionAsync();
                Log.Information("IpcService: Found existing instance via D-Bus/TCP, attempting to activate it");
                try
                {
                    var proxy = client.CreateProxy<IApplicationObject>(ServiceName, ApplicationObject.Path);
                    await proxy.ActivateAsync();
                    Log.Information("IpcService: Activation request sent via D-Bus/TCP. Shutting down now");
                    activated = true;
                }
                catch (Exception e)
                {
                    Log.Warning("IpcService: Unable to invoke activation method via D-Bus proxy: {Message}", e.Message);
                }
            }
            catch (Exception e)
            {
                Log.Debug("IpcService: Unable to connect via D-Bus/TCP: {Message}", e.Message);
            }
            
            // If D-Bus/TCP failed and we're on Windows, try Named Pipe as fallback
            if (!activated && !PlatformUtils.IsLinux)
            {
                try
                {
                    await ActivateViaNamedPipeAsync();
                    Log.Information("IpcService: Activation request sent via Named Pipe. Shutting down now");
                    activated = true;
                }
                catch (Exception e)
                {
                    Log.Warning("IpcService: Unable to activate via Named Pipe: {Message}", e.Message);
                }
            }
            
            if (!activated)
            {
                Log.Warning("IpcService: Another instance is running but cannot be activated. Exiting anyway to prevent conflicts.");
            }
            
            // Dispose the mutex - we don't own it, so don't call ReleaseMutex()
            _singleInstanceMutex?.Dispose();
            
            // Exit regardless of whether activation succeeded
            // This prevents multiple instances even when IPC server failed to start
            Environment.Exit(0);
            return false; // This line won't be reached, but needed for compiler
        }
        
        // This is the first instance - mutex was successfully created and owned
        // Keep the mutex alive for the lifetime of the application
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
            Log.Warning("IpcService: Unable to register D-Bus/TCP server: {Message}", e.Message);
            // Cleanup connection on failure
            connection?.Dispose();
            
            // On Windows, start Named Pipe server as fallback (doesn't require network permissions)
            if (!PlatformUtils.IsLinux)
            {
                Log.Information("IpcService: Starting Named Pipe server as fallback");
                try
                {
                    await StartNamedPipeServerAsync();
                }
                catch (Exception ex)
                {
                    Log.Warning("IpcService: Unable to start Named Pipe server: {Message}", ex.Message);
                    Log.Warning("IpcService: Other instances will not be able to detect this one. " +
                                "This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
                }
            }
            else
            {
                Log.Warning("IpcService: Other instances will not be able to detect this one. " +
                            "This can cause Bluetooth issues since only one app can interact with the earbuds at a time.");
            }
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
    
    /// <summary>
    /// Start a Named Pipe server as fallback IPC mechanism (Windows only).
    /// This doesn't require network permissions and works when TCP is blocked.
    /// </summary>
    private static async Task StartNamedPipeServerAsync()
    {
        _pipeServerCancellation = new CancellationTokenSource();
        var token = _pipeServerCancellation.Token;
        
        Log.Information("IpcService: Named Pipe server starting on pipe: {PipeName}", PipeName);
        
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
                
                // Wait for a client to connect
                await server.WaitForConnectionAsync(token);
                
                Log.Debug("IpcService: Named Pipe client connected");
                
                // Read the command from the client
                using var reader = new StreamReader(server);
                var command = await reader.ReadLineAsync();
                
                Log.Debug("IpcService: Received command via Named Pipe: {Command}", command);
                
                // Execute the command
                if (command == "ACTIVATE")
                {
                    Interface.MainWindow.Instance.BringToFront();
                    Log.Information("IpcService: Window activated via Named Pipe request");
                }
                
                // Send acknowledgment
                using var writer = new StreamWriter(server) { AutoFlush = true };
                await writer.WriteLineAsync("OK");
            }
            catch (OperationCanceledException)
            {
                // Server is shutting down
                break;
            }
            catch (Exception e)
            {
                if (!token.IsCancellationRequested)
                {
                    Log.Warning("IpcService: Error in Named Pipe server: {Message}", e.Message);
                    // Wait a bit before retrying
                    await Task.Delay(1000, token);
                }
            }
        }
        
        Log.Information("IpcService: Named Pipe server stopped");
    }
    
    /// <summary>
    /// Activate an existing instance via Named Pipe (Windows only).
    /// </summary>
    private static async Task ActivateViaNamedPipeAsync()
    {
        using var client = new NamedPipeClientStream(
            ".",
            PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);
        
        // Try to connect with a timeout
        var cts = new CancellationTokenSource(2000); // 2 second timeout
        await client.ConnectAsync(cts.Token);
        
        // Send activation command
        using var writer = new StreamWriter(client) { AutoFlush = true };
        await writer.WriteLineAsync("ACTIVATE");
        
        // Wait for acknowledgment
        using var reader = new StreamReader(client);
        var response = await reader.ReadLineAsync();
        
        if (response != "OK")
        {
            throw new Exception($"Unexpected response from Named Pipe server: {response}");
        }
    }
}
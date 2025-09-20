using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform.Model;
using Serilog;

namespace GalaxyBudsClient.Utils.PendingCommands
{
    /// <summary>
    /// Manages commands that should be sent when device reconnects
    /// </summary>
    public class PendingCommandsManager
    {
        private static PendingCommandsManager? _instance;
        private static readonly object _lock = new object();
        
        public static PendingCommandsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new PendingCommandsManager();
                    }
                }
                return _instance;
            }
        }

        private readonly ConcurrentQueue<PendingCommand> _pendingCommands = new();
        private bool _isProcessing = false;
        private readonly object _processingLock = new object();

        private PendingCommandsManager()
        {
            // Subscribe to connection events
            BluetoothImpl.Instance.Connected += OnDeviceConnected;
            BluetoothImpl.Instance.Disconnected += OnDeviceDisconnected;
            BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
        }

        /// <summary>
        /// Adds a command to be sent when device reconnects, or sends immediately if connected
        /// </summary>
        public async Task<bool> SendCommandAsync(MsgIds msgId, object? payload = null, string? description = null)
        {
            if (BluetoothImpl.Instance.IsConnected)
            {
                // Device is connected, send immediately
                try
                {
                    // Handle different payload types
                    if (payload == null)
                    {
                        await BluetoothImpl.Instance.SendRequestAsync(msgId);
                    }
                    else if (payload is bool boolPayload)
                    {
                        await BluetoothImpl.Instance.SendRequestAsync(msgId, boolPayload);
                    }
                    else if (payload is byte[] byteArrayPayload)
                    {
                        await BluetoothImpl.Instance.SendRequestAsync(msgId, byteArrayPayload);
                    }
                    else
                    {
                        Log.Warning("Unsupported payload type for immediate command {MsgId}: {PayloadType}", 
                            msgId, payload.GetType().Name);
                        return false;
                    }
                    
                    Log.Information("✅ Command sent immediately: {MsgId} - {Description}", msgId, description ?? "No description");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "❌ Failed to send command immediately: {MsgId} - {Description}", msgId, description ?? "No description");
                    return false;
                }
            }
            else
            {
                // Device is disconnected, queue command
                var pendingCommand = new PendingCommand
                {
                    MsgId = msgId,
                    Payload = payload,
                    Description = description ?? msgId.ToString(),
                    QueuedAt = DateTime.Now
                };

                _pendingCommands.Enqueue(pendingCommand);
                Log.Warning("📋 Command queued for later: {MsgId} - {Description} (Total queued: {Count})", 
                    msgId, pendingCommand.Description, _pendingCommands.Count);
                return false;
            }
        }

        private async void OnDeviceConnected(object? sender, EventArgs e)
        {
            Log.Information("🔗 Device connected - processing pending commands...");
            await ProcessPendingCommandsAsync();
        }

        private void OnDeviceDisconnected(object? sender, string e)
        {
            Log.Information("🔌 Device disconnected - commands will be queued");
        }

        private void OnBluetoothError(object? sender, BluetoothException e)
        {
            Log.Information("⚠️ Bluetooth error - commands will be queued");
        }

        private async Task ProcessPendingCommandsAsync()
        {
            lock (_processingLock)
            {
                if (_isProcessing)
                {
                    Log.Debug("Already processing pending commands, skipping...");
                    return;
                }
                _isProcessing = true;
            }

            try
            {
                if (_pendingCommands.IsEmpty)
                {
                    Log.Debug("No pending commands to process");
                    return;
                }

                Log.Information("📤 Processing {Count} pending commands...", _pendingCommands.Count);
                int successCount = 0;
                int failCount = 0;

                // Wait a bit for connection to stabilize
                await Task.Delay(2000);

                while (_pendingCommands.TryDequeue(out var command))
                {
                    if (!BluetoothImpl.Instance.IsConnected)
                    {
                        // Device disconnected while processing, re-queue remaining commands
                        _pendingCommands.Enqueue(command);
                        Log.Warning("⚠️ Device disconnected while processing commands - re-queued remaining commands");
                        break;
                    }

                    try
                    {
                        Log.Information("📤 Sending pending command: {MsgId} - {Description} (queued at {QueuedAt})", 
                            command.MsgId, command.Description, command.QueuedAt.ToString("HH:mm:ss"));

                        // Handle different payload types
                        if (command.Payload == null)
                        {
                            await BluetoothImpl.Instance.SendRequestAsync(command.MsgId);
                        }
                        else if (command.Payload is bool boolPayload)
                        {
                            await BluetoothImpl.Instance.SendRequestAsync(command.MsgId, boolPayload);
                        }
                        else if (command.Payload is byte[] byteArrayPayload)
                        {
                            await BluetoothImpl.Instance.SendRequestAsync(command.MsgId, byteArrayPayload);
                        }
                        else
                        {
                            // Convert other types to byte array if possible
                            Log.Warning("Unsupported payload type for command {MsgId}: {PayloadType}", 
                                command.MsgId, command.Payload.GetType().Name);
                            continue;
                        }
                        successCount++;

                        Log.Information("✅ Pending command sent successfully: {MsgId} - {Description}", 
                            command.MsgId, command.Description);

                        // Small delay between commands to avoid overwhelming the device
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Log.Error(ex, "❌ Failed to send pending command: {MsgId} - {Description}", 
                            command.MsgId, command.Description);
                    }
                }

                Log.Information("📋 Pending commands processing completed: {Success} successful, {Failed} failed", 
                    successCount, failCount);
            }
            finally
            {
                lock (_processingLock)
                {
                    _isProcessing = false;
                }
            }
        }

        /// <summary>
        /// Gets the number of pending commands in queue
        /// </summary>
        public int PendingCommandsCount => _pendingCommands.Count;

        /// <summary>
        /// Clears all pending commands
        /// </summary>
        public void ClearPendingCommands()
        {
            while (_pendingCommands.TryDequeue(out _)) { }
            Log.Information("🗑️ All pending commands cleared");
        }
    }

    internal class PendingCommand
    {
        public MsgIds MsgId { get; set; }
        public object? Payload { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime QueuedAt { get; set; }
    }
}

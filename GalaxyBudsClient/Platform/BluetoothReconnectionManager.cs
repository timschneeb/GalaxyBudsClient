using System;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform.Model;
using Serilog;

namespace GalaxyBudsClient.Platform;

public sealed class BluetoothReconnectionManager : IDisposable
{
    private static readonly object Padlock = new();
    private static BluetoothReconnectionManager? _instance;
    public static BluetoothReconnectionManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new BluetoothReconnectionManager();
            }
        }
    }

    private readonly int[] _retryDelaysMs = [2000, 5000, 10000, 30000, 60000]; // Exponential backoff
    private const int SilentFailureTimeoutMs = 10000; // 10 seconds
    private const int MaxRetryAttempts = 5;

    private int _currentRetryAttempt;
    private CancellationTokenSource? _reconnectCancelSource;
    private CancellationTokenSource? _timeoutCancelSource;
    private bool _isManualDisconnect;
    private bool _isReconnecting;
    private BluetoothImpl? _bluetoothImpl;

    private BluetoothReconnectionManager()
    {
        // Don't subscribe to events here to avoid circular dependency
        // Initialize will be called by BluetoothImpl after construction
    }
    
    public void Initialize(BluetoothImpl bluetoothImpl)
    {
        if (_bluetoothImpl != null)
        {
            Log.Debug("BluetoothReconnectionManager: Already initialized, skipping");
            return; // Already initialized
        }
            
        _bluetoothImpl = bluetoothImpl;
        // Still subscribe to events for when they are invoked
        _bluetoothImpl.Disconnected += OnDisconnected;
        _bluetoothImpl.BluetoothError += OnBluetoothError;
        _bluetoothImpl.Connected += OnConnected;
        
        Log.Information("BluetoothReconnectionManager: Initialized successfully, auto-reconnect enabled: {Enabled}", 
            Settings.Data.AutoReconnectEnabled);
    }
    
    // Public methods called directly from BluetoothImpl (bypass event system)
    public void OnDisconnectionOccurred(string reason)
    {
        OnDisconnected(null, reason);
    }
    
    public void OnBluetoothErrorOccurred(BluetoothException exception)
    {
        OnBluetoothError(null, exception);
    }

    public void Dispose()
    {
        StopReconnection();
        if (_bluetoothImpl != null)
        {
            _bluetoothImpl.Disconnected -= OnDisconnected;
            _bluetoothImpl.BluetoothError -= OnBluetoothError;
            _bluetoothImpl.Connected -= OnConnected;
        }
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        // Reset retry counter on successful connection
        _currentRetryAttempt = 0;
        _isReconnecting = false;
        
        // Cancel timeout detection if running
        _timeoutCancelSource?.Cancel();
        _timeoutCancelSource?.Dispose();
        _timeoutCancelSource = null;
        
        Log.Debug("BluetoothReconnectionManager: Connection established, reset retry counter");
    }

    private void OnDisconnected(object? sender, string reason)
    {
        Log.Debug("BluetoothReconnectionManager.OnDisconnected called: AutoReconnectEnabled={Enabled}, Reason={Reason}, IsManualDisconnect={IsManual}", 
            Settings.Data.AutoReconnectEnabled, reason, _isManualDisconnect);
        
        if (!Settings.Data.AutoReconnectEnabled)
        {
            Log.Debug("BluetoothReconnectionManager: Auto-reconnect disabled, ignoring disconnection");
            return;
        }

        // Check if this is a user-requested disconnect
        if (reason.Contains("User requested disconnect", StringComparison.OrdinalIgnoreCase))
        {
            _isManualDisconnect = true;
            _currentRetryAttempt = 0;
            StopReconnection();
            Log.Information("BluetoothReconnectionManager: Manual disconnect detected, auto-reconnect disabled");
            return;
        }

        if (_isManualDisconnect)
        {
            Log.Debug("BluetoothReconnectionManager: Skipping auto-reconnect after manual disconnect");
            return;
        }

        Log.Information("BluetoothReconnectionManager: Disconnection detected: {Reason}", reason);
        StartReconnection();
    }

    private void OnBluetoothError(object? sender, BluetoothException exception)
    {
        Log.Debug("BluetoothReconnectionManager.OnBluetoothError called: AutoReconnectEnabled={Enabled}, ErrorCode={Code}, Message={Message}, IsManual={IsManual}", 
            Settings.Data.AutoReconnectEnabled, exception.ErrorCode, exception.ErrorMessage ?? exception.Message, _isManualDisconnect);
        
        if (!Settings.Data.AutoReconnectEnabled)
        {
            Log.Debug("BluetoothReconnectionManager: Auto-reconnect disabled, ignoring error");
            return;
        }

        if (_isManualDisconnect)
        {
            Log.Debug("BluetoothReconnectionManager: Skipping auto-reconnect after manual disconnect");
            return;
        }

        Log.Information("BluetoothReconnectionManager: Bluetooth error detected: {ErrorCode} - {Message}", 
            exception.ErrorCode, exception.ErrorMessage ?? exception.Message);
        StartReconnection();
    }

    private void StartReconnection()
    {
        if (_isReconnecting)
        {
            Log.Debug("BluetoothReconnectionManager: Reconnection already in progress");
            return;
        }

        _isReconnecting = true;
        
        // Cancel any existing reconnection task
        StopReconnection();
        
        _reconnectCancelSource = new CancellationTokenSource();
        _ = Task.Run(() => ReconnectionLoop(_reconnectCancelSource.Token), _reconnectCancelSource.Token);
    }

    private void StopReconnection()
    {
        _reconnectCancelSource?.Cancel();
        _reconnectCancelSource?.Dispose();
        _reconnectCancelSource = null;
        
        _timeoutCancelSource?.Cancel();
        _timeoutCancelSource?.Dispose();
        _timeoutCancelSource = null;
    }

    private async Task ReconnectionLoop(CancellationToken cancellationToken)
    {
        while (_currentRetryAttempt < MaxRetryAttempts && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                var delayMs = _currentRetryAttempt < _retryDelaysMs.Length 
                    ? _retryDelaysMs[_currentRetryAttempt] 
                    : _retryDelaysMs[^1];
                
                Log.Information("BluetoothReconnectionManager: Attempt {Attempt}/{Max} - waiting {Delay}ms before reconnect", 
                    _currentRetryAttempt + 1, MaxRetryAttempts, delayMs);
                
                await Task.Delay(delayMs, cancellationToken);
                
                if (cancellationToken.IsCancellationRequested)
                    break;

                Log.Information("BluetoothReconnectionManager: Attempting to reconnect...");
                
                if (_bluetoothImpl == null)
                {
                    Log.Error("BluetoothReconnectionManager: BluetoothImpl not initialized");
                    break;
                }
                
                // Start timeout detection for silent failures
                _timeoutCancelSource?.Dispose();
                _timeoutCancelSource = new CancellationTokenSource();
                var timeoutTask = Task.Delay(SilentFailureTimeoutMs, _timeoutCancelSource.Token);
                
                var connectTask = _bluetoothImpl.ConnectAsync();
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask && !_timeoutCancelSource.Token.IsCancellationRequested)
                {
                    // Timeout - connection appears stuck
                    Log.Warning("BluetoothReconnectionManager: Connection attempt timed out (silent failure detection)");
                    _currentRetryAttempt++;
                    
                    // Try to disconnect to clean up
                    try
                    {
                        await _bluetoothImpl.DisconnectAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "BluetoothReconnectionManager: Error during cleanup disconnect");
                    }
                    
                    continue;
                }
                
                // Cancel and dispose timeout token after connect completes
                _timeoutCancelSource?.Cancel();
                _timeoutCancelSource?.Dispose();
                _timeoutCancelSource = null;
                
                var connectResult = await connectTask;
                
                if (connectResult)
                {
                    Log.Information("BluetoothReconnectionManager: Reconnection successful");
                    _currentRetryAttempt = 0;
                    _isReconnecting = false;
                    return;
                }
                else
                {
                    Log.Warning("BluetoothReconnectionManager: Reconnection attempt {Attempt} failed", 
                        _currentRetryAttempt + 1);
                    _currentRetryAttempt++;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Debug("BluetoothReconnectionManager: Reconnection cancelled");
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BluetoothReconnectionManager: Error during reconnection attempt {Attempt}", 
                    _currentRetryAttempt + 1);
                _currentRetryAttempt++;
            }
        }

        if (_currentRetryAttempt >= MaxRetryAttempts)
        {
            Log.Warning("BluetoothReconnectionManager: Maximum retry attempts ({Max}) reached, giving up", 
                MaxRetryAttempts);
        }
        
        _isReconnecting = false;
        _currentRetryAttempt = 0;
    }

    public void ResetManualDisconnectFlag()
    {
        Log.Debug("BluetoothReconnectionManager: Manual disconnect flag reset - auto-reconnect re-enabled");
        _isManualDisconnect = false;
        _currentRetryAttempt = 0;
    }
}

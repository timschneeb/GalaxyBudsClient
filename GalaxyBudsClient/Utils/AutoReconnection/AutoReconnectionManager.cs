using System;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Platform.Model;
using Serilog;

namespace GalaxyBudsClient.Utils.AutoReconnection;

/// <summary>
/// Gerenciador de reconexão automática para Galaxy Buds
/// Monitora desconexões e tenta reconectar automaticamente com estratégia inteligente
/// </summary>
public class AutoReconnectionManager : IDisposable
{
    private static AutoReconnectionManager? _instance;
    public static AutoReconnectionManager Instance => _instance ??= new AutoReconnectionManager();
    
    private Timer? _reconnectionTimer;
    private bool _disposed;
    private bool _isReconnecting;
    private bool _wasManualDisconnection;
    private int _reconnectionAttempts;
    
    // Configurações de reconexão
    private readonly int[] _reconnectionIntervals = { 2, 5, 10, 30, 60 }; // segundos
    private const int MaxReconnectionAttempts = 5;
    
    private AutoReconnectionManager()
    {
        // Escutar eventos de conexão/desconexão
        BluetoothImpl.Instance.Connected += OnConnected;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
        BluetoothImpl.Instance.BluetoothError += OnBluetoothError;
        
        Log.Information("AutoReconnectionManager initialized");
    }
    
    /// <summary>
    /// Inicia o sistema de reconexão automática
    /// </summary>
    public void Start()
    {
        if (!Settings.Data.AutoReconnectionEnabled)
        {
            Log.Debug("AutoReconnectionManager: Feature is disabled");
            return;
        }
        
        Log.Information("AutoReconnectionManager started");
    }
    
    /// <summary>
    /// Para o sistema de reconexão automática
    /// </summary>
    public void Stop()
    {
        _reconnectionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _isReconnecting = false;
        _reconnectionAttempts = 0;
        
        Log.Information("AutoReconnectionManager stopped");
    }
    
    /// <summary>
    /// Marca que a desconexão foi manual (usuário clicou em desconectar)
    /// </summary>
    public void MarkManualDisconnection()
    {
        _wasManualDisconnection = true;
        Stop(); // Para tentativas de reconexão se houver
        Log.Debug("AutoReconnectionManager: Manual disconnection marked");
    }
    
    private void OnConnected(object? sender, EventArgs e)
    {
        // Conexão bem-sucedida - resetar estado
        _isReconnecting = false;
        _reconnectionAttempts = 0;
        _wasManualDisconnection = false;
        
        // Parar timer de reconexão se estiver rodando
        _reconnectionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        
        Log.Information("AutoReconnectionManager: Device connected successfully");
    }
    
    private void OnDisconnected(object? sender, string reason)
    {
        Log.Information("🔍 AutoReconnectionManager: OnDisconnected called - Reason: {Reason}", reason);
        HandleDisconnection(reason);
    }
    
    private void OnBluetoothError(object? sender, BluetoothException exception)
    {
        Log.Information("🔍 AutoReconnectionManager: OnBluetoothError called - Error: {Error}", exception.Message);
        
        // Se já estamos tentando reconectar, não tratar como nova desconexão
        if (_isReconnecting)
        {
            Log.Debug("🔍 AutoReconnectionManager: Ignoring BluetoothError during reconnection attempts");
            return;
        }
        
        HandleDisconnection($"Bluetooth error: {exception.Message}");
    }
    
    private void HandleDisconnection(string reason)
    {
        Log.Information("🔍 AutoReconnectionManager: HandleDisconnection called - Reason: {Reason}", reason);
        Log.Information("🔍 AutoReconnectionManager: Current state - Enabled: {Enabled}, Manual: {Manual}, Reconnecting: {Reconnecting}", 
            Settings.Data.AutoReconnectionEnabled, _wasManualDisconnection, _isReconnecting);
        
        // Não tentar reconectar se:
        // 1. Funcionalidade está desabilitada
        // 2. Foi desconexão manual
        // 3. Já está tentando reconectar
        if (!Settings.Data.AutoReconnectionEnabled || 
            _wasManualDisconnection || 
            _isReconnecting)
        {
            Log.Warning("⚠️ AutoReconnectionManager: Skipping reconnection - Enabled: {Enabled}, Manual: {Manual}, Reconnecting: {Reconnecting}", 
                Settings.Data.AutoReconnectionEnabled, _wasManualDisconnection, _isReconnecting);
            return;
        }
        
        Log.Information("🚀 AutoReconnectionManager: Device disconnected ({Reason}) - starting reconnection attempts", reason);
        StartReconnectionAttempts();
    }
    
    private void StartReconnectionAttempts()
    {
        Log.Information("🔄 AutoReconnectionManager: StartReconnectionAttempts called");
        _isReconnecting = true;
        _reconnectionAttempts = 0;
        
        Log.Information("⏰ AutoReconnectionManager: Scheduling immediate reconnection attempt");
        // Primeira tentativa imediata
        ScheduleReconnectionAttempt(0);
    }
    
    private void ScheduleReconnectionAttempt(int delaySeconds)
    {
        if (_disposed || !Settings.Data.AutoReconnectionEnabled)
            return;
            
        _reconnectionTimer?.Dispose();
        _reconnectionTimer = new Timer(async _ => await AttemptReconnection(), 
            null, TimeSpan.FromSeconds(delaySeconds), Timeout.InfiniteTimeSpan);
        
        if (delaySeconds > 0)
        {
            Log.Information("AutoReconnectionManager: Next reconnection attempt in {Delay}s (attempt {Current}/{Max})", 
                delaySeconds, _reconnectionAttempts + 1, MaxReconnectionAttempts);
        }
    }
    
    private async Task AttemptReconnection()
    {
        Log.Information("🔧 AutoReconnectionManager: AttemptReconnection called");
        
        if (_disposed || !Settings.Data.AutoReconnectionEnabled || !_isReconnecting)
        {
            Log.Warning("⚠️ AutoReconnectionManager: Aborting reconnection - Disposed: {Disposed}, Enabled: {Enabled}, Reconnecting: {Reconnecting}",
                _disposed, Settings.Data.AutoReconnectionEnabled, _isReconnecting);
            return;
        }
            
        _reconnectionAttempts++;
        
        try
        {
            Log.Information("🔄 AutoReconnectionManager: Attempting reconnection #{Attempt}/{Max}", 
                _reconnectionAttempts, MaxReconnectionAttempts);
            
            // Verificar se ainda há um dispositivo válido configurado
            if (!BluetoothImpl.HasValidDevice)
            {
                Log.Warning("⚠️ AutoReconnectionManager: No valid device configured - stopping reconnection attempts");
                Stop();
                return;
            }
            
            Log.Information("📡 AutoReconnectionManager: Calling BluetoothImpl.Instance.ConnectAsync()");
            // Tentar conectar
            var result = await BluetoothImpl.Instance.ConnectAsync();
            
            Log.Information("✅ AutoReconnectionManager: ConnectAsync returned: {Result}", result);
            
            if (result)
            {
                // Conexão iniciada com sucesso (sucesso será confirmado no evento Connected)
                Log.Information("🚀 AutoReconnectionManager: Reconnection attempt #{Attempt} initiated successfully", _reconnectionAttempts);
                
                // Aguardar 10 segundos para confirmar se a conexão foi estabelecida
                _ = Task.Delay(10000).ContinueWith(async _ =>
                {
                    if (_isReconnecting && !BluetoothImpl.Instance.IsConnected)
                    {
                        Log.Warning("⚠️ AutoReconnectionManager: Connection timeout - device not connected after 10s");
                        
                        // Tentar próxima tentativa se ainda há tentativas disponíveis
                        if (_reconnectionAttempts < MaxReconnectionAttempts)
                        {
                            var intervalIndex = Math.Min(_reconnectionAttempts - 1, _reconnectionIntervals.Length - 1);
                            var nextDelay = _reconnectionIntervals[intervalIndex];
                            
                            Log.Information("⏳ AutoReconnectionManager: Scheduling next attempt in {Delay}s due to timeout", nextDelay);
                            ScheduleReconnectionAttempt(nextDelay);
                        }
                        else
                        {
                            Log.Warning("🛑 AutoReconnectionManager: Maximum reconnection attempts ({Max}) reached after timeout - giving up", 
                                MaxReconnectionAttempts);
                            Stop();
                        }
                    }
                });
            }
            else
            {
                // Falha na conexão - tentar novamente se ainda há tentativas
                Log.Warning("❌ AutoReconnectionManager: Reconnection attempt #{Attempt} failed", _reconnectionAttempts);
                
                if (_reconnectionAttempts < MaxReconnectionAttempts)
                {
                    // Calcular próximo intervalo (com backoff exponencial limitado)
                    var intervalIndex = Math.Min(_reconnectionAttempts - 1, _reconnectionIntervals.Length - 1);
                    var nextDelay = _reconnectionIntervals[intervalIndex];
                    
                    Log.Information("⏳ AutoReconnectionManager: Scheduling next attempt in {Delay}s", nextDelay);
                    ScheduleReconnectionAttempt(nextDelay);
                }
                else
                {
                    Log.Warning("🛑 AutoReconnectionManager: Maximum reconnection attempts ({Max}) reached - giving up", 
                        MaxReconnectionAttempts);
                    Stop();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ AutoReconnectionManager: Reconnection attempt #{Attempt} failed: {Error}", 
                _reconnectionAttempts, ex.Message);
            
            // Verificar se deve tentar novamente
            if (_reconnectionAttempts < MaxReconnectionAttempts)
            {
                // Calcular próximo intervalo (com backoff exponencial limitado)
                var intervalIndex = Math.Min(_reconnectionAttempts - 1, _reconnectionIntervals.Length - 1);
                var nextDelay = _reconnectionIntervals[intervalIndex];
                
                Log.Information("⏳ AutoReconnectionManager: Scheduling next attempt in {Delay}s", nextDelay);
                ScheduleReconnectionAttempt(nextDelay);
            }
            else
            {
                Log.Warning("🛑 AutoReconnectionManager: Maximum reconnection attempts ({Max}) reached - giving up", 
                    MaxReconnectionAttempts);
                Stop();
            }
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
            
        _disposed = true;
        
        // Remover event handlers
        BluetoothImpl.Instance.Connected -= OnConnected;
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
        
        // Limpar timer
        _reconnectionTimer?.Dispose();
        _reconnectionTimer = null;
        
        Log.Information("AutoReconnectionManager disposed");
    }
}

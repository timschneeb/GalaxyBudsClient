using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Utils.TeamsIntegration;

/// <summary>
/// Monitor simples que detecta chamadas do Microsoft Teams e controla a conexão direta
/// </summary>
public class TeamsCallMonitor : IDisposable
{
    private static TeamsCallMonitor? _instance;
    public static TeamsCallMonitor? Instance => _instance;
    
    private Timer? _monitorTimer;
    private bool _isRunning;
    private bool _disposed;
    private bool? _lastCallState; // null = desconhecido, true = em chamada, false = não em chamada
    
    // Configurações do monitor
    private const int CheckIntervalSeconds = 5;
    private readonly string TeamsLogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"Packages\MSTeams_8wekyb3d8bbwe\LocalCache\Microsoft\MSTeams\Logs");
    private const string TeamsLogFilePattern = "MSTeams_*.log";
    
    private TeamsCallMonitor()
    {
        _monitorTimer = new Timer(CheckTeamsStatus, null, Timeout.Infinite, Timeout.Infinite);
        Log.Information("Teams integration monitor initialized");
    }
    
    public static TeamsCallMonitor Initialize()
    {
        if (_instance == null)
        {
            _instance = new TeamsCallMonitor();
        }
        return _instance;
    }
    
    public void Start()
    {
        if (_isRunning)
            return;
            
        _isRunning = true;
        _monitorTimer?.Change(TimeSpan.FromSeconds(CheckIntervalSeconds), TimeSpan.FromSeconds(CheckIntervalSeconds));
        Log.Information("Teams integration monitor started - checking every {Interval}s", CheckIntervalSeconds);
    }
    
    public void Stop()
    {
        if (_disposed || !_isRunning)
            return;
            
        _isRunning = false;
        _monitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        Log.Information("Teams integration monitor stopped");
    }
    
    private async void CheckTeamsStatus(object? state)
    {
        if (_disposed || !_isRunning)
            return;
            
        try
        {
            var isCurrentlyInCall = await GetTeamsCallStatusAsync();
            
            // Verificar se o status mudou para evitar comandos redundantes
            if (isCurrentlyInCall != _lastCallState)
            {
                Log.Debug("Status da chamada mudou: {Previous} -> {Current}", _lastCallState?.ToString() ?? "desconhecido", isCurrentlyInCall);
                
                _lastCallState = isCurrentlyInCall;
                
                if (isCurrentlyInCall)
                {
                    Log.Information("🔴 Chamada do Teams INICIADA - desativando conexão direta");
                    await SendSeamlessConnectionCommand(false); // Desativar
                }
                else
                {
                    Log.Information("🟢 Chamada do Teams FINALIZADA - ativando conexão direta");
                    await SendSeamlessConnectionCommand(true); // Ativar
                }
            }
            else
            {
                Log.Debug("Status da chamada inalterado: {Status} - nenhum comando enviado", isCurrentlyInCall);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro no monitor do Teams: {Message}", ex.Message);
        }
    }
    
    private async Task<bool> GetTeamsCallStatusAsync()
    {
        try
        {
            if (!Directory.Exists(TeamsLogFolder))
            {
                Log.Debug("Teams log folder not found: {Folder}", TeamsLogFolder);
                return false;
            }
            
            var logFiles = Directory.GetFiles(TeamsLogFolder, TeamsLogFilePattern)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Take(3) // Verificar apenas os 3 arquivos mais recentes
                .ToArray();
                
            if (logFiles.Length == 0)
            {
                Log.Debug("No Teams log files found");
                return false;
            }
            
            var cutoffTime = DateTime.UtcNow.AddMinutes(-5); // Verificar apenas logs dos últimos 5 minutos
            
            foreach (var logFile in logFiles)
            {
                try
                {
                    // Usar FileStream com FileShare.ReadWrite para permitir acesso compartilhado
                    using var fileStream = new FileStream(logFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(fileStream, System.Text.Encoding.UTF8);
                    var content = await reader.ReadToEndAsync();
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    
                    // Procurar por eventos de chamada recentes (baseado na memória do sistema)
                    var recentCallEvents = lines
                        .Where(line => line.Contains("NotifyCallActive") || 
                                      line.Contains("NotifyCallAccepted") || 
                                      line.Contains("NotifyCallEnded") ||
                                      line.Contains("call_state_changed") ||
                                      line.Contains("CallState"))
                        .Where(line => IsLineRecent(line, cutoffTime))
                        .ToList();
                    
                    Log.Debug("Found {Count} recent call events in {File}", recentCallEvents.Count, logFile.Name);
                    
                    if (recentCallEvents.Any())
                    {
                        // Log todos os eventos encontrados para debug
                        foreach (var eventLine in recentCallEvents.Take(5)) // Mostrar apenas os 5 mais recentes
                        {
                            Log.Debug("Call event: {Event}", eventLine.Substring(0, Math.Min(200, eventLine.Length)));
                        }
                        
                        // Ordenar eventos por timestamp (mais recente primeiro)
                        var sortedEvents = recentCallEvents
                            .OrderByDescending(line => ExtractTimestamp(line))
                            .ToList();
                        
                        // Verificar o evento mais recente para determinar o status
                        var mostRecentEvent = sortedEvents.FirstOrDefault();
                        if (mostRecentEvent != null)
                        {
                            if (mostRecentEvent.Contains("NotifyCallEnded"))
                            {
                                Log.Debug("Most recent event: Call ENDED");
                                return false;
                            }
                            else if (mostRecentEvent.Contains("NotifyCallActive") || mostRecentEvent.Contains("NotifyCallAccepted"))
                            {
                                Log.Debug("Most recent event: Call ACTIVE/ACCEPTED");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Error reading Teams log file: {File}", logFile.FullName);
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking Teams call status: {Message}", ex.Message);
            return false;
        }
    }
    
    private static bool IsLineRecent(string line, DateTime cutoffTime)
    {
        var timestamp = ExtractTimestamp(line);
        return timestamp >= cutoffTime;
    }
    
    private static DateTime ExtractTimestamp(string line)
    {
        try
        {
            // Extrair timestamp da linha do log (formato típico: 2024-01-15T10:30:45.123Z)
            var timestampMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{3})?(?:Z|[+-]\d{2}:\d{2})?)");
            
            if (timestampMatch.Success && DateTime.TryParse(timestampMatch.Value, out var timestamp))
            {
                return timestamp;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Error parsing timestamp from line");
        }
        
        // Se não conseguir extrair timestamp, retornar valor mínimo
        return DateTime.MinValue;
    }
    
    private async Task SendSeamlessConnectionCommand(bool enable)
    {
        try
        {
            Log.Information("🔍 Verificando conexão do dispositivo...");
            Log.Information($"🔍 BluetoothImpl.Instance.IsConnected: {BluetoothImpl.Instance.IsConnected}");
            
            if (!BluetoothImpl.Instance.IsConnected)
            {
                Log.Warning("⚠️ Dispositivo não conectado - comando não enviado");
                return;
            }
            
            // Usar lógica invertida: false para ativar, true para desativar (conforme protocolo do dispositivo)
            bool commandPayload = !enable;
            
            Log.Information("📤 Enviando comando SET_SEAMLESS_CONNECTION: payload={Payload} para {Action}", 
                commandPayload, enable ? "ATIVAR" : "DESATIVAR");
            
            Log.Information("🔍 DETALHES DO COMANDO:");
            Log.Information($"🔍   - MsgId: {MsgIds.SET_SEAMLESS_CONNECTION}");
            Log.Information($"🔍   - Payload (bool): {commandPayload}");
            Log.Information($"🔍   - Payload (convertido): {Convert.ToByte(commandPayload)}");
            
            Log.Information("🔍 Enviando via BluetoothImpl.Instance.SendRequestAsync...");
            
            try
            {
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.SET_SEAMLESS_CONNECTION, commandPayload);
                Log.Information("✅ Comando enviado com sucesso para o dispositivo");
                Log.Information($"🔍 COMANDO ENVIADO - Esperado: Value={Convert.ToByte(commandPayload)}, RawParameters={Convert.ToByte(commandPayload):X2} (para {(enable ? "ativar" : "desativar")})");
                
                // Aguardar um pouco e verificar se o comando teve efeito
                await Task.Delay(1000);
                Log.Information("🔍 Solicitando status atualizado do dispositivo...");
                await BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Erro ao enviar comando SET_SEAMLESS_CONNECTION: {Error}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao enviar comando SET_SEAMLESS_CONNECTION: {Error}", ex.Message);
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _isRunning = false;
        _monitorTimer?.Dispose();
        Log.Information("Teams integration monitor disposed");
    }
}

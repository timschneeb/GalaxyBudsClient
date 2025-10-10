using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Utils.PendingCommands;
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
    private readonly HashSet<string> _processedEvents = new(); // Cache de eventos já processados
    
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
        // Aguardar mais tempo na inicialização para evitar comando desnecessário
        _monitorTimer?.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(CheckIntervalSeconds));
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
                Log.Information("📞 Teams call status changed: {Previous} -> {Current}", 
                    _lastCallState?.ToString() ?? "unknown", isCurrentlyInCall);
                
                _lastCallState = isCurrentlyInCall;
                
                if (isCurrentlyInCall)
                {
                    Log.Information("🔴 Teams call STARTED - disabling seamless connection");
                    await SendSeamlessConnectionCommand(false); // Desativar
                }
                else
                {
                    Log.Information("🟢 Teams call ENDED - enabling seamless connection");
                    await SendSeamlessConnectionCommand(true); // Ativar
                }
            }
            // Remover log de debug quando status não muda - evita spam no log
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
                .Take(1) // Verificar apenas o arquivo mais recente
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
                    
                    // Filter out already processed events to avoid spam
                    var newEvents = recentCallEvents
                        .Where(line => !_processedEvents.Contains(line.GetHashCode().ToString()))
                        .ToList();
                    
                    // Only process if there are genuinely new events
                    if (newEvents.Any())
                    {
                        Log.Debug("Found {NewCount} new call events (out of {Total} recent) in {File}", 
                            newEvents.Count, recentCallEvents.Count, logFile.Name);
                        
                        // Add new events to processed cache (keep cache size reasonable)
                        foreach (var eventLine in newEvents)
                        {
                            _processedEvents.Add(eventLine.GetHashCode().ToString());
                        }
                        
                        // Limit cache size to prevent memory issues
                        if (_processedEvents.Count > 100)
                        {
                            var oldEvents = _processedEvents.Take(_processedEvents.Count - 50).ToList();
                            foreach (var oldEvent in oldEvents)
                            {
                                _processedEvents.Remove(oldEvent);
                            }
                        }
                        
                        // Only log events in verbose mode to reduce spam
                        if (Log.IsEnabled(Serilog.Events.LogEventLevel.Verbose))
                        {
                            foreach (var eventLine in newEvents.Take(2)) // Show only top 2 new events
                            {
                                Log.Verbose("New Teams call event: {Event}", eventLine.Substring(0, Math.Min(120, eventLine.Length)));
                            }
                        }
                    }
                    
                    // Always process all recent events to determine current status (but only log new ones)
                    if (recentCallEvents.Any())
                    {
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
                                return false;
                            }
                            else if (mostRecentEvent.Contains("NotifyCallActive") || mostRecentEvent.Contains("NotifyCallAccepted"))
                            {
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
            // Usar lógica invertida: false para ativar, true para desativar (conforme protocolo do dispositivo)
            bool commandPayload = !enable;
            
            string description = $"Teams: {(enable ? "ATIVAR" : "DESATIVAR")} conexão direta";
            
            Log.Information("📤 Enviando comando via fila de comandos pendentes: {Description}", description);
            
            // Usar o sistema de fila de comandos pendentes
            bool wasSentImmediately = await PendingCommandsManager.Instance.SendCommandAsync(
                MsgIds.SET_SEAMLESS_CONNECTION, 
                commandPayload, 
                description);
            
            if (wasSentImmediately)
            {
                Log.Information("✅ Comando enviado imediatamente (dispositivo conectado)");
                
                // Aguardar um pouco e solicitar status atualizado
                await Task.Delay(1000);
                await PendingCommandsManager.Instance.SendCommandAsync(
                    MsgIds.DEBUG_GET_ALL_DATA, 
                    null, 
                    "Teams: Solicitar status após comando");
            }
            else
            {
                Log.Information("📋 Comando adicionado à fila (dispositivo desconectado) - será enviado automaticamente na reconexão");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Erro ao processar comando SET_SEAMLESS_CONNECTION: {Error}", ex.Message);
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

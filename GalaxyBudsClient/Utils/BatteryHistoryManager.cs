using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using GalaxyBudsClient.Interface;
using GalaxyBudsClient.Interface.ViewModels.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Database;
using GalaxyBudsClient.Platform;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GalaxyBudsClient.Utils;

public class BatteryHistoryManager
{
    // Note: must not be a Mutex. Mutexes are thread-affine and re-entrant, which breaks across 'await'
    // (ReleaseMutex may run on a different thread, and re-entrant acquisition on the UI thread does not exclude).
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _currentMac;
    private HistoryRecord? _lastRecord;
    
    private const int RetainDays = 30;
    
    private BatteryHistoryManager()
    {
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        
        SppMessageReceiver.Instance.StatusUpdate += async (_, _) => await CollectNow(); 
        MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>()!
            .PropertyChanged += async (_, _) => await CollectNow();

        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Exit += (_, _) => _ = CollectNow(insertNullFrame: true);
        }
        
        _currentMac = BluetoothImpl.Instance.Device.Current?.MacAddress;
        _ = CollectNow(insertNullFrame: true);
    }

    private async void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.Data.CollectBatteryHistory) &&
            Settings.Data.CollectBatteryHistory == false)
        {
            Log.Debug("Battery history disabled. Deleting all databases");
            await DeleteAllDatabasesAsync();
        }
    }

    private async void OnDeviceChanged(object? sender, Device? e)
    {
        _currentMac = BluetoothImpl.Instance.Device.Current?.MacAddress;
        _lastRecord = null;
        await CollectNow(insertNullFrame: true);
    }
    
    private async void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected))
        {
            if(!BluetoothImpl.Instance.IsConnected)
                _lastRecord = null;
            
            await CollectNow();
            await CleanupNow();
        }
    }
    
    public async Task DeleteDatabaseForDeviceAsync(Device device)
    {
        if (string.IsNullOrWhiteSpace(device.MacAddress))
            return;

        var mac = device.MacAddress;
        Log.Debug("Deleting battery stats for {Dev}", mac);

        if (await _lock.WaitAsync(2000))
        {
            try
            {
                if (_currentMac != null && string.Equals(_currentMac, mac, StringComparison.OrdinalIgnoreCase))
                {
                    _lastRecord = null;
                    
                    // Only stop tracking the MAC if the device was actually unregistered.
                    // If history was merely disabled, keep it so collection resumes once re-enabled.
                    if (Settings.Data.Devices.All(x => !string.Equals(x.MacAddress, mac, StringComparison.OrdinalIgnoreCase)))
                        _currentMac = null;
                }

                // Clear connection pools so that SQLite releases active OS file handles
                SqliteConnection.ClearAllPools();

                var path = GetPathForMac(mac);
                if (File.Exists(path))
                    File.Delete(path);

                var journalPath = path + "-journal";
                if (File.Exists(journalPath))
                    File.Delete(journalPath);

                var walPath = path + "-wal";
                if (File.Exists(walPath))
                    File.Delete(walPath);

                var shmPath = path + "-shm";
                if (File.Exists(shmPath))
                    File.Delete(shmPath);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to delete battery stats for {Dev}", mac);
            }
            finally
            {
                _lock.Release();
            }
        }
        else
        {
            Log.Warning("Timed out waiting for lock while deleting battery stats for {Dev}", mac);
        }
    }

    public async Task DeleteAllDatabasesAsync()
    {
        foreach (var device in Settings.Data.Devices.ToArray())
        {
            await DeleteDatabaseForDeviceAsync(device);
        }
    }
    
    private async Task CleanupNow()
    {
        if (!PlatformUtils.IsDesktop || _currentMac == null || !Settings.Data.CollectBatteryHistory)
            return;
        
        if (await _lock.WaitAsync(500))
        {
            try
            {
                if (_currentMac == null || !Settings.Data.CollectBatteryHistory)
                    return;

                Log.Debug("Cleaning up old battery stats for {Dev}", _currentMac);
                
                await using var db = new HistoryDbContext(GetPathForMac(_currentMac));
                await db.Database.MigrateAsync();
                db.ExecutePragmas();
                
                var cutOffDate = DateTime.Now - TimeSpan.FromDays(RetainDays);
                foreach (var record in db.Records.Where(record => record.Timestamp <= cutOffDate))
                {
                    db.Records.Remove(record);
                }
                
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to clean up battery stats for {Dev}", _currentMac);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
    
    private async Task CollectNow(bool insertNullFrame = false)
    {
        if (!PlatformUtils.IsDesktop || _currentMac == null || !Settings.Data.CollectBatteryHistory)
        {
            _lastRecord = null;
            return;
        }

        var status = !insertNullFrame && BluetoothImpl.Instance.IsConnected ? DeviceMessageCache.Instance.StatusUpdate : null;
        var noiseControlVm = !insertNullFrame && BluetoothImpl.Instance.IsConnected ? MainView.Instance!.ResolveViewModelByType<NoiseControlPageViewModel>() : null;
        var record = new HistoryRecord
        {
            PlacementL = status?.PlacementL ?? PlacementStates.Disconnected,
            PlacementR = status?.PlacementR ?? PlacementStates.Disconnected,
            BatteryL = status?.BatteryL,
            BatteryR = status?.BatteryR,
            BatteryCase = status?.BatteryCase,
            IsChargingL = status?.IsLeftCharging,
            IsChargingR = status?.IsRightCharging,
            IsChargingCase = status?.IsCaseCharging,
            HostDevice = status?.MainConnection,
            NoiseControlMode = noiseControlVm?.NoiseControlMode,
            Timestamp = DateTime.Now
        };
        
        if (HistoryRecord.HistoryRecordComparer.Equals(record, _lastRecord))
            return;
        
        if (await _lock.WaitAsync(50))
        {
            try
            {
                if (_currentMac == null || !Settings.Data.CollectBatteryHistory)
                    return;

                await using var db = new HistoryDbContext(GetPathForMac(_currentMac));
                await db.Database.MigrateAsync();
                db.ExecutePragmas();
                
                _lastRecord = record;
                db.Records.Add(record);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to collect battery history record for {Dev}", _currentMac);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
   
    private static string GetPathForMac(string mac)
    {
        return PlatformUtils.CombineDataPath($"battery_stats_{mac.Replace(":","")}.db");
    }
    
    public static async Task<DisposableQuery<HistoryRecord>> BeginDisposableQueryAsync(Device? device = null)
    {
        device ??= BluetoothImpl.Instance.Device.Current;
        if (device == null)
            return new DisposableQuery<HistoryRecord>(null);
        
        try
        {
            var db = new HistoryDbContext(GetPathForMac(device.MacAddress));
            await db.Database.MigrateAsync();
            db.ExecutePragmas();
            
            return new DisposableQuery<HistoryRecord>(db);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize battery history database query for {Dev}", device.MacAddress);
            return new DisposableQuery<HistoryRecord>(null);
        }
    }
    
    #region Singleton
    private static readonly object Padlock = new();
    private static BatteryHistoryManager? _instance;
    public static BatteryHistoryManager Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new BatteryHistoryManager();
            }
        }
    }

    public static void Init()
    {
        lock (Padlock)
        {
            _instance ??= new BatteryHistoryManager();
        }
    }
    #endregion
}
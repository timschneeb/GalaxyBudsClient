using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GalaxyBudsClient.Utils;

public class BatteryHistoryManager
{
    private readonly Mutex _lock = new();
    private string? _currentMac;
    private HistoryRecord? _lastRecord;
    
    private const int RetainDays = 30;
    
    private BatteryHistoryManager()
    {
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        
        SppMessageReceiver.Instance.StatusUpdate += async (_, _) => await CollectNow();
        MainWindow.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>()!
            .PropertyChanged += async (_, _) => await CollectNow();

        if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Exit += (_, _) => _ = CollectNow(insertNullFrame: true);
        }
        
        _currentMac = BluetoothImpl.Instance.Device.Current?.MacAddress;
        _ = CollectNow(insertNullFrame: true);
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.Data.CollectBatteryHistory) &&
            Settings.Data.CollectBatteryHistory == false)
        {
            Log.Debug("Battery history disabled. Deleting all databases.");
            DeleteAllDatabases();
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
    
    public void DeleteDatabaseForDevice(Device device)
    {
        Log.Debug("Deleting battery stats for {Dev}", _currentMac);
        var path = GetPathForMac(device.MacAddress);
        if(File.Exists(path))
            File.Delete(path);
    }

    public void DeleteAllDatabases()
    {
        foreach (var device in Settings.Data.Devices)
        {
            DeleteDatabaseForDevice(device);
        }
    }
    
    private async Task CleanupNow()
    {
        if (_currentMac == null || !Settings.Data.CollectBatteryHistory)
            return;
        
        if (_lock.WaitOne(500))
        {
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
            _lock.ReleaseMutex();
        }
    }
    
    private async Task CollectNow(bool insertNullFrame = false)
    {
        if (_currentMac == null || !Settings.Data.CollectBatteryHistory)
        {
            _lastRecord = null;
            return;
        };
        
        var status = !insertNullFrame && BluetoothImpl.Instance.IsConnected ? DeviceMessageCache.Instance.StatusUpdate : null;
        var noiseControlVm = !insertNullFrame && BluetoothImpl.Instance.IsConnected ? MainWindow.Instance.MainView.ResolveViewModelByType<NoiseControlPageViewModel>() : null;
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
        
        if(HistoryRecord.HistoryRecordComparer.Equals(record, _lastRecord))
            return;
        
        if (_lock.WaitOne(50))
        {
            await using var db = new HistoryDbContext(GetPathForMac(_currentMac));
            await db.Database.MigrateAsync();
            db.ExecutePragmas();
            
            _lastRecord = record;
            db.Records.Add(record);
            await db.SaveChangesAsync();
            
            _lock.ReleaseMutex();
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
        
        var db = new HistoryDbContext(GetPathForMac(device.MacAddress));
        await db.Database.MigrateAsync();
        db.ExecutePragmas();
        
        return new DisposableQuery<HistoryRecord>(db);
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
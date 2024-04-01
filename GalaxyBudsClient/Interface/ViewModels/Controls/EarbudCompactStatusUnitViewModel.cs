using System.ComponentModel;
using Avalonia.Controls;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Controls;

public class EarbudCompactStatusUnitViewModel : ViewModelBase
{
    [Reactive] public bool IsLeftOnline { set; get; }
    [Reactive] public bool IsRightOnline { set; get; }
    [Reactive] public int LeftBattery { set; get; }
    [Reactive] public int RightBattery { set; get; }
    [Reactive] public int? CaseBattery { set; get; }
    [Reactive] public PlacementStates LeftWearState { set; get; }
    [Reactive] public PlacementStates RightWearState { set; get; }
    
    [Reactive] public GridLength CenterColumnWidth { set; get; } = new(75);
    
    public EarbudCompactStatusUnitViewModel()
    {
        SppMessageHandler.Instance.BaseUpdate += OnStatusUpdated;
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
        Loc.LanguageUpdated += LoadFromCache;
        LoadFromCache();
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected) && 
            DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Recalculate temperature values when temperature unit changes
        if (e.PropertyName == nameof(Settings.Instance.TemperatureUnit))
            LoadFromCache();
    }
    
    private void LoadFromCache()
    {
        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        var connected = BluetoothImpl.Instance.IsConnected;
        LeftBattery = e.BatteryL;
        RightBattery = e.BatteryR;
        CaseBattery = e.BatteryCase is <= 0 or > 100 ? null : e.BatteryCase;
        LeftWearState = e.PlacementL;
        RightWearState = e.PlacementR;
        IsLeftOnline = connected && e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected;
        IsRightOnline = connected && e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected;

        CenterColumnWidth = new GridLength(CaseBattery != null ? 75 : 55);
    }
}
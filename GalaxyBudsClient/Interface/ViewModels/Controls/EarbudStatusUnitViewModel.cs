using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.ViewModels.Controls;

public partial class EarbudStatusUnitViewModel : ViewModelBase
{
    [Reactive] private bool _isLeftOnline;
    [Reactive] private bool _isRightOnline;
    [Reactive] private int _leftBattery;
    [Reactive] private int _rightBattery;
    [Reactive] private int? _caseBattery;
    [Reactive] private double _leftVoltage;
    [Reactive] private double _rightVoltage;
    [Reactive] private double _leftCurrent;
    [Reactive] private double _rightCurrent;
    [Reactive] private double _leftTemperature;
    [Reactive] private double _rightTemperature;
    [Reactive] private PlacementStates _leftWearState;
    [Reactive] private PlacementStates _rightWearState;
    
    public EarbudStatusUnitViewModel()
    {
        SppMessageReceiver.Instance.BaseUpdate += OnStatusUpdated;
        SppMessageReceiver.Instance.GetAllDataResponse += OnGetAllDataResponse;
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        Loc.LanguageUpdated += LoadFromCache;
        LoadFromCache();
        
        _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected) && 
                DeviceMessageCache.Instance.BasicStatusUpdate != null)
                OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        });
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Recalculate temperature values when temperature unit changes
            if (e.PropertyName == nameof(Settings.Data.TemperatureUnit))
                LoadFromCache();
        });
    }
    
    private void LoadFromCache()
    {
        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        if (DeviceMessageCache.Instance.DebugGetAllData != null)
            OnGetAllDataResponse(null, DeviceMessageCache.Instance.DebugGetAllData);
        else
            _ = BluetoothImpl.Instance.SendRequestAsync(MsgIds.DEBUG_GET_ALL_DATA);
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var connected = BluetoothImpl.Instance.IsConnected;
            LeftBattery = e.BatteryL;
            RightBattery = e.BatteryR;
            CaseBattery = e.BatteryCase is <= 0 or > 100 ? null : e.BatteryCase;
            LeftWearState = e.PlacementL;
            RightWearState = e.PlacementR;
            IsLeftOnline = connected && e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected;
            IsRightOnline = connected && e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected;
        });
    }
        
    private void OnGetAllDataResponse(object? sender, DebugGetAllDataDecoder e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Buds2 Pro seem to send 0 for all values sometimes. Ignore those updates.
            if(e is { LeftThermistor: <= 0, RightThermistor: <= 0 })
                return;
            
            var useF = Settings.Data.TemperatureUnit == TemperatureUnits.Fahrenheit;
            LeftVoltage = e.LeftAdcVCell;
            RightVoltage = e.RightAdcVCell;
            LeftCurrent = e.LeftAdcCurrent;
            RightCurrent = e.RightAdcCurrent;
            LeftTemperature = useF ? 9.0 / 5.0 * e.LeftThermistor + 32 : e.LeftThermistor;
            RightTemperature = useF ? 9.0 / 5.0 * e.RightThermistor + 32 :  e.RightThermistor;
        });
    }
}

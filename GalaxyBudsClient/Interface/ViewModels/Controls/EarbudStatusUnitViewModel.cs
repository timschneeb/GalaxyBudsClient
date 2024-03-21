using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Controls;

public class EarbudStatusUnitViewModel : ViewModelBase
{
    [Reactive] public bool IsLeftOnline { set; get; }
    [Reactive] public bool IsRightOnline { set; get; }
    [Reactive] public int LeftBattery { set; get; }
    [Reactive] public int RightBattery { set; get; }
    [Reactive] public double LeftVoltage { set; get; }
    [Reactive] public double RightVoltage { set; get; }
    [Reactive] public double LeftCurrent { set; get; }
    [Reactive] public double RightCurrent { set; get; }
    [Reactive] public double LeftTemperature { set; get; }
    [Reactive] public double RightTemperature { set; get; }
    [Reactive] public PlacementStates LeftWearState { set; get; }
    [Reactive] public PlacementStates RightWearState { set; get; }
    
    public EarbudStatusUnitViewModel()
    {
        SppMessageHandler.Instance.BaseUpdate += OnStatusUpdated;
        SppMessageHandler.Instance.GetAllDataResponse += OnGetAllDataResponse;
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
        Loc.LanguageUpdated += LoadFromCache;
        LoadFromCache();
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
        if (DeviceMessageCache.Instance.DebugGetAllData != null)
            OnGetAllDataResponse(null, DeviceMessageCache.Instance.DebugGetAllData);
        else
            _ = BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.DEBUG_GET_ALL_DATA);
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        LeftWearState = e.PlacementL;
        RightWearState = e.PlacementR;
    }
        
    private void OnGetAllDataResponse(object? sender, DebugGetAllDataParser e)
    {
        var useF = Settings.Instance.TemperatureUnit == TemperatureUnits.Fahrenheit;
        
        IsLeftOnline = e.LeftAdcSOC > 0;
        IsRightOnline = e.RightAdcSOC > 0;
        LeftBattery = (int)Math.Round(e.LeftAdcSOC);
        RightBattery = (int)Math.Round(e.RightAdcSOC);
        
        LeftVoltage = e.LeftAdcVCell;
        RightVoltage = e.RightAdcVCell;
        LeftCurrent = e.LeftAdcCurrent;
        RightCurrent = e.RightAdcCurrent;
        LeftTemperature = useF ? 9.0 / 5.0 * e.LeftThermistor + 32 : e.LeftThermistor;
        RightTemperature = useF ? 9.0 / 5.0 * e.RightThermistor + 32 :  e.RightThermistor;
    }
}
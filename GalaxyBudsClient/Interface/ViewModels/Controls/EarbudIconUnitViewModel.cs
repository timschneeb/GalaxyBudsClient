using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using ReactiveUI.Fody.Helpers;

namespace GalaxyBudsClient.Interface.ViewModels.Controls;

public class EarbudIconUnitViewModel : ViewModelBase
{
    [Reactive] public IImage? LeftIcon { set; get; }
    [Reactive] public IImage? RightIcon { set; get; }
    [Reactive] public bool IsLeftOnline { set; get; }
    [Reactive] public bool IsRightOnline { set; get; }
    
    public EarbudIconUnitViewModel()
    {
        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        SppMessageHandler.Instance.BaseUpdate += OnStatusUpdated;
        BluetoothService.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        UpdateEarbudIcons();
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(BluetoothService.Instance.IsConnected) && 
           DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        
        UpdateEarbudIcons();
    }

    private void UpdateEarbudIcons()
    {
        var type = BluetoothService.Instance.DeviceSpec.IconResourceKey;
        LeftIcon = Application.Current?.FindResource($"Left{type}Connected") as IImage;
        RightIcon = Application.Current?.FindResource($"Right{type}Connected") as IImage;
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        var connected = BluetoothService.Instance.IsConnected;
        IsLeftOnline = connected && e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected;
        IsRightOnline = connected && e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected;
    }
}
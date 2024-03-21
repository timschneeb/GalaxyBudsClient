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
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        UpdateEarbudIcons();
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateEarbudIcons();
    }

    private void UpdateEarbudIcons()
    {
        var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
        LeftIcon = Application.Current?.FindResource($"Left{type}Connected") as IImage;
        RightIcon = Application.Current?.FindResource($"Right{type}Connected") as IImage;
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        IsLeftOnline = e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected;
        IsRightOnline = e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected;
    }
}
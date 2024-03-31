using System;
using System.ComponentModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Colors = GalaxyBudsClient.Model.Constants.Colors;

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
        Settings.Instance.PropertyChanged += OnMainSettingsPropertyChanged;
        UpdateEarbudIcons();
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(ISettings.RealisticEarbudImages))
            UpdateEarbudIcons();
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected) && 
           DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        
        UpdateEarbudIcons();
    }

    private void UpdateEarbudIcons()
    {
        var extCache = DeviceMessageCache.Instance.ExtendedStatusUpdate;
        if (Settings.Instance.RealisticEarbudImages && extCache != null && BluetoothImpl.Instance.DeviceSpec.Supports(Features.DeviceColor))
        {
            Uri GetUri(int variant) => new($"{Program.AvaresUrl}/Resources/Device/Realistic/{extCache.DeviceColor}-{variant}.png");
            try
            {
                LeftIcon = new Bitmap(AssetLoader.Open(GetUri(0)));
                RightIcon = new Bitmap(AssetLoader.Open(GetUri(1)));
                return;
            }
            catch(FileNotFoundException ex)
            {
                Log.Warning(ex, "Failed to load earbud icon asset from {File}", ex.FileName);
                // This should not happen, but if it does, we'll fall back to the default icons
            }
        }
        
        var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
        LeftIcon = Application.Current?.FindResource($"Left{type}Connected") as IImage;
        RightIcon = Application.Current?.FindResource($"Right{type}Connected") as IImage;
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        var connected = BluetoothImpl.Instance.IsConnected;
        IsLeftOnline = connected && e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected;
        IsRightOnline = connected && e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected;
    }
}
using System;
using System.ComponentModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Interface.Controls;

public class EarbudIcon : Image
{
    public EarbudIcon()
    {
        Height = Width = 75;

        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);
        SppMessageReceiver.Instance.BaseUpdate += OnStatusUpdated;
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
        BluetoothImpl.Instance.Connected += OnConnected;
        Settings.MainSettingsPropertyChanged += OnMainSettingsPropertyChanged;
        Settings.DevicePropertyChanged += OnDevicePropertyChanged;
        Settings.Data.PropertyChanged += OnSettingsDataPropertyChanged;
        UpdateEarbudIcons();
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateEarbudIcons();
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        UpdateEarbudIcons();
    }

    public static readonly StyledProperty<Devices> SideProperty = AvaloniaProperty.Register<EarbudIcon, Devices>(nameof(Side));
    public Devices Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if(change.Property == SideProperty)
            UpdateEarbudIcons();
        else
            base.OnPropertyChanged(change);
    }

    private void OnMainSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(Settings.Data.RealisticEarbudImages))
            UpdateEarbudIcons();
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected) &&
                DeviceMessageCache.Instance.BasicStatusUpdate != null)
                OnStatusUpdated(null, DeviceMessageCache.Instance.BasicStatusUpdate);

            UpdateEarbudIcons();
        });
    }

    private void OnSettingsDataPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.Data.ColorOverride))
            UpdateEarbudIcons();
    }

    private void UpdateEarbudIcons()
    {
        var color = Settings.Data.ColorOverride ?? BluetoothImpl.Instance.Device.Current?.DeviceColor ?? DeviceMessageCache.Instance.ExtendedStatusUpdate?.DeviceColor;
        if (Settings.Data.RealisticEarbudImages && color != null &&
            BluetoothImpl.Instance.DeviceSpec.Supports(Features.DeviceColor))
        {
            Uri GetUri(int variant) => new($"{Program.AvaresUrl}/Resources/Device/Realistic/{color}-{variant}.png");

            try
            {
                Source = new Bitmap(AssetLoader.Open(GetUri((int)Side)));
                return;
            }
            catch (FileNotFoundException ex)
            {
                Log.Warning("Failed to load earbud icon asset: {Msg}", ex.Message);
                // This should not happen, but if it does, we'll fall back to the default icons
            }
        }

        var type = BluetoothImpl.Instance.DeviceSpec.IconResourceKey;
        Source = Application.Current?.FindResource($"{(Side == Devices.L ? "Left" : "Right")}{type}Connected") as IImage;
    }
    
    private void OnStatusUpdated(object? sender, IBasicStatusUpdate e)
    {
        var connected = BluetoothImpl.Instance.IsConnected;
        var isOnline = (Side == Devices.L && e.BatteryL > 0 && e.PlacementL != PlacementStates.Disconnected) ||
                       (Side == Devices.R && e.BatteryR > 0 && e.PlacementR != PlacementStates.Disconnected);
        Opacity = connected && isOnline ? 1 : 0.4;
        UpdateEarbudIcons();
    }
}
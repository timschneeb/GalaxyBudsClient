using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if the computer does not support SSC or if the device mode doesn't support game mode, or disables it if SSC is not the chosen codec.
/// </summary>
/// <remarks>
/// This behavior updates the <see cref="Control.IsEnabled"/> and <see cref="Control.IsVisible"/> property of the associated control and may clash with existing bindings.
/// </remarks>
public class GameModeBehavior : Behavior<Control>
{
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        BluetoothImpl.Instance.IsSscChanged += OnBluetoothPropertyChanged;
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        UpdateState();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        BluetoothImpl.Instance.IsSscChanged -= OnBluetoothPropertyChanged;
        SppMessageReceiver.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        BluetoothImpl.Instance.Device.DeviceChanged -= OnDeviceChanged;
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        Dispatcher.UIThread.Post(UpdateState, DispatcherPriority.Normal);
    }

    private void OnDeviceChanged(object? sender, Device? e)
    {
        Dispatcher.UIThread.Post(UpdateState, DispatcherPriority.Normal);
    }

    private void OnBluetoothPropertyChanged(object? sender, object? e)
    {
        Dispatcher.UIThread.Post(UpdateState, DispatcherPriority.Normal);
    }

    protected void UpdateState()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.IsVisible = BluetoothImpl.Instance.CanBeSsc && BluetoothImpl.Instance.DeviceSpec.Supports(Features.AdjustSoundSync);
        AssociatedObject.IsEnabled = BluetoothImpl.Instance.IsSsc;
    }
}

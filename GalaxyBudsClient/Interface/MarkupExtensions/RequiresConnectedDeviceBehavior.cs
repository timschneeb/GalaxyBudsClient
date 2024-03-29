using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if no device is connected.
/// </summary>
/// <remarks>
/// This behavior updates the <see cref="Control.IsEnabled"/> property of the associated control and may clash with existing bindings.
/// </remarks>
public class RequiresConnectedDeviceBehavior : Behavior<Control>
{
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateState();
        BluetoothImpl.Instance.PropertyChanged += OnBluetoothPropertyChanged;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        BluetoothImpl.Instance.PropertyChanged -= OnBluetoothPropertyChanged;
    }

    private void OnBluetoothPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BluetoothImpl.Instance.IsConnected))
        {
            Dispatcher.UIThread.Post(UpdateState, DispatcherPriority.Normal);
        }
    }
    
    protected virtual void UpdateState()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.IsEnabled = BluetoothImpl.Instance.IsConnected;
    }
}
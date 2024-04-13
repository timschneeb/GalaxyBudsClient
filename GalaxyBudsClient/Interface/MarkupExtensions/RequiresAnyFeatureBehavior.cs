using System.Collections;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Config;
using GalaxyBudsClient.Model.Config.Legacy;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if the connected device does not support the specified feature.
/// </summary>
public class RequiresAnyFeatureBehavior : Behavior<Control>
{
    public static readonly StyledProperty<IEnumerable> FeaturesProperty =
        AvaloniaProperty.Register<RequiresAnyFeatureBehavior, IEnumerable>(nameof(Features));
    
    public IEnumerable Features
    {
        get => GetValue(FeaturesProperty);
        set => SetValue(FeaturesProperty, value);
    }
    
    
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateState();
        BluetoothImpl.Instance.Device.DeviceChanged += OnDeviceChanged;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        SppMessageReceiver.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        BluetoothImpl.Instance.Device.DeviceChanged -= OnDeviceChanged;
    }
    
    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder e)
    {
        UpdateState();
    }
    
    private void OnDeviceChanged(object? sender, Device? e)
    {
        UpdateState();
    }
    
    protected void UpdateState()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.IsVisible = Features.Cast<Features>().Any(BluetoothImpl.Instance.DeviceSpec.Supports);
    }
}
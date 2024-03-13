using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

/// <summary>
/// Behavior that hides the control if the connected device does not support the specified feature.
/// </summary>
public class RequiresFeatureBehavior : Behavior<Control>
{
    public static readonly StyledProperty<IDeviceSpec.Feature> FeatureProperty =
        AvaloniaProperty.Register<RequiresFeatureBehavior, IDeviceSpec.Feature>(nameof(Feature));
    
    public IDeviceSpec.Feature Feature
    {
        get => GetValue(FeatureProperty);
        set => SetValue(FeatureProperty, value);
    }
    
    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        UpdateVisibility();
        Settings.Instance.RegisteredDevice.PropertyChanged += OnDevicePropertyChanged;
    }
    
    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        Settings.Instance.RegisteredDevice.PropertyChanged -= OnDevicePropertyChanged;
    }

    private void OnDevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateVisibility();
    }
    
    private void UpdateVisibility()
    {
        if (AssociatedObject is null)
            return;

        AssociatedObject.IsVisible = BluetoothImpl.Instance.DeviceSpec.Supports(Feature);
    }
}
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsSliderItem : SettingsSymbolItem
{
    public SettingsSliderItem()
    {
        _slider = new Slider
        {
            Orientation = Orientation.Horizontal,
            MinWidth = 250,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            TickPlacement = TickPlacement.BottomRight
        };
        _slider.ValueChanged += OnValueChanged;
        IsClickEnabled = false;
        Footer = _slider;
    }

    private readonly Slider _slider;
    
    public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent = 
        RoutedEvent.Register<SettingsSwitchItem, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<int> ValueProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Minimum), defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<int> MaximumProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Maximum), defaultBindingMode: BindingMode.TwoWay);
    
    public event EventHandler<RoutedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public int Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private void OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        Value = (int)Math.Round(e.NewValue);
        RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ValueProperty)
        {
            _slider.Value = Value;
        }
        else if (change.Property == MinimumProperty)
        {
            _slider.Minimum = Minimum;
        }
        else if (change.Property == MaximumProperty)
        {
            _slider.Maximum = Maximum;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
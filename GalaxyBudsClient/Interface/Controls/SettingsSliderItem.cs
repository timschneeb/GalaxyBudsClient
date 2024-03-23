using System;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Serilog;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsSliderItem : SettingsSymbolItem
{
    public SettingsSliderItem()
    {
        _timer = new Timer(130);
        _timer.Elapsed += OnDebounceTimerElapsed;
        
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
        TickPlacement = TickPlacement.BottomRight;
        Footer = _slider;
    }
    
    private readonly Timer _timer;
    private readonly Slider _slider;
    
    public static readonly RoutedEvent<RoutedEventArgs> ValueChangedEvent = 
        RoutedEvent.Register<SettingsSwitchItem, RoutedEventArgs>(nameof(ValueChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<int> ValueProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<int> MinimumProperty =
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Minimum), defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly StyledProperty<int> MaximumProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, int>(nameof(Maximum), defaultBindingMode: BindingMode.TwoWay);
   
    public static readonly StyledProperty<bool> DebounceProperty = 
        AvaloniaProperty.Register<SettingsSwitchItem, bool>(nameof(Debounce), defaultBindingMode: BindingMode.OneWay);
   
    public static readonly StyledProperty<TickPlacement> TickPlacementProperty = 
        Slider.TickPlacementProperty.AddOwner<SettingsSliderItem>();
    
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

    public bool Debounce
    {
        get => GetValue(DebounceProperty);
        set => SetValue(DebounceProperty, value);
    }

    public TickPlacement TickPlacement
    {
        get => GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    private void OnDebounceTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        Dispatcher.UIThread.Post(() => UpdateValue(_slider.Value), DispatcherPriority.Input);
    }

    private void OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (Debounce)
        {
            // Log.Error("Debounce: timer already enabled? {B}", _timer.Enabled);
            if (_timer.Enabled == false)
            {
                _timer.Start();
            }
        }
        else
        {
            UpdateValue(e.NewValue);
        }
    }

    private void UpdateValue(double newValue)
    {
        Value = (int)Math.Round(newValue);
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
        else if (change.Property == TickPlacementProperty)
        {
            _slider.TickPlacement = TickPlacement;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
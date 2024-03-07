using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using AvColor = Avalonia.Media.Color;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsColorItem : SettingsSymbolItem
{
    public SettingsColorItem()
    {
        _button = new ColorPickerButton()
        {
            IsMoreButtonVisible = true,
            UseSpectrum = true,
            UseColorPalette = true,
            UseColorWheel = true,
            UseColorTriangle = true,
            IsCompact = true,
            ShowAcceptDismissButtons = true, 
            CustomPaletteColors = Palette,
            IsAlphaEnabled = false
        };
        _button.ColorChanged += OnColorChanged;
        Footer = _button;
    }

    private readonly ColorPickerButton _button;
    
    public static readonly RoutedEvent<RoutedEventArgs> ColorChangedEvent = 
        RoutedEvent.Register<SettingsSwitchItem, RoutedEventArgs>(nameof(ColorChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<AvColor?> ColorProperty = 
        ColorPickerButton.ColorProperty.AddOwner<SettingsSwitchItem>();
    
    public event EventHandler<RoutedEventArgs>? ColorChanged
    {
        add => AddHandler(ColorChangedEvent, value);
        remove => RemoveHandler(ColorChangedEvent, value);
    }

    public AvColor? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    
    private void OnColorChanged(ColorPickerButton sender, ColorButtonColorChangedEventArgs args)
    {
        Color = args.NewColor;
        RaiseEvent(new RoutedEventArgs(ColorChangedEvent));
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ColorProperty)
        {
            _button.Color = Color;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
    
    private IEnumerable<AvColor> Palette { get; } = new[]
    {
        Colors.Orange,
        AvColor.FromRgb(255,140,0),
        AvColor.FromRgb(247,99,12),
        AvColor.FromRgb(202,80,16),
        AvColor.FromRgb(218,59,1),
        AvColor.FromRgb(239,105,80),
        AvColor.FromRgb(209,52,56),
        AvColor.FromRgb(255,67,67),
        AvColor.FromRgb(231,72,86),
        AvColor.FromRgb(232,17,35),
        AvColor.FromRgb(234,0,94),
        AvColor.FromRgb(195,0,82),
        AvColor.FromRgb(227,0,140),
        AvColor.FromRgb(191,0,119),
        AvColor.FromRgb(194,57,179),
        AvColor.FromRgb(154,0,137),
        AvColor.FromRgb(0,120,212),
        AvColor.FromRgb(0,99,177),
        AvColor.FromRgb(142,140,216),
        AvColor.FromRgb(107,105,214),
        AvColor.FromRgb(135,100,184),
        AvColor.FromRgb(116,77,169),
        AvColor.FromRgb(177,70,194),
        AvColor.FromRgb(136,23,152),
        AvColor.FromRgb(0,153,188),
        AvColor.FromRgb(45,125,154),
        AvColor.FromRgb(0,183,195),
        AvColor.FromRgb(3,131,135),
        AvColor.FromRgb(0,178,148),
        AvColor.FromRgb(1,133,116),
        AvColor.FromRgb(0,204,106),
        AvColor.FromRgb(16,137,62),
        AvColor.FromRgb(122,117,116),
        AvColor.FromRgb(93,90,88),
        AvColor.FromRgb(104,118,138),
        AvColor.FromRgb(81,92,107),
        AvColor.FromRgb(86,124,115),
        AvColor.FromRgb(72,104,96),
        AvColor.FromRgb(73,130,5),
        AvColor.FromRgb(16,124,16),
        AvColor.FromRgb(118,118,118),
        AvColor.FromRgb(76,74,72),
        AvColor.FromRgb(105,121,126),
        AvColor.FromRgb(74,84,89),
        AvColor.FromRgb(100,124,100),
        AvColor.FromRgb(82,94,84),
        AvColor.FromRgb(132,117,69),
        AvColor.FromRgb(126,115,95)
    };
}

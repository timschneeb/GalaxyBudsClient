using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsSwitchItem : SettingsSymbolItem
{
    public SettingsSwitchItem()
    {
        _toggle = new ToggleSwitch();
        _toggle.IsCheckedChanged += (_, _) => IsChecked = _toggle.IsChecked;

        Click += OnClick;
        IsClickEnabled = true;
        Footer = _toggle;

        Loc.LanguageUpdated += OnLanguageUpdated;
        OnLanguageUpdated();
    }

    private readonly ToggleSwitch _toggle;

    public static readonly RoutedEvent<RoutedEventArgs> IsCheckedChangedEvent =
        RoutedEvent.Register<SettingsSwitchItem, RoutedEventArgs>(nameof(IsCheckedChanged), RoutingStrategies.Bubble);

    public static readonly StyledProperty<bool?> IsCheckedProperty =
        ToggleButton.IsCheckedProperty.AddOwner<SettingsSwitchItem>();

    public event EventHandler<RoutedEventArgs>? IsCheckedChanged
    {
        add => AddHandler(IsCheckedChangedEvent, value);
        remove => RemoveHandler(IsCheckedChangedEvent, value);
    }

    private void OnLanguageUpdated()
    {
        _toggle.OnContent = Strings.On;
        _toggle.OffContent = Strings.Off;
    }

    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        IsChecked = !IsChecked;
        RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == IsCheckedProperty)
        {
            _toggle.IsChecked = IsChecked;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}

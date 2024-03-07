using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using GalaxyBudsClient.Interface.Converters;
using GalaxyBudsClient.Interface.MarkupExtensions;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsComboBoxItem : SettingsSymbolItem
{
    public SettingsComboBoxItem()
    {
        _comboBox = new ComboBox
        {
            MinWidth = 150,
            DisplayMemberBinding = new Binding(".")
            {
                Converter = new EnumToDescriptionConverter()
            }
        };
        _comboBox.SelectionChanged += OnSelectionChanged;
        IsClickEnabled = false;
        Footer = _comboBox;
    }
    
    private readonly ComboBox _comboBox;
    
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SettingsComboBoxItem, SelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);
    
    public static readonly StyledProperty<object?> SelectedValueProperty = 
        SelectingItemsControl.SelectedValueProperty.AddOwner<SettingsComboBoxItem>();
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = 
        ItemsControl.ItemsSourceProperty.AddOwner<SettingsComboBoxItem>();

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }
    
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedValue = _comboBox.SelectedValue;
        RaiseEvent(e);
    }
 
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ItemsSourceProperty)
        {
            _comboBox.ItemsSource = ItemsSource;
        }
        else if (change.Property == SelectedValueProperty)
        {
            _comboBox.SelectedValue = SelectedValue;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
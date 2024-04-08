using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using FluentAvalonia.UI.Controls;
using GalaxyBudsClient.Interface.Converters;

namespace GalaxyBudsClient.Interface.Controls;


public class SettingsComboBoxItem : SettingsSymbolItem
{
    public SettingsComboBoxItem()
    {
        _comboBox = new FAComboBox
        {
            MinWidth = 150,
            DisplayMemberBinding = new Binding(".")
            { 
                Converter = new EnumToDescriptionConverter()
            }
        };
        _comboBox.SelectionChanged += OnSelectionChanged;

        _toolButton = new Button
        {
            IsVisible = false,
            IsEnabled = false,
            MaxHeight = 32
        };
        _toolButton.Click += OnToolButtonClick;
        
        IsClickEnabled = false;
        Footer = new StackPanel
        {
            Spacing = 4,
            Orientation = Orientation.Horizontal,
            Children = { _comboBox, _toolButton }
        };
    }

    private readonly Button _toolButton;
    private readonly FAComboBox _comboBox;
    
    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<SettingsComboBoxItem, SelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);
    
    public static readonly RoutedEvent<RoutedEventArgs> ToolButtonClickEvent =
        RoutedEvent.Register<SettingsComboBoxItem, RoutedEventArgs>(nameof(ToolButtonClick), RoutingStrategies.Bubble);
    
    public static readonly StyledProperty<object?> SelectedValueProperty = 
        SelectingItemsControl.SelectedValueProperty.AddOwner<SettingsComboBoxItem>();
    
    public static readonly StyledProperty<IBinding?> DisplayMemberBindingProperty = 
        ItemsControl.DisplayMemberBindingProperty.AddOwner<SettingsComboBoxItem>();

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = 
        ItemsControl.ItemsSourceProperty.AddOwner<SettingsComboBoxItem>();

    public static readonly StyledProperty<bool> IsToolButtonVisibleProperty =
        AvaloniaProperty.Register<SettingsComboBoxItem, bool>(nameof(IsToolButtonVisible));
    
    public static readonly StyledProperty<bool> IsToolButtonEnabledProperty =
        AvaloniaProperty.Register<SettingsComboBoxItem, bool>(nameof(IsToolButtonEnabled));
    
    public static readonly StyledProperty<object?> ToolButtonContentProperty =
        AvaloniaProperty.Register<SettingsComboBoxItem, object?>(nameof(ToolButtonContent));
    
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    } 
    
    public event EventHandler<RoutedEventArgs>? ToolButtonClick
    {
        add => AddHandler(ToolButtonClickEvent, value);
        remove => RemoveHandler(ToolButtonClickEvent, value);
    }

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }
    
    [AssignBinding]
    [InheritDataTypeFromItems(nameof(ItemsSource))]
    public IBinding? DisplayMemberBinding
    {
        get => GetValue(DisplayMemberBindingProperty);
        set => SetValue(DisplayMemberBindingProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }    
    
    public bool IsToolButtonVisible
    {
        get => GetValue(IsToolButtonVisibleProperty);
        set => SetValue(IsToolButtonVisibleProperty, value);
    }

    public bool IsToolButtonEnabled
    {
        get => GetValue(IsToolButtonEnabledProperty);
        set => SetValue(IsToolButtonEnabledProperty, value);
    }

    public object? ToolButtonContent
    {
        get => GetValue(ToolButtonContentProperty);
        set => SetValue(ToolButtonContentProperty, value);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedValue = _comboBox.SelectedValue;
        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
    }
 
    private void OnToolButtonClick(object? sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ToolButtonClickEvent));
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
        else if (change.Property == DisplayMemberBindingProperty)
        {
            _comboBox.DisplayMemberBinding = DisplayMemberBinding;
        }
        else if (change.Property == ToolButtonContentProperty)
        {
            _toolButton.Content = ToolButtonContent;
        }
        else if (change.Property == IsToolButtonVisibleProperty)
        {
            _toolButton.IsVisible = IsToolButtonVisible;
        }
        else if (change.Property == IsToolButtonEnabledProperty)
        {
            _toolButton.IsEnabled = IsToolButtonEnabled;
        }
        else
        {
            base.OnPropertyChanged(change);
        }
    }
}
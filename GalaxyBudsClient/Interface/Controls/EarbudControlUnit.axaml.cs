using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GalaxyBudsClient.Interface.Controls
{
    public partial class EarbudControlUnit : UserControl
    {
        public EarbudControlUnit()
        {
            InitializeComponent();
            WarningContainer.IsVisible = false;
            ToggleButton.IsCheckedChanged += (_, _) => IsChecked = ToggleButton.IsChecked;
        }

        public static readonly RoutedEvent<RoutedEventArgs> IsCheckedChangedEvent = 
            RoutedEvent.Register<EarbudControlUnit, RoutedEventArgs>(nameof(IsCheckedChanged), RoutingStrategies.Bubble);

        public static readonly StyledProperty<bool?> IsCheckedProperty = 
            GlowingToggleButton.IsCheckedProperty.AddOwner<EarbudControlUnit>();

        public static readonly StyledProperty<string?> ButtonTextProperty =
            AvaloniaProperty.Register<EarbudControlUnit, string?>(nameof(ButtonText));
        
        public static readonly StyledProperty<string?> WarningTextProperty =
            AvaloniaProperty.Register<EarbudControlUnit, string?>(nameof(WarningText));
    
        public static readonly StyledProperty<object?> LeftContentProperty =
            AvaloniaProperty.Register<EarbudControlUnit, object?>(nameof(LeftContent));
    
        public static readonly StyledProperty<object?> RightContentProperty =
            AvaloniaProperty.Register<EarbudControlUnit, object?>(nameof(RightContent));
    
        public event EventHandler<RoutedEventArgs>? IsCheckedChanged
        {
            add => AddHandler(IsCheckedChangedEvent, value);
            remove => RemoveHandler(IsCheckedChangedEvent, value);
        }

        public bool? IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        
        public string? ButtonText
        {
            get => GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }
        
        public string? WarningText
        {
            get => GetValue(WarningTextProperty);
            set => SetValue(WarningTextProperty, value);
        }
        
        public object? LeftContent
        {
            get => GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }
        
        public object? RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsCheckedProperty)
            {
                ToggleButton.IsChecked = IsChecked;
                RaiseEvent(new RoutedEventArgs(IsCheckedChangedEvent));
            }
            else if (change.Property == ButtonTextProperty)
            {
                ToggleButton.Text = ButtonText;
            }
            else if (change.Property == WarningTextProperty)
            {
                WarningContainer.IsVisible = !string.IsNullOrEmpty(WarningText);
                WarningLabel.Content = WarningText;
            }
            else if (change.Property == LeftContentProperty)
            {
                LeftContentControl.Content = LeftContent;
            }
            else if (change.Property == RightContentProperty)
            {
                RightContentControl.Content = RightContent;
            }
            else
            {
                base.OnPropertyChanged(change);
            }
        }
    }
}

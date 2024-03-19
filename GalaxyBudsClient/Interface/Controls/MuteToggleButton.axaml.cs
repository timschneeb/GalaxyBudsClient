using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using FluentIcons.Common;

namespace GalaxyBudsClient.Interface.Controls
{
    public partial class MuteToggleButton : UserControl
    {
        public MuteToggleButton()
        {
            InitializeComponent();
            
            Button.DataContext = this;
            Button.IsCheckedChanged += (_, _) => IsChecked = Button.IsChecked;
        }
        
        public static readonly StyledProperty<bool?> IsCheckedProperty = 
            ToggleButton.IsCheckedProperty.AddOwner<MuteToggleButton>();

        public bool? IsChecked
        {
            get => GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsCheckedProperty)
            {
                Button.IsChecked = IsChecked;
                Icon.Symbol = IsChecked == true ? Symbol.SpeakerOff : Symbol.Speaker2;
            }
            else
            {
                base.OnPropertyChanged(change);
            }
        }
    }
}

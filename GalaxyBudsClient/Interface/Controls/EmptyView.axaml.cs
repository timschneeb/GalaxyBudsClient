using Avalonia;
using Avalonia.Controls;
using FluentIcons.Common;
using FluentIcons.Avalonia.Fluent;

namespace GalaxyBudsClient.Interface.Controls
{
    public partial class EmptyView : UserControl
    {
        public EmptyView()
        {
            InitializeComponent();
        }
        
        public static readonly StyledProperty<Symbol> SymbolProperty = 
            SymbolIcon.SymbolProperty.AddOwner<EmptyView>();
        
        public static readonly StyledProperty<string?> TextProperty = 
            TextBlock.TextProperty.AddOwner<EmptyView>();

        public Symbol Symbol
        {
            get => GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }
        
        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == SymbolProperty)
            {
                SymbolIcon.Symbol = Symbol;
            }
            else if (change.Property == TextProperty)
            {
                TextBlock.Text = Text;
            }
            else
            {
                base.OnPropertyChanged(change);
            }
            base.OnPropertyChanged(change);
        }
    }
}

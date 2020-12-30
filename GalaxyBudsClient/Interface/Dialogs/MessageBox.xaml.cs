using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class MessageBox : Window
    {
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<MessageBox, string>(nameof(Description));

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        
        public MessageBox()
        {
            DataContext = this;
            AvaloniaXamlLoader.Load(this);
        }
        
        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
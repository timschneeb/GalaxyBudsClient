using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class QuestionBox : Window
    {
        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<QuestionBox, string>(nameof(Description));

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
        
        public QuestionBox()
        {
            DataContext = this;
            AvaloniaXamlLoader.Load(this);
        }
        
        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
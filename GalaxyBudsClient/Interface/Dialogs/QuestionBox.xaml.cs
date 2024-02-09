using System.Threading.Tasks;
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

        public new async Task<TResult> ShowDialog<TResult>(Window owner)
        {
            return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(300);
                return await base.ShowDialog<TResult>(owner);
            });
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
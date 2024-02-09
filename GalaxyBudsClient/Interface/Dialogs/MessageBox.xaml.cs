using System.Threading.Tasks;
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

        public new Task ShowDialog(Window owner)
        {
            return Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(300);
                await base.ShowDialog(owner);
            });
        }

        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class InputDialog : Window
    {
        private readonly TextBox _input;
        
        public InputDialog()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            _input = this.FindControl<TextBox>("Input");
        }
        
        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            this.Close(null);
        }

        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            this.Close(_input.Text);
        }
    }
}
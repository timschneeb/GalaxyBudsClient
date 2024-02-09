using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public sealed class InputDialog : Window
    {
        public readonly TextBox Input;
        
        public InputDialog()
        {
            AvaloniaXamlLoader.Load(this);
            this.AttachDevTools();

            Input = this.FindControl<TextBox>("Input");
        }

        public new async Task<TResult> ShowDialog<TResult>(Window owner)
        {
            return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(300);
                return await base.ShowDialog<TResult>(owner);
            });
        }
        
        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            this.Close(null);
        }

        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            this.Close(Input.Text);
        }
    }
}
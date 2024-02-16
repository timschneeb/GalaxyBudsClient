using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Utils;
using Key = Avalonia.Input.Key;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public class HotkeyRecorder : Window
    {
        private readonly TextBlock _keyLabel;
        private bool _recording;
        private bool _resultOnce;

        public bool Result { set; get; } = false; 
        
        public HotkeyRecorder()
        {
            AvaloniaXamlLoader.Load(this);

            _keyLabel = this.GetControl<TextBlock>("KeyString");
        }
        
        public List<Key>? Hotkeys { get; private set; }
        
        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            _recording = false;
            e.Handled = true;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (!_recording || Hotkeys == null)
            {
                Hotkeys = null;
                Hotkeys = new List<Key>();
            }

            if (Hotkeys.Contains(e.Key))
                return;

            Hotkeys.Add(e.Key);

            _recording = true;

            _keyLabel.Text = Hotkeys.AsAvaloniaHotkeyString();
            e.Handled = true;
        }

        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            Result = false;
            _resultOnce = true;
            Close();
        }

        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            Result = true;
            _resultOnce = true;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_resultOnce)
            {
                Result = false;
            }
            base.OnClosed(e);
        }

        public new async Task ShowDialog(Window owner)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(300);
                await base.ShowDialog(owner);
            });
        }
    }
}

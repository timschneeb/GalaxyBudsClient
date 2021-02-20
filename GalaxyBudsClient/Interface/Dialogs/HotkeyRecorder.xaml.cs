using System;
using System.Collections.Generic;
using System.Text;
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
        private bool _result_once;

        public bool Result { set; get; } = false; 
        
        public HotkeyRecorder()
        {
            AvaloniaXamlLoader.Load(this);

            _keyLabel = this.FindControl<TextBlock>("KeyString");
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
            _result_once = true;
            Close();
        }

        private void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            Result = true;
            _result_once = true;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_result_once)
            {
                Result = false;
            }
            base.OnClosed(e);
        }
    }
}

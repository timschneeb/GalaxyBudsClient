using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Utils;
using GalaxyBudsClient.Utils.DynamicLocalization;
using Key = Avalonia.Input.Key;
using KeyEventArgs = Avalonia.Input.KeyEventArgs;

namespace GalaxyBudsClient.Interface.Dialogs
{
    public class HotkeyActionBuilder : Window
    {
        private class ViewModel
        {
            public IEnumerable KeySource => Enum.GetValues(typeof(Keys)).Cast<Keys>().ToList();
            public IEnumerable ActionSource
            {
                get
                {
                    var actions = new List<ActionViewHolder>();
                    Enum.GetValues(typeof(EventDispatcher.Event))
                        .Cast<EventDispatcher.Event>()
                        .ToList()
                        .ForEach(x => actions.Add(new ActionViewHolder(x.GetDescription(), x)));
                    return actions;
                }
            }
        }
        
        private readonly ViewModel _vm = new ViewModel();

        private readonly TextBlock _keyLabel;
        private readonly ComboBox _key1;
        private readonly ComboBox _key2;
        private readonly ComboBox _action;
        private readonly CheckBox _modCtrl;
        private readonly CheckBox _modAlt;
        private readonly CheckBox _modShift;
        private readonly CheckBox _modWin;

        private bool _result_once;

        public bool Result { set; get; } = false; 
        public IEnumerable<ModifierKeys>? SelectedModifierKeys { set; get; }
        public IEnumerable<Keys>? SelectedKeys { set; get; }

        public HotkeyActionBuilder()
        {
            DataContext = _vm;
            AvaloniaXamlLoader.Load(this);

            _keyLabel = this.FindControl<TextBlock>("KeyString");
            _key1 = this.FindControl<ComboBox>("Key1");
            _key2 = this.FindControl<ComboBox>("Key2");
            _action = this.FindControl<ComboBox>("Action");
            _modCtrl = this.FindControl<CheckBox>("ModCtrl");
            _modAlt = this.FindControl<CheckBox>("ModAlt");
            _modShift = this.FindControl<CheckBox>("ModShift");
            _modWin = this.FindControl<CheckBox>("ModWin");

            _key1.SelectedItem = Keys.None;
            _key2.SelectedItem = Keys.None;
            _action.SelectedIndex = -1;
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

        private void UpdateKeyCombo()
        {
            if ((_key1.SelectedItem == null || (Keys)_key1.SelectedItem == Keys.None) && 
                (_key2.SelectedItem == null || (Keys)_key2.SelectedItem == Keys.None))
            {
                _keyLabel.Text = Loc.Resolve("hotkey_edit_invalid");
                return;
            }

            var modifier = new List<ModifierKeys>();
            var keys = new List<Keys>();
            if (_key1.SelectedItem != null && (Keys)_key1.SelectedItem != Keys.None)
            {
                keys.Add((Keys)_key1.SelectedItem);
            }
            if (_key2.SelectedItem != null && (Keys)_key2.SelectedItem != Keys.None)
            {
                keys.Add((Keys)_key2.SelectedItem);
            }
            
            if (_modCtrl.IsChecked == true)
            {
                modifier.Add(ModifierKeys.Control);
            }
            if (_modAlt.IsChecked == true)
            {
                modifier.Add(ModifierKeys.Alt);
            } 
            if (_modWin.IsChecked == true)
            {
                modifier.Add(ModifierKeys.Win);
            }
            if (_modShift.IsChecked == true)
            {
                modifier.Add(ModifierKeys.Shift);
            }

            _keyLabel.Text = keys.AsHotkeyString(modifier);
        }
        
        private void OnModifierChanged(object? sender, RoutedEventArgs e)
        {
            UpdateKeyCombo();
        }

        private void OnKeySelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateKeyCombo();
        }

        private void Action_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {

        }
    }
}

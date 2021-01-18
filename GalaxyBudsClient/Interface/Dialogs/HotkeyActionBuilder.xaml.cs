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
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
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
                        .Where(EventDispatcher.CheckDeviceSupport)
                        .ToList()
                        .ForEach(x => actions.Add(new ActionViewHolder(x)));
                    return actions;
                }
            }
        }
        
        private readonly ViewModel _vm = new ViewModel();

        private readonly TextBlock _keyLabel;
        private readonly ComboBox _key1;
        private readonly ComboBox _action;
        private readonly CheckBox _modCtrl;
        private readonly CheckBox _modAlt;
        private readonly CheckBox _modShift;
        private readonly CheckBox _modWin;

        private bool _result_once;

        public bool Result { private set; get; } = false; 
        public Hotkey? Hotkey { set; get; }
        
        public HotkeyActionBuilder() : this(null)
        {
        }
        
        public HotkeyActionBuilder(Hotkey? hotkey = null)
        {
            DataContext = _vm;
            AvaloniaXamlLoader.Load(this);

            Hotkey = hotkey;
            
            _keyLabel = this.FindControl<TextBlock>("KeyString");
            _key1 = this.FindControl<ComboBox>("Key1");
            _action = this.FindControl<ComboBox>("Action");
            _modCtrl = this.FindControl<CheckBox>("ModCtrl");
            _modAlt = this.FindControl<CheckBox>("ModAlt");
            _modShift = this.FindControl<CheckBox>("ModShift");
            _modWin = this.FindControl<CheckBox>("ModWin");

            _key1.SelectedItem = Keys.None;
            _action.SelectedItem = _action.Items
                .Cast<ActionViewHolder>()
                .FirstOrDefault(x => x.Value == EventDispatcher.Event.None);

            if (Hotkey != null)
            {
                if (Hotkey.Keys.ToList().Count >= 1)
                {
                    _key1.SelectedItem = Hotkey.Keys.ToArray()[0];
                }

                _modCtrl.IsChecked = Hotkey.Modifier.ToList().Contains(ModifierKeys.Control);
                _modAlt.IsChecked = Hotkey.Modifier.ToList().Contains(ModifierKeys.Alt);
                _modShift.IsChecked = Hotkey.Modifier.ToList().Contains(ModifierKeys.Shift);
                _modWin.IsChecked = Hotkey.Modifier.ToList().Contains(ModifierKeys.Win);

                _action.SelectedItem = _action.Items
                    .Cast<ActionViewHolder>()
                    .FirstOrDefault(x => x.Value == Hotkey.Action);
            }
        }
        
        private void Cancel_OnClick(object? sender, RoutedEventArgs e)
        {
            Hotkey = null;
            Result = false;
            _result_once = true;
            Close();
        }

        private async void Apply_OnClick(object? sender, RoutedEventArgs e)
        {
            if ((_key1.SelectedItem == null || (Keys)_key1.SelectedItem == Keys.None) || Hotkey == null)
            {
                await new MessageBox()
                {
                    Title = Loc.Resolve("hotkey_edit_invalid"),
                    Description = Loc.Resolve("hotkey_edit_invalid_desc"),
                    Topmost = true,
                }.ShowDialog(this);
                return;
            }

            if ((_action.SelectedItem as ActionViewHolder) == null ||
                (_action.SelectedItem as ActionViewHolder)?.Value == EventDispatcher.Event.None)
            {
                await new MessageBox()
                {
                    Title = Loc.Resolve("hotkey_edit_invalid_action"),
                    Description = Loc.Resolve("hotkey_edit_invalid_action_desc"),
                    Topmost = true,
                }.ShowDialog(this);
                return;
            }

            var result = await HotkeyReceiverImpl.Instance.ValidateHotkeyAsync(Hotkey);
            if (result != null)
            {
                await new MessageBox()
                {
                    Title = Loc.Resolve("hotkey_add_error"),
                    Description = result.Message,
                    Topmost = true
                }.ShowDialog(MainWindow.Instance);
                return;
            }
            
            Result = true;
            _result_once = true;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (!_result_once)
            {
                Hotkey = null;
                Result = false;
            }
            base.OnClosed(e);
        }

        private void UpdateKeyCombo()
        {
            if (_key1.SelectedItem == null || (Keys)_key1.SelectedItem == Keys.None)
            {
                _keyLabel.Text = Loc.Resolve("hotkey_edit_invalid");
                return;
            }
            if ((_action.SelectedItem as ActionViewHolder) == null ||
                (_action.SelectedItem as ActionViewHolder)?.Value == EventDispatcher.Event.None)
            {
                _keyLabel.Text = Loc.Resolve("hotkey_edit_invalid_action");
                return;
            }
            
            var modifier = new List<ModifierKeys>();
            var keys = new List<Keys>();
            if (_key1.SelectedItem != null && (Keys)_key1.SelectedItem != Keys.None)
            {
                keys.Add((Keys)_key1.SelectedItem);
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

            if (modifier.Count < 1)
            {
                _keyLabel.Text = Loc.Resolve("hotkey_edit_invalid_mod");
                return;
            }

            if (_action.SelectedItem is ActionViewHolder viewHolder)
            {
                Hotkey = new Hotkey(modifier, keys, viewHolder.Value);
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
            UpdateKeyCombo();
        }
    }
}

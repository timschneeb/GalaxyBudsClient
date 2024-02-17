using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
using SharpHook;
using SharpHook.Native;
#pragma warning disable CS0618 // Type or member is obsolete

namespace GalaxyBudsClient.Platform.Linux
{
    public class HotkeyReceiver : IHotkeyReceiver
    {
        private readonly List<Hotkey> _hotkeys = new();
        private readonly Dictionary<ushort, KeyCode> _pressedKeys = new();
        
        private readonly Task? _hookTask;
        private readonly TaskPoolGlobalHook _hook;

        public HotkeyReceiver()
        {
            _hook = new TaskPoolGlobalHook(new TaskPoolGlobalHookOptions(2, true));
            _hook.KeyPressed += OnKeyPressed;
            _hook.KeyReleased += OnKeyReleased;
            
            try
            {
                _hookTask = _hook.RunAsync();
            }
            catch (Exception ex)
            {
                _hookTask = null;
                Log.Error(ex, "Linux.HotkeyReceiver: Failed to start hook");
            }
        }

        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            _pressedKeys.Remove(e.Data.RawCode);
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            _pressedKeys.TryAdd(e.Data.RawCode, e.Data.KeyCode);
            OnKeyComboPressed(_pressedKeys.Values);
            Log.Debug(
                "HotkeyReceiver.Linux: Key pressed: {Keys}", 
                string.Join('+', _pressedKeys.Select(x => x.Value.ToString().Replace("Vc", "")))
            );
        }
        
        private void OnKeyComboPressed(IEnumerable<KeyCode> keyCodes)
        {
            List<ModifierKeys> modifierKeys = new();
            List<Keys> keys = new();
            foreach (var keyCode in keyCodes)
            {
                var key = TranslateKey(keyCode);
                switch (key)
                {
                    case null:
                        continue;
                    case Keys.LShiftKey or Keys.RShiftKey:
                        modifierKeys.Add(ModifierKeys.Shift);
                        break;
                    case Keys.LControlKey or Keys.RControlKey:
                        modifierKeys.Add(ModifierKeys.Control);
                        break;
                    case Keys.LMenu or Keys.RMenu:
                        modifierKeys.Add(ModifierKeys.Alt);
                        break;
                    case Keys.LWin or Keys.RWin:
                        modifierKeys.Add(ModifierKeys.Win);
                        break;
                    default:
                        keys.Add(key.Value);
                        break;
                }
            }
            
            foreach (var hotkey in _hotkeys.Where(hotkey => hotkey.Keys.SequenceEqual(keys) && 
                                                            hotkey.Modifier.SequenceEqual(modifierKeys)))
            {
                EventDispatcher.Instance.Dispatch(hotkey.Action);
            }
        }
        
        public async Task RegisterHotkeyAsync(Hotkey hotkey)
        {
            _hotkeys.Add(hotkey);
            Log.Debug("Hotkey successfully registered ({Name})", hotkey.HotkeyName);
            await Task.CompletedTask;
        }
        
        public async Task ValidateHotkeyAsync(Hotkey hotkey)
        {
            await Task.CompletedTask;
        }

        public async Task UnregisterAllAsync()
        {
            _hotkeys.Clear();
            Log.Debug("Linux.HotkeyReceiver: All hotkeys unregistered");
            await Task.CompletedTask;
        }

        public async void Dispose()
        {
            await UnregisterAllAsync();
            _hookTask?.Dispose();
            _hook.Dispose();
            GC.SuppressFinalize(this);
        }
        
        private static Keys? TranslateKey(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.Vc0 => Keys.Decimal0,
                KeyCode.Vc1 => Keys.Decimal1,
                KeyCode.Vc2 => Keys.Decimal2,
                KeyCode.Vc3 => Keys.Decimal3,
                KeyCode.Vc4 => Keys.Decimal4,
                KeyCode.Vc5 => Keys.Decimal5,
                KeyCode.Vc6 => Keys.Decimal6,
                KeyCode.Vc7 => Keys.Decimal7,
                KeyCode.Vc8 => Keys.Decimal8,
                KeyCode.Vc9 => Keys.Decimal9,
                KeyCode.VcA => Keys.A,
                KeyCode.VcB => Keys.B,
                KeyCode.VcC => Keys.C,
                KeyCode.VcD => Keys.D,
                KeyCode.VcE => Keys.E,
                KeyCode.VcF => Keys.F,
                KeyCode.VcG => Keys.G,
                KeyCode.VcH => Keys.H,
                KeyCode.VcI => Keys.I,
                KeyCode.VcJ => Keys.J,
                KeyCode.VcK => Keys.K,
                KeyCode.VcL => Keys.L,
                KeyCode.VcM => Keys.M,
                KeyCode.VcN => Keys.N,
                KeyCode.VcO => Keys.O,
                KeyCode.VcP => Keys.P,
                KeyCode.VcQ => Keys.Q,
                KeyCode.VcR => Keys.R,
                KeyCode.VcS => Keys.S,
                KeyCode.VcT => Keys.T,
                KeyCode.VcU => Keys.U,
                KeyCode.VcV => Keys.V,
                KeyCode.VcW => Keys.W,
                KeyCode.VcX => Keys.X,
                KeyCode.VcY => Keys.Y,
                KeyCode.VcZ => Keys.Z,
                KeyCode.VcUndefined => null,
                KeyCode.VcEscape => Keys.Escape,
                KeyCode.VcF1 => Keys.F1,
                KeyCode.VcF2 => Keys.F2,
                KeyCode.VcF3 => Keys.F3,
                KeyCode.VcF4 => Keys.F4,
                KeyCode.VcF5 => Keys.F5,
                KeyCode.VcF6 => Keys.F6,
                KeyCode.VcF7 => Keys.F7,
                KeyCode.VcF8 => Keys.F8,
                KeyCode.VcF9 => Keys.F9,
                KeyCode.VcF10 => Keys.F10,
                KeyCode.VcF11 => Keys.F11,
                KeyCode.VcF12 => Keys.F12,
                KeyCode.VcF13 => Keys.F13,
                KeyCode.VcF14 => Keys.F14,
                KeyCode.VcF15 => Keys.F15,
                KeyCode.VcF16 => Keys.F16,
                KeyCode.VcF17 => Keys.F17,
                KeyCode.VcF18 => Keys.F18,
                KeyCode.VcF19 => Keys.F19,
                KeyCode.VcF20 => Keys.F20,
                KeyCode.VcF21 => Keys.F21,
                KeyCode.VcF22 => Keys.F22,
                KeyCode.VcF23 => Keys.F23,
                KeyCode.VcF24 => Keys.F24,
                KeyCode.VcBackQuote => Keys.OemQuotes,
                KeyCode.VcMinus => Keys.OemMinus,
                KeyCode.VcEquals => null,
                KeyCode.VcBackspace => Keys.Backspace,
                KeyCode.VcTab => Keys.Tab,
                KeyCode.VcCapsLock => Keys.CapsLock,
                KeyCode.VcOpenBracket => Keys.OemOpenBrackets,
                KeyCode.VcCloseBracket => Keys.OemCloseBrackets,
                KeyCode.VcBackslash => Keys.OemBackslash,
                KeyCode.VcSemicolon => Keys.OemSemicolon,
                KeyCode.VcQuote => Keys.OemQuotes,
                KeyCode.VcEnter => Keys.Enter,
                KeyCode.VcComma => Keys.OemComma,
                KeyCode.VcPeriod => Keys.OemPeriod,
                KeyCode.VcSlash => Keys.Separator,
                KeyCode.VcSpace => Keys.Space,
                KeyCode.Vc102 => null,
                KeyCode.VcMisc => null,
                KeyCode.VcPrintScreen => Keys.PrintScreen,
                KeyCode.VcPrint => Keys.Print,
                KeyCode.VcSelect => Keys.Select,
                KeyCode.VcExecute => Keys.Execute,
                KeyCode.VcScrollLock => Keys.ScrollLock,
                KeyCode.VcPause => Keys.Pause,
                KeyCode.VcCancel => Keys.CancelKey,
                KeyCode.VcHelp => Keys.Help,
                KeyCode.VcInsert => Keys.Insert,
                KeyCode.VcDelete => Keys.Delete,
                KeyCode.VcHome => Keys.Home,
                KeyCode.VcEnd => Keys.End,
                KeyCode.VcPageUp => Keys.PageUp,
                KeyCode.VcPageDown => Keys.PageDown,
                KeyCode.VcUp => Keys.Up,
                KeyCode.VcLeft => Keys.Left,
                KeyCode.VcRight => Keys.Right,
                KeyCode.VcDown => Keys.Down,
                KeyCode.VcNumLock => Keys.NumLock,
                KeyCode.VcNumPadClear => Keys.Clear,
                KeyCode.VcNumPadDivide => Keys.Divide,
                KeyCode.VcNumPadMultiply => Keys.Multiply,
                KeyCode.VcNumPadSubtract => Keys.Subtract,
                KeyCode.VcNumPadEquals => null,
                KeyCode.VcNumPadAdd => Keys.Add,
                KeyCode.VcNumPadEnter => Keys.Enter,
                KeyCode.VcNumPadDecimal => Keys.Decimal,
                KeyCode.VcNumPadSeparator => Keys.Separator,
                KeyCode.VcNumPad0 => Keys.NumPad0,
                KeyCode.VcNumPad1 => Keys.NumPad1,
                KeyCode.VcNumPad2 => Keys.NumPad2,
                KeyCode.VcNumPad3 => Keys.NumPad3,
                KeyCode.VcNumPad4 => Keys.NumPad4,
                KeyCode.VcNumPad5 => Keys.NumPad5,
                KeyCode.VcNumPad6 => Keys.NumPad6,
                KeyCode.VcNumPad7 => Keys.NumPad7,
                KeyCode.VcNumPad8 => Keys.NumPad8,
                KeyCode.VcNumPad9 => Keys.NumPad9,
                KeyCode.VcLeftShift => Keys.LShiftKey,
                KeyCode.VcRightShift => Keys.RShiftKey,
                KeyCode.VcLeftControl => Keys.LControlKey,
                KeyCode.VcRightControl => Keys.RControlKey,
                KeyCode.VcLeftAlt => Keys.LMenu,
                KeyCode.VcRightAlt => Keys.RMenu,
                KeyCode.VcLeftMeta => Keys.LWin,
                KeyCode.VcRightMeta => Keys.RWin,
                KeyCode.VcContextMenu => Keys.Menu,
                KeyCode.VcFunction => null,
                KeyCode.VcChangeInputSource => null,
                KeyCode.VcPower => null,
                KeyCode.VcSleep => Keys.Sleep,
                KeyCode.VcWake => null,
                KeyCode.VcMedia => null,
                KeyCode.VcMediaPlay => Keys.Play,
                KeyCode.VcMediaStop => Keys.MediaStop,
                KeyCode.VcMediaPrevious => Keys.MediaPreviousTrack,
                KeyCode.VcMediaNext => Keys.MediaNextTrack,
                KeyCode.VcMediaSelect => Keys.SelectMedia,
                KeyCode.VcMediaEject => null,
                KeyCode.VcVolumeMute => Keys.VolumeMute,
                KeyCode.VcVolumeDown => Keys.VolumeDown,
                KeyCode.VcVolumeUp => Keys.VolumeUp,
                KeyCode.VcPlay => Keys.Play,
                KeyCode.VcZoom => Keys.Zoom,
                KeyCode.VcApp1 => Keys.LaunchApplication1,
                KeyCode.VcApp2 => Keys.LaunchApplication2,
                KeyCode.VcAppBrowser => null,
                KeyCode.VcAppCalculator => null,
                KeyCode.VcAppMail => null,
                KeyCode.VcBrowserSearch => Keys.BrowserSearch,
                KeyCode.VcBrowserHome => Keys.BrowserHome,
                KeyCode.VcBrowserBack => Keys.BrowserBack,
                KeyCode.VcBrowserForward => Keys.BrowserForward,
                KeyCode.VcBrowserStop => Keys.BrowserStop,
                KeyCode.VcBrowserRefresh => Keys.BrowserRefresh,
                KeyCode.VcBrowserFavorites => Keys.BrowserFavorites,
                KeyCode.VcKatakanaHiragana => null,
                KeyCode.VcKatakana => null,
                KeyCode.VcHiragana => null,
                KeyCode.VcKana => Keys.KanaMode,
                KeyCode.VcKanji => Keys.KanjiMode,
                KeyCode.VcHangul => null,
                KeyCode.VcJunja => Keys.JunjaMode,
                KeyCode.VcFinal => Keys.FinalMode,
                KeyCode.VcHanja => null,
                KeyCode.VcAccept => Keys.IMEAccept,
                KeyCode.VcConvert => Keys.IMEConvert,
                KeyCode.VcNonConvert => Keys.IMENonconvert,
                KeyCode.VcImeOn => null,
                KeyCode.VcImeOff => null,
                KeyCode.VcModeChange => Keys.IMEModeChange,
                KeyCode.VcProcess => null,
                KeyCode.VcAlphanumeric => null,
                KeyCode.VcUnderscore => null,
                KeyCode.VcYen => null,
                KeyCode.VcJpComma => Keys.OemComma,
                _ => null
            };
        }
    
    }
}
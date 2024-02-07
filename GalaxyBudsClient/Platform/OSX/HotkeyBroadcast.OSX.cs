using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;
#if OSX
using ThePBone.OSX.Native.Unmanaged;
#endif

namespace GalaxyBudsClient.Platform.OSX
{
    public class HotkeyBroadcast : IHotkeyBroadcast
    {
#if OSX
        private readonly unsafe void* _hotkeySimObjc;

        public HotkeyBroadcast()
        {
            unsafe
            {
                fixed (void* ptr = &_hotkeySimObjc)
                {
                    AppUtils.allocHotkeySim(ptr);
                }

                if (_hotkeySimObjc == null)
                {
                    Log.Error("OSX.HotkeyBroadcast: Failed to allocate HotkeySimImpl");
                }
            }
        }
#endif

        public void SendKeys(IEnumerable<Key> keys)
        {
            SendMacKeys(keys.Select(key =>
            {
                switch (key)
                {
                    case Key.A:
                        return Keys.A;
                    case Key.None:
                        return Keys.None;
                    case Key.Cancel:
                        return Keys.CancelKey;
                    case Key.Back:
                        return Keys.Backspace;
                    case Key.Tab:
                        return Keys.Tab;
                    case Key.LineFeed:
                        return Keys.LineFeed;
                    case Key.Clear:
                        return Keys.Clear;
                    case Key.Return:
                        return Keys.Enter;
                    case Key.Pause:
                        return Keys.Pause;
                    case Key.CapsLock:
                        return Keys.CapsLock;
                    case Key.HangulMode:
                        break;
                    case Key.JunjaMode:
                        break;
                    case Key.FinalMode:
                        break;
                    case Key.KanjiMode:
                        break;
                    case Key.Escape:
                        return Keys.Escape;
                    case Key.ImeConvert:
                        return Keys.IMEConvert;
                    case Key.ImeNonConvert:
                        return Keys.IMENonconvert;
                    case Key.ImeAccept:
                        return Keys.IMEAccept;
                    case Key.ImeModeChange:
                        return Keys.IMEModeChange;
                    case Key.Space:
                        return Keys.Space;
                    case Key.PageUp:
                        return Keys.PageUp;
                    case Key.PageDown:
                        return Keys.PageDown;
                    case Key.End:
                        return Keys.End;
                    case Key.Home:
                        return Keys.Home;
                    case Key.Left:
                        return Keys.Left;
                    case Key.Up:
                        return Keys.Up;
                    case Key.Right:
                        return Keys.Right;
                    case Key.Down:
                        return Keys.Down;
                    case Key.Select:
                        return Keys.Select;
                    case Key.Print:
                        return Keys.Print;
                    case Key.Execute:
                        return Keys.Execute;
                    case Key.Snapshot:
                        return Keys.PrintScreen;
                    case Key.Insert:
                        return Keys.Insert;
                    case Key.Delete:
                        return Keys.Delete;
                    case Key.Help:
                        return Keys.Help;
                    case Key.D0:
                        break;
                    case Key.D1:
                        break;
                    case Key.D2:
                        break;
                    case Key.D3:
                        break;
                    case Key.D4:
                        break;
                    case Key.D5:
                        break;
                    case Key.D6:
                        break;
                    case Key.D7:
                        break;
                    case Key.D8:
                        break;
                    case Key.D9:
                        break;
                    case Key.B:
                        return Keys.B;
                    case Key.C:
                        return Keys.C;
                    case Key.D:
                        return Keys.D;
                    case Key.E:
                        return Keys.E;
                    case Key.F:
                        return Keys.F;
                    case Key.G:
                        return Keys.G;
                    case Key.H:
                        return Keys.H;
                    case Key.I:
                        return Keys.I;
                    case Key.J:
                        return Keys.J;
                    case Key.K:
                        return Keys.K;
                    case Key.L:
                        return Keys.L;
                    case Key.M:
                        return Keys.M;
                    case Key.N:
                        return Keys.N;
                    case Key.O:
                        return Keys.O;
                    case Key.P:
                        return Keys.P;
                    case Key.Q:
                        return Keys.Q;
                    case Key.R:
                        return Keys.R;
                    case Key.S:
                        return Keys.S;
                    case Key.T:
                        return Keys.T;
                    case Key.U:
                        return Keys.U;
                    case Key.V:
                        return Keys.V;
                    case Key.W:
                        return Keys.W;
                    case Key.X:
                        return Keys.X;
                    case Key.Y:
                        return Keys.Y;
                    case Key.Z:
                        return Keys.Z;
                    case Key.LWin:
                        return Keys.LWin;
                    case Key.RWin:
                        return Keys.RWin;
                    case Key.Apps:
                        return Keys.Apps;
                    case Key.Sleep:
                        return Keys.Sleep;
                    case Key.NumPad0:
                        return Keys.NumPad0;
                    case Key.NumPad1:
                        return Keys.NumPad1;
                    case Key.NumPad2:
                        return Keys.NumPad2;
                    case Key.NumPad3:
                        return Keys.NumPad3;
                    case Key.NumPad4:
                        return Keys.NumPad4;
                    case Key.NumPad5:
                        return Keys.NumPad5;
                    case Key.NumPad6:
                        return Keys.NumPad6;
                    case Key.NumPad7:
                        return Keys.NumPad7;
                    case Key.NumPad8:
                        return Keys.NumPad8;
                    case Key.NumPad9:
                        return Keys.NumPad9;
                    case Key.Multiply:
                        return Keys.Multiply;
                    case Key.Add:
                        return Keys.Add;
                    case Key.Separator:
                        return Keys.Separator;
                    case Key.Subtract:
                        return Keys.Subtract;
                    case Key.Decimal:
                        return Keys.Decimal;
                    case Key.Divide:
                        return Keys.Divide;
                    case Key.F1:
                        return Keys.F1;
                    case Key.F2:
                        return Keys.F2;
                    case Key.F3:
                        return Keys.F3;
                    case Key.F4:
                        return Keys.F4;
                    case Key.F5:
                        return Keys.F5;
                    case Key.F6:
                        return Keys.F6;
                    case Key.F7:
                        return Keys.F7;
                    case Key.F8:
                        return Keys.F8;
                    case Key.F9:
                        return Keys.F9;
                    case Key.F10:
                        return Keys.F10;
                    case Key.F11:
                        return Keys.F11;
                    case Key.F12:
                        return Keys.F12;
                    case Key.F13:
                        return Keys.F13;
                    case Key.F14:
                        return Keys.F14;
                    case Key.F15:
                        return Keys.F15;
                    case Key.F16:
                        return Keys.F16;
                    case Key.F17:
                        return Keys.F17;
                    case Key.F18:
                        return Keys.F18;
                    case Key.F19:
                        return Keys.F19;
                    case Key.F20:
                        return Keys.F20;
                    case Key.F21:
                        return Keys.F21;
                    case Key.F22:
                        return Keys.F22;
                    case Key.F23:
                        return Keys.F23;
                    case Key.F24:
                        return Keys.F24;
                    case Key.NumLock:
                        return Keys.NumLock;
                    case Key.Scroll:
                        return Keys.ScrollLock;
                    case Key.LeftShift:
                        return Keys.LShiftKey;
                    case Key.RightShift:
                        return Keys.RShiftKey;
                    case Key.LeftCtrl:
                        return Keys.LControlKey;
                    case Key.RightCtrl:
                        return Keys.RControlKey;
                    case Key.LeftAlt:
                        return Keys.LMenu;
                    case Key.RightAlt:
                        return Keys.RMenu;
                    case Key.BrowserBack:
                        return Keys.BrowserBack;
                    case Key.BrowserForward:
                        return Keys.BrowserForward;
                    case Key.BrowserRefresh:
                        return Keys.BrowserRefresh;
                    case Key.BrowserStop:
                        return Keys.BrowserStop;
                    case Key.BrowserSearch:
                        return Keys.BrowserSearch;
                    case Key.BrowserFavorites:
                        return Keys.BrowserFavorites;
                    case Key.BrowserHome:
                        return Keys.BrowserHome;
                    case Key.VolumeMute:
                        return Keys.VolumeMute;
                    case Key.VolumeDown:
                        return Keys.VolumeDown;
                    case Key.VolumeUp:
                        return Keys.VolumeUp;
                    case Key.MediaNextTrack:
                        return Keys.MediaNextTrack;
                    case Key.MediaPreviousTrack:
                        return Keys.MediaPreviousTrack;
                    case Key.MediaStop:
                        return Keys.MediaStop;
                    case Key.MediaPlayPause:
                        return Keys.MediaPlayPause;
                    case Key.LaunchMail:
                        return Keys.LaunchMail;
                    case Key.SelectMedia:
                        return Keys.SelectMedia;
                    case Key.LaunchApplication1:
                        return Keys.LaunchApplication1;
                    case Key.LaunchApplication2:
                        return Keys.LaunchApplication2;
                    case Key.OemSemicolon:
                        return Keys.OemSemicolon;
                    case Key.OemPlus:
                        return Keys.OemPlus;
                    case Key.OemComma:
                        return Keys.OemComma;
                    case Key.OemMinus:
                        return Keys.OemMinus;
                    case Key.OemPeriod:
                        return Keys.OemPeriod;
                    case Key.OemQuestion:
                        return Keys.OemQuestion;
                    case Key.OemTilde:
                        return Keys.OemTilde;
                    case Key.AbntC1:
                        break;
                    case Key.AbntC2:
                        break;
                    case Key.OemOpenBrackets:
                        return Keys.OemOpenBrackets;
                    case Key.OemPipe:
                        return Keys.OemPipe;
                    case Key.OemCloseBrackets:
                        return Keys.OemCloseBrackets;
                    case Key.OemQuotes:
                        return Keys.OemQuotes;
                    case Key.Oem8:
                        return Keys.Oem8;
                    case Key.OemBackslash:
                        return Keys.OemBackslash;
                    case Key.ImeProcessed:
                        break;
                    case Key.System:
                        break;
                    case Key.OemAttn:
                        break;
                    case Key.OemFinish:
                        break;
                    case Key.DbeHiragana:
                        break;
                    case Key.DbeSbcsChar:
                        break;
                    case Key.DbeDbcsChar:
                        break;
                    case Key.OemBackTab:
                        break;
                    case Key.DbeNoRoman:
                        break;
                    case Key.CrSel:
                        break;
                    case Key.ExSel:
                        break;
                    case Key.EraseEof:
                        break;
                    case Key.Play:
                        return Keys.Play;
                    case Key.DbeNoCodeInput:
                        break;
                    case Key.NoName:
                        break;
                    case Key.DbeEnterDialogConversionMode:
                        break;
                    case Key.OemClear:
                        break;
                    case Key.DeadCharProcessed:
                        break;
                    case Key.FnLeftArrow:
                        break;
                    case Key.FnRightArrow:
                        break;
                    case Key.FnUpArrow:
                        break;
                    case Key.FnDownArrow:
                        break;
                }
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }).ToList());
        }

        private void SendMacKeys(List<Keys> win32Keys)
        {
            var maskCmd = false;
            var maskOpt = false;
            var maskCtrl = false;
            var maskShft = false;
            foreach (var key in win32Keys)
            {
                SimulateHotkeyInternal((uint)key, true, maskCmd, maskOpt, maskCtrl, maskShft);
                maskCmd = key == Keys.LWin || key == Keys.RWin || maskCmd;
                maskShft = key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey || maskShft;
                maskOpt = key == Keys.Menu || key == Keys.LMenu || key == Keys.RMenu || maskOpt;
                maskCtrl = key == Keys.ControlKey || key == Keys.LControlKey || key == Keys.RControlKey || maskCtrl;
            }
            win32Keys.Reverse();
            foreach (var key in win32Keys)
            {
                maskCmd = !(key == Keys.LWin || key == Keys.RWin) && maskCmd;
                maskShft = !(key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey) && maskShft;
                maskOpt = !(key == Keys.Menu || key == Keys.LMenu || key == Keys.RMenu) && maskOpt;
                maskCtrl = !(key == Keys.ControlKey || key == Keys.LControlKey || key == Keys.RControlKey) && maskCtrl;
                SimulateHotkeyInternal((uint)key, false, maskCmd, maskOpt, maskCtrl, maskShft);
            }
        }

        private void SimulateHotkeyInternal(uint keyCode, bool down, bool maskCmd, bool maskOpt, bool maskCtrl,
            bool maskShft)
        {
#if OSX
            unsafe
            {
                AppUtils.simulateHotKey(_hotkeySimObjc, keyCode, down, maskCmd, maskOpt, maskCtrl, maskShft);
            }
#endif
        }

#if OSX
        ~HotkeyBroadcast()
        {
            unsafe
            {
                AppUtils.deallocHotkeySim(_hotkeySimObjc);
            }
        }
#endif
    }
}
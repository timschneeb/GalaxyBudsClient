using System.Collections.Generic;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;
using GalaxyBudsClient.Platform.Interfaces;
using Key = Avalonia.Input.Key;

namespace GalaxyBudsClient.Platform.Windows
{
    public class HotkeyBroadcast : IHotkeyBroadcast
    {
        public void SendKeys(IEnumerable<Key> keys)
        {
            InputSimulator sim = new InputSimulator();
            var enumerable = keys.ToList();
            foreach (var key in enumerable)
            {
                sim.Keyboard.KeyDown(VirtualKeyFromKey(key));
            }
            foreach (var key in enumerable)
            {
                sim.Keyboard.KeyUp(VirtualKeyFromKey(key));
            }
        }
        
        /// <summary> 
        ///     Convert our Key enum into a Win32 VirtualKey. 
        /// </summary>
        public static VirtualKeyCode VirtualKeyFromKey(Key key) 
        {
            VirtualKeyCode virtualKey = 0;
 
            switch(key) 
            {
                case Key.Cancel: 
                    virtualKey = VirtualKeyCode.CANCEL; 
                    break;
  
                case Key.Back:
                    virtualKey = VirtualKeyCode.BACK;
                    break;
  
                case Key.Tab:
                    virtualKey = VirtualKeyCode.TAB; 
                    break; 
 
                case Key.Clear: 
                    virtualKey = VirtualKeyCode.CLEAR;
                    break;
 
                case Key.Return: 
                    virtualKey = VirtualKeyCode.RETURN;
                    break; 
  
                case Key.Pause:
                    virtualKey = VirtualKeyCode.PAUSE; 
                    break;
 
                case Key.Capital:
                    virtualKey = VirtualKeyCode.CAPITAL; 
                    break;
  
                case Key.KanaMode: 
                    virtualKey = VirtualKeyCode.KANA;
                    break; 
 
                case Key.JunjaMode:
                    virtualKey = VirtualKeyCode.JUNJA;
                    break; 
 
                case Key.FinalMode: 
                    virtualKey = VirtualKeyCode.FINAL; 
                    break;
  
                case Key.KanjiMode:
                    virtualKey = VirtualKeyCode.KANJI;
                    break;
  
                case Key.Escape:
                    virtualKey = VirtualKeyCode.ESCAPE; 
                    break; 
 
                case Key.ImeConvert: 
                    virtualKey = VirtualKeyCode.CONVERT;
                    break;
 
                case Key.ImeNonConvert: 
                    virtualKey = VirtualKeyCode.NONCONVERT;
                    break; 
  
                case Key.ImeAccept:
                    virtualKey = VirtualKeyCode.ACCEPT; 
                    break;
 
                case Key.ImeModeChange:
                    virtualKey = VirtualKeyCode.MODECHANGE; 
                    break;
  
                case Key.Space: 
                    virtualKey = VirtualKeyCode.SPACE;
                    break; 
 
                case Key.Prior:
                    virtualKey = VirtualKeyCode.PRIOR;
                    break; 
 
                case Key.Next: 
                    virtualKey = VirtualKeyCode.NEXT; 
                    break;
  
                case Key.End:
                    virtualKey = VirtualKeyCode.END;
                    break;
  
                case Key.Home:
                    virtualKey = VirtualKeyCode.HOME; 
                    break; 
 
                case Key.Left: 
                    virtualKey = VirtualKeyCode.LEFT;
                    break;
 
                case Key.Up: 
                    virtualKey = VirtualKeyCode.UP;
                    break; 
  
                case Key.Right:
                    virtualKey = VirtualKeyCode.RIGHT; 
                    break;
 
                case Key.Down:
                    virtualKey = VirtualKeyCode.DOWN; 
                    break;
  
                case Key.Select: 
                    virtualKey = VirtualKeyCode.SELECT;
                    break; 
 
                case Key.Print:
                    virtualKey = VirtualKeyCode.PRINT;
                    break; 
 
                case Key.Execute: 
                    virtualKey = VirtualKeyCode.EXECUTE; 
                    break;
  
                case Key.Snapshot:
                    virtualKey = VirtualKeyCode.SNAPSHOT;
                    break;
  
                case Key.Insert:
                    virtualKey = VirtualKeyCode.INSERT; 
                    break; 
 
                case Key.Delete: 
                    virtualKey = VirtualKeyCode.DELETE;
                    break;
 
                case Key.Help: 
                    virtualKey = VirtualKeyCode.HELP;
                    break; 
  
                case Key.D0:
                    virtualKey = VirtualKeyCode.VK_0; 
                    break;
 
                case Key.D1:
                    virtualKey = VirtualKeyCode.VK_1; 
                    break;
  
                case Key.D2: 
                    virtualKey = VirtualKeyCode.VK_2;
                    break; 
 
                case Key.D3:
                    virtualKey = VirtualKeyCode.VK_3;
                    break; 
 
                case Key.D4: 
                    virtualKey = VirtualKeyCode.VK_4; 
                    break;
  
                case Key.D5:
                    virtualKey = VirtualKeyCode.VK_5;
                    break;
  
                case Key.D6:
                    virtualKey = VirtualKeyCode.VK_6; 
                    break; 
 
                case Key.D7: 
                    virtualKey = VirtualKeyCode.VK_7;
                    break;
 
                case Key.D8: 
                    virtualKey = VirtualKeyCode.VK_8;
                    break; 
  
                case Key.D9:
                    virtualKey = VirtualKeyCode.VK_9; 
                    break;
 
                case Key.A:
                    virtualKey = VirtualKeyCode.VK_A; 
                    break;
  
                case Key.B: 
                    virtualKey = VirtualKeyCode.VK_B;
                    break; 
 
                case Key.C:
                    virtualKey = VirtualKeyCode.VK_C;
                    break; 
 
                case Key.D: 
                    virtualKey = VirtualKeyCode.VK_D; 
                    break;
  
                case Key.E:
                    virtualKey = VirtualKeyCode.VK_E;
                    break;
  
                case Key.F:
                    virtualKey = VirtualKeyCode.VK_F; 
                    break; 
 
                case Key.G: 
                    virtualKey = VirtualKeyCode.VK_G;
                    break;
 
                case Key.H: 
                    virtualKey = VirtualKeyCode.VK_H;
                    break; 
  
                case Key.I:
                    virtualKey = VirtualKeyCode.VK_I; 
                    break;
 
                case Key.J:
                    virtualKey = VirtualKeyCode.VK_J; 
                    break;
  
                case Key.K: 
                    virtualKey = VirtualKeyCode.VK_K;
                    break; 
 
                case Key.L:
                    virtualKey = VirtualKeyCode.VK_L;
                    break; 
 
                case Key.M: 
                    virtualKey = VirtualKeyCode.VK_M; 
                    break;
  
                case Key.N:
                    virtualKey = VirtualKeyCode.VK_N;
                    break;
  
                case Key.O:
                    virtualKey = VirtualKeyCode.VK_O; 
                    break; 
 
                case Key.P: 
                    virtualKey = VirtualKeyCode.VK_P;
                    break;
 
                case Key.Q: 
                    virtualKey = VirtualKeyCode.VK_Q;
                    break; 
  
                case Key.R:
                    virtualKey = VirtualKeyCode.VK_R; 
                    break;
 
                case Key.S:
                    virtualKey = VirtualKeyCode.VK_S; 
                    break;
  
                case Key.T: 
                    virtualKey = VirtualKeyCode.VK_T;
                    break; 
 
                case Key.U:
                    virtualKey = VirtualKeyCode.VK_U;
                    break; 
 
                case Key.V: 
                    virtualKey = VirtualKeyCode.VK_V; 
                    break;
  
                case Key.W:
                    virtualKey = VirtualKeyCode.VK_W;
                    break;
  
                case Key.X:
                    virtualKey = VirtualKeyCode.VK_X; 
                    break; 
 
                case Key.Y: 
                    virtualKey = VirtualKeyCode.VK_Y;
                    break;
 
                case Key.Z: 
                    virtualKey = VirtualKeyCode.VK_Z;
                    break; 
  
                case Key.LWin:
                    virtualKey = VirtualKeyCode.LWIN; 
                    break;
 
                case Key.RWin:
                    virtualKey = VirtualKeyCode.RWIN; 
                    break;
  
                case Key.Apps: 
                    virtualKey = VirtualKeyCode.APPS;
                    break; 
 
                case Key.Sleep:
                    virtualKey = VirtualKeyCode.SLEEP;
                    break; 
 
                case Key.NumPad0: 
                    virtualKey = VirtualKeyCode.NUMPAD0; 
                    break;
  
                case Key.NumPad1:
                    virtualKey = VirtualKeyCode.NUMPAD1;
                    break;
  
                case Key.NumPad2:
                    virtualKey = VirtualKeyCode.NUMPAD2; 
                    break; 
 
                case Key.NumPad3: 
                    virtualKey = VirtualKeyCode.NUMPAD3;
                    break;
 
                case Key.NumPad4: 
                    virtualKey = VirtualKeyCode.NUMPAD4;
                    break; 
  
                case Key.NumPad5:
                    virtualKey = VirtualKeyCode.NUMPAD5; 
                    break;
 
                case Key.NumPad6:
                    virtualKey = VirtualKeyCode.NUMPAD6; 
                    break;
  
                case Key.NumPad7: 
                    virtualKey = VirtualKeyCode.NUMPAD7;
                    break; 
 
                case Key.NumPad8:
                    virtualKey = VirtualKeyCode.NUMPAD8;
                    break; 
 
                case Key.NumPad9: 
                    virtualKey = VirtualKeyCode.NUMPAD9; 
                    break;
  
                case Key.Multiply:
                    virtualKey = VirtualKeyCode.MULTIPLY;
                    break;
  
                case Key.Add:
                    virtualKey = VirtualKeyCode.ADD; 
                    break; 
 
                case Key.Separator: 
                    virtualKey = VirtualKeyCode.SEPARATOR;
                    break;
 
                case Key.Subtract: 
                    virtualKey = VirtualKeyCode.SUBTRACT;
                    break; 
  
                case Key.Decimal:
                    virtualKey = VirtualKeyCode.DECIMAL; 
                    break;
 
                case Key.Divide:
                    virtualKey = VirtualKeyCode.DIVIDE; 
                    break;
  
                case Key.F1: 
                    virtualKey = VirtualKeyCode.F1;
                    break; 
 
                case Key.F2:
                    virtualKey = VirtualKeyCode.F2;
                    break; 
 
                case Key.F3: 
                    virtualKey = VirtualKeyCode.F3; 
                    break;
  
                case Key.F4:
                    virtualKey = VirtualKeyCode.F4;
                    break;
  
                case Key.F5:
                    virtualKey = VirtualKeyCode.F5; 
                    break; 
 
                case Key.F6: 
                    virtualKey = VirtualKeyCode.F6;
                    break;
 
                case Key.F7: 
                    virtualKey = VirtualKeyCode.F7;
                    break; 
  
                case Key.F8:
                    virtualKey = VirtualKeyCode.F8; 
                    break;
 
                case Key.F9:
                    virtualKey = VirtualKeyCode.F9; 
                    break;
  
                case Key.F10: 
                    virtualKey = VirtualKeyCode.F10;
                    break; 
 
                case Key.F11:
                    virtualKey = VirtualKeyCode.F11;
                    break; 
 
                case Key.F12: 
                    virtualKey = VirtualKeyCode.F12; 
                    break;
  
                case Key.F13:
                    virtualKey = VirtualKeyCode.F13;
                    break;
  
                case Key.F14:
                    virtualKey = VirtualKeyCode.F14; 
                    break; 
 
                case Key.F15: 
                    virtualKey = VirtualKeyCode.F15;
                    break;
 
                case Key.F16: 
                    virtualKey = VirtualKeyCode.F16;
                    break; 
  
                case Key.F17:
                    virtualKey = VirtualKeyCode.F17; 
                    break;
 
                case Key.F18:
                    virtualKey = VirtualKeyCode.F18; 
                    break;
  
                case Key.F19: 
                    virtualKey = VirtualKeyCode.F19;
                    break; 
 
                case Key.F20:
                    virtualKey = VirtualKeyCode.F20;
                    break; 
 
                case Key.F21: 
                    virtualKey = VirtualKeyCode.F21; 
                    break;
  
                case Key.F22:
                    virtualKey = VirtualKeyCode.F22;
                    break;
  
                case Key.F23:
                    virtualKey = VirtualKeyCode.F23; 
                    break; 
 
                case Key.F24: 
                    virtualKey = VirtualKeyCode.F24;
                    break;
 
                case Key.NumLock: 
                    virtualKey = VirtualKeyCode.NUMLOCK;
                    break; 
  
                case Key.Scroll:
                    virtualKey = VirtualKeyCode.SCROLL; 
                    break;
 
                case Key.LeftShift:
                    virtualKey = VirtualKeyCode.LSHIFT; 
                    break;
  
                case Key.RightShift: 
                    virtualKey = VirtualKeyCode.RSHIFT;
                    break; 
 
                case Key.LeftCtrl:
                    virtualKey = VirtualKeyCode.LCONTROL;
                    break; 
 
                case Key.RightCtrl: 
                    virtualKey = VirtualKeyCode.RCONTROL; 
                    break;
  
                case Key.LeftAlt:
                    virtualKey = VirtualKeyCode.LMENU;
                    break;
  
                case Key.RightAlt:
                    virtualKey = VirtualKeyCode.RMENU; 
                    break; 
 
                case Key.BrowserBack: 
                    virtualKey = VirtualKeyCode.BROWSER_BACK;
                    break;
 
                case Key.BrowserForward: 
                    virtualKey = VirtualKeyCode.BROWSER_FORWARD;
                    break; 
  
                case Key.BrowserRefresh:
                    virtualKey = VirtualKeyCode.BROWSER_REFRESH; 
                    break;
 
                case Key.BrowserStop:
                    virtualKey = VirtualKeyCode.BROWSER_STOP; 
                    break;
  
                case Key.BrowserSearch: 
                    virtualKey = VirtualKeyCode.BROWSER_SEARCH;
                    break; 
 
                case Key.BrowserFavorites:
                    virtualKey = VirtualKeyCode.BROWSER_FAVORITES;
                    break; 
 
                case Key.BrowserHome: 
                    virtualKey = VirtualKeyCode.BROWSER_HOME; 
                    break;
  
                case Key.VolumeMute:
                    virtualKey = VirtualKeyCode.VOLUME_MUTE;
                    break;
  
                case Key.VolumeDown:
                    virtualKey = VirtualKeyCode.VOLUME_DOWN; 
                    break; 
 
                case Key.VolumeUp: 
                    virtualKey = VirtualKeyCode.VOLUME_UP;
                    break;
 
                case Key.MediaNextTrack: 
                    virtualKey = VirtualKeyCode.MEDIA_NEXT_TRACK;
                    break; 
  
                case Key.MediaPreviousTrack:
                    virtualKey = VirtualKeyCode.MEDIA_PREV_TRACK; 
                    break;
 
                case Key.MediaStop:
                    virtualKey = VirtualKeyCode.MEDIA_STOP; 
                    break;
  
                case Key.MediaPlayPause: 
                    virtualKey = VirtualKeyCode.MEDIA_PLAY_PAUSE;
                    break; 
 
                case Key.LaunchMail:
                    virtualKey = VirtualKeyCode.LAUNCH_MAIL;
                    break; 
 
                case Key.SelectMedia: 
                    virtualKey = VirtualKeyCode.LAUNCH_MEDIA_SELECT; 
                    break;
  
                case Key.LaunchApplication1:
                    virtualKey = VirtualKeyCode.LAUNCH_APP1;
                    break;
  
                case Key.LaunchApplication2:
                    virtualKey = VirtualKeyCode.LAUNCH_APP2; 
                    break; 
 
                case Key.OemSemicolon: 
                    virtualKey = VirtualKeyCode.OEM_1;
                    break;
 
                case Key.OemPlus: 
                    virtualKey = VirtualKeyCode.OEM_PLUS;
                    break; 
  
                case Key.OemComma:
                    virtualKey = VirtualKeyCode.OEM_COMMA; 
                    break;
 
                case Key.OemMinus:
                    virtualKey = VirtualKeyCode.OEM_MINUS; 
                    break;
  
                case Key.OemPeriod: 
                    virtualKey = VirtualKeyCode.OEM_PERIOD;
                    break; 
 
                case Key.OemQuestion:
                    virtualKey = VirtualKeyCode.OEM_2;
                    break; 
 
                case Key.OemTilde: 
                    virtualKey = VirtualKeyCode.OEM_3; 
                    break;
                
                case Key.OemOpenBrackets: 
                    virtualKey = VirtualKeyCode.OEM_4;
                    break;
 
                case Key.OemPipe: 
                    virtualKey = VirtualKeyCode.OEM_5;
                    break; 
  
                case Key.OemCloseBrackets:
                    virtualKey = VirtualKeyCode.OEM_6; 
                    break;
 
                case Key.OemQuotes:
                    virtualKey = VirtualKeyCode.OEM_7; 
                    break;
  
                case Key.Oem8: 
                    virtualKey = VirtualKeyCode.OEM_8;
                    break; 
 
                case Key.OemBackslash:
                    virtualKey = VirtualKeyCode.OEM_102;
                    break; 
 
                case Key.ImeProcessed: 
                    virtualKey = VirtualKeyCode.PROCESSKEY; 
                    break;
                
                case Key.Attn:                          // DbeNoRoman 
                    virtualKey = VirtualKeyCode.ATTN; // VK_DBE_NOROMAN
                    break; 
 
                case Key.CrSel:                          // DbeEnterWordRegisterMode
                    virtualKey = VirtualKeyCode.CRSEL; // VK_DBE_ENTERWORDREGISTERMODE
                    break; 
 
                case Key.ExSel:                          // EnterImeConfigureMode 
                    virtualKey = VirtualKeyCode.EXSEL; // VK_DBE_ENTERIMECONFIGMODE 
                    break;
  
                case Key.EraseEof:                       // DbeFlushString
                    virtualKey = VirtualKeyCode.EREOF; // VK_DBE_FLUSHSTRING
                    break;
  
                case Key.Play:                           // DbeCodeInput
                    virtualKey = VirtualKeyCode.PLAY;  // VK_DBE_CODEINPUT 
                    break; 
 
                case Key.Zoom:                           // DbeNoCodeInput 
                    virtualKey = VirtualKeyCode.ZOOM;  // VK_DBE_NOCODEINPUT
                    break;
 
                case Key.NoName:                          // DbeDetermineString 
                    virtualKey = VirtualKeyCode.NONAME; // VK_DBE_DETERMINESTRING
                    break; 
  
                case Key.Pa1:                          // DbeEnterDlgConversionMode
                    virtualKey = VirtualKeyCode.PA1; // VK_ENTERDLGCONVERSIONMODE 
                    break;
 
                case Key.OemClear:
                    virtualKey = VirtualKeyCode.OEM_CLEAR; 
                    break;
  
                default: 
                    virtualKey = 0;
                    break; 
            }
 
            return virtualKey;
        } 
    }
}
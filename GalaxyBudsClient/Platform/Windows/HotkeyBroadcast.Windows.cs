using System.Collections.Generic;
using System.Linq;
using GalaxyBudsClient.Platform.Interfaces;
using WindowsInput;
using WindowsInput.Native;
using Key = Avalonia.Input.Key;

namespace GalaxyBudsClient.Platform.Windows
{
    public class HotkeyBroadcast : IHotkeyBroadcast
    {
        public void SendKeys(IEnumerable<Key> keys)
        {
            var sim = new InputSimulator();
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
            VirtualKeyCode virtualKey = key switch
            {
                Key.Cancel => VirtualKeyCode.CANCEL,
                Key.Back => VirtualKeyCode.BACK,
                Key.Tab => VirtualKeyCode.TAB,
                Key.Clear => VirtualKeyCode.CLEAR,
                Key.Return => VirtualKeyCode.RETURN,
                Key.Pause => VirtualKeyCode.PAUSE,
                Key.Capital => VirtualKeyCode.CAPITAL,
                Key.KanaMode => VirtualKeyCode.KANA,
                Key.JunjaMode => VirtualKeyCode.JUNJA,
                Key.FinalMode => VirtualKeyCode.FINAL,
                Key.KanjiMode => VirtualKeyCode.KANJI,
                Key.Escape => VirtualKeyCode.ESCAPE,
                Key.ImeConvert => VirtualKeyCode.CONVERT,
                Key.ImeNonConvert => VirtualKeyCode.NONCONVERT,
                Key.ImeAccept => VirtualKeyCode.ACCEPT,
                Key.ImeModeChange => VirtualKeyCode.MODECHANGE,
                Key.Space => VirtualKeyCode.SPACE,
                Key.Prior => VirtualKeyCode.PRIOR,
                Key.Next => VirtualKeyCode.NEXT,
                Key.End => VirtualKeyCode.END,
                Key.Home => VirtualKeyCode.HOME,
                Key.Left => VirtualKeyCode.LEFT,
                Key.Up => VirtualKeyCode.UP,
                Key.Right => VirtualKeyCode.RIGHT,
                Key.Down => VirtualKeyCode.DOWN,
                Key.Select => VirtualKeyCode.SELECT,
                Key.Print => VirtualKeyCode.PRINT,
                Key.Execute => VirtualKeyCode.EXECUTE,
                Key.Snapshot => VirtualKeyCode.SNAPSHOT,
                Key.Insert => VirtualKeyCode.INSERT,
                Key.Delete => VirtualKeyCode.DELETE,
                Key.Help => VirtualKeyCode.HELP,
                Key.D0 => VirtualKeyCode.VK_0,
                Key.D1 => VirtualKeyCode.VK_1,
                Key.D2 => VirtualKeyCode.VK_2,
                Key.D3 => VirtualKeyCode.VK_3,
                Key.D4 => VirtualKeyCode.VK_4,
                Key.D5 => VirtualKeyCode.VK_5,
                Key.D6 => VirtualKeyCode.VK_6,
                Key.D7 => VirtualKeyCode.VK_7,
                Key.D8 => VirtualKeyCode.VK_8,
                Key.D9 => VirtualKeyCode.VK_9,
                Key.A => VirtualKeyCode.VK_A,
                Key.B => VirtualKeyCode.VK_B,
                Key.C => VirtualKeyCode.VK_C,
                Key.D => VirtualKeyCode.VK_D,
                Key.E => VirtualKeyCode.VK_E,
                Key.F => VirtualKeyCode.VK_F,
                Key.G => VirtualKeyCode.VK_G,
                Key.H => VirtualKeyCode.VK_H,
                Key.I => VirtualKeyCode.VK_I,
                Key.J => VirtualKeyCode.VK_J,
                Key.K => VirtualKeyCode.VK_K,
                Key.L => VirtualKeyCode.VK_L,
                Key.M => VirtualKeyCode.VK_M,
                Key.N => VirtualKeyCode.VK_N,
                Key.O => VirtualKeyCode.VK_O,
                Key.P => VirtualKeyCode.VK_P,
                Key.Q => VirtualKeyCode.VK_Q,
                Key.R => VirtualKeyCode.VK_R,
                Key.S => VirtualKeyCode.VK_S,
                Key.T => VirtualKeyCode.VK_T,
                Key.U => VirtualKeyCode.VK_U,
                Key.V => VirtualKeyCode.VK_V,
                Key.W => VirtualKeyCode.VK_W,
                Key.X => VirtualKeyCode.VK_X,
                Key.Y => VirtualKeyCode.VK_Y,
                Key.Z => VirtualKeyCode.VK_Z,
                Key.LWin => VirtualKeyCode.LWIN,
                Key.RWin => VirtualKeyCode.RWIN,
                Key.Apps => VirtualKeyCode.APPS,
                Key.Sleep => VirtualKeyCode.SLEEP,
                Key.NumPad0 => VirtualKeyCode.NUMPAD0,
                Key.NumPad1 => VirtualKeyCode.NUMPAD1,
                Key.NumPad2 => VirtualKeyCode.NUMPAD2,
                Key.NumPad3 => VirtualKeyCode.NUMPAD3,
                Key.NumPad4 => VirtualKeyCode.NUMPAD4,
                Key.NumPad5 => VirtualKeyCode.NUMPAD5,
                Key.NumPad6 => VirtualKeyCode.NUMPAD6,
                Key.NumPad7 => VirtualKeyCode.NUMPAD7,
                Key.NumPad8 => VirtualKeyCode.NUMPAD8,
                Key.NumPad9 => VirtualKeyCode.NUMPAD9,
                Key.Multiply => VirtualKeyCode.MULTIPLY,
                Key.Add => VirtualKeyCode.ADD,
                Key.Separator => VirtualKeyCode.SEPARATOR,
                Key.Subtract => VirtualKeyCode.SUBTRACT,
                Key.Decimal => VirtualKeyCode.DECIMAL,
                Key.Divide => VirtualKeyCode.DIVIDE,
                Key.F1 => VirtualKeyCode.F1,
                Key.F2 => VirtualKeyCode.F2,
                Key.F3 => VirtualKeyCode.F3,
                Key.F4 => VirtualKeyCode.F4,
                Key.F5 => VirtualKeyCode.F5,
                Key.F6 => VirtualKeyCode.F6,
                Key.F7 => VirtualKeyCode.F7,
                Key.F8 => VirtualKeyCode.F8,
                Key.F9 => VirtualKeyCode.F9,
                Key.F10 => VirtualKeyCode.F10,
                Key.F11 => VirtualKeyCode.F11,
                Key.F12 => VirtualKeyCode.F12,
                Key.F13 => VirtualKeyCode.F13,
                Key.F14 => VirtualKeyCode.F14,
                Key.F15 => VirtualKeyCode.F15,
                Key.F16 => VirtualKeyCode.F16,
                Key.F17 => VirtualKeyCode.F17,
                Key.F18 => VirtualKeyCode.F18,
                Key.F19 => VirtualKeyCode.F19,
                Key.F20 => VirtualKeyCode.F20,
                Key.F21 => VirtualKeyCode.F21,
                Key.F22 => VirtualKeyCode.F22,
                Key.F23 => VirtualKeyCode.F23,
                Key.F24 => VirtualKeyCode.F24,
                Key.NumLock => VirtualKeyCode.NUMLOCK,
                Key.Scroll => VirtualKeyCode.SCROLL,
                Key.LeftShift => VirtualKeyCode.LSHIFT,
                Key.RightShift => VirtualKeyCode.RSHIFT,
                Key.LeftCtrl => VirtualKeyCode.LCONTROL,
                Key.RightCtrl => VirtualKeyCode.RCONTROL,
                Key.LeftAlt => VirtualKeyCode.LMENU,
                Key.RightAlt => VirtualKeyCode.RMENU,
                Key.BrowserBack => VirtualKeyCode.BROWSER_BACK,
                Key.BrowserForward => VirtualKeyCode.BROWSER_FORWARD,
                Key.BrowserRefresh => VirtualKeyCode.BROWSER_REFRESH,
                Key.BrowserStop => VirtualKeyCode.BROWSER_STOP,
                Key.BrowserSearch => VirtualKeyCode.BROWSER_SEARCH,
                Key.BrowserFavorites => VirtualKeyCode.BROWSER_FAVORITES,
                Key.BrowserHome => VirtualKeyCode.BROWSER_HOME,
                Key.VolumeMute => VirtualKeyCode.VOLUME_MUTE,
                Key.VolumeDown => VirtualKeyCode.VOLUME_DOWN,
                Key.VolumeUp => VirtualKeyCode.VOLUME_UP,
                Key.MediaNextTrack => VirtualKeyCode.MEDIA_NEXT_TRACK,
                Key.MediaPreviousTrack => VirtualKeyCode.MEDIA_PREV_TRACK,
                Key.MediaStop => VirtualKeyCode.MEDIA_STOP,
                Key.MediaPlayPause => VirtualKeyCode.MEDIA_PLAY_PAUSE,
                Key.LaunchMail => VirtualKeyCode.LAUNCH_MAIL,
                Key.SelectMedia => VirtualKeyCode.LAUNCH_MEDIA_SELECT,
                Key.LaunchApplication1 => VirtualKeyCode.LAUNCH_APP1,
                Key.LaunchApplication2 => VirtualKeyCode.LAUNCH_APP2,
                Key.OemSemicolon => VirtualKeyCode.OEM_1,
                Key.OemPlus => VirtualKeyCode.OEM_PLUS,
                Key.OemComma => VirtualKeyCode.OEM_COMMA,
                Key.OemMinus => VirtualKeyCode.OEM_MINUS,
                Key.OemPeriod => VirtualKeyCode.OEM_PERIOD,
                Key.OemQuestion => VirtualKeyCode.OEM_2,
                Key.OemTilde => VirtualKeyCode.OEM_3,
                Key.OemOpenBrackets => VirtualKeyCode.OEM_4,
                Key.OemPipe => VirtualKeyCode.OEM_5,
                Key.OemCloseBrackets => VirtualKeyCode.OEM_6,
                Key.OemQuotes => VirtualKeyCode.OEM_7,
                Key.Oem8 => VirtualKeyCode.OEM_8,
                Key.OemBackslash => VirtualKeyCode.OEM_102,
                Key.ImeProcessed => VirtualKeyCode.PROCESSKEY,
                Key.Attn => // DbeNoRoman 
                    VirtualKeyCode.ATTN // VK_DBE_NOROMAN
                ,
                Key.CrSel => // DbeEnterWordRegisterMode
                    VirtualKeyCode.CRSEL // VK_DBE_ENTERWORDREGISTERMODE
                ,
                Key.ExSel => // EnterImeConfigureMode 
                    VirtualKeyCode.EXSEL // VK_DBE_ENTERIMECONFIGMODE 
                ,
                Key.EraseEof => // DbeFlushString
                    VirtualKeyCode.EREOF // VK_DBE_FLUSHSTRING
                ,
                Key.Play => // DbeCodeInput
                    VirtualKeyCode.PLAY // VK_DBE_CODEINPUT 
                ,
                Key.Zoom => // DbeNoCodeInput 
                    VirtualKeyCode.ZOOM // VK_DBE_NOCODEINPUT
                ,
                Key.NoName => // DbeDetermineString 
                    VirtualKeyCode.NONAME // VK_DBE_DETERMINESTRING
                ,
                Key.Pa1 => // DbeEnterDlgConversionMode
                    VirtualKeyCode.PA1 // VK_ENTERDLGCONVERSIONMODE 
                ,
                Key.OemClear => VirtualKeyCode.OEM_CLEAR,
                _ => 0
            };

            return virtualKey;
        } 
    }
}
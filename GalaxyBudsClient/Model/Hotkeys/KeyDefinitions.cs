using System;
using Avalonia.Input;

namespace GalaxyBudsClient.Model.Hotkeys
{
    [Flags]
    public enum ModifierKeys : uint
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

    public static class UserKeys
    {
        public static readonly Keys[] KeyList =
        {
            Keys.Backspace,
            Keys.Tab,
            Keys.LineFeed,
            Keys.Clear,
            Keys.Enter,
            Keys.Pause,
            Keys.CapsLock,
            Keys.Escape,
            Keys.Space,
            Keys.PageUp,
            Keys.PageDown,
            Keys.End,
            Keys.Home,
            Keys.Left,
            Keys.Up,
            Keys.Right,
            Keys.Down,
            Keys.Select,
            Keys.Print,
            Keys.Execute,
            Keys.PrintScreen,
            Keys.Insert,
            Keys.Delete,
            Keys.Help,
            Keys.Decimal0,
            Keys.Decimal1,
            Keys.Decimal2,
            Keys.Decimal3,
            Keys.Decimal4,
            Keys.Decimal5,
            Keys.Decimal6,
            Keys.Decimal7,
            Keys.Decimal8,
            Keys.Decimal9,
            Keys.A,
            Keys.B,
            Keys.C,
            Keys.D,
            Keys.E,
            Keys.F,
            Keys.G,
            Keys.H,
            Keys.I,
            Keys.J,
            Keys.K,
            Keys.L,
            Keys.M,
            Keys.N,
            Keys.O,
            Keys.P,
            Keys.Q,
            Keys.R,
            Keys.S,
            Keys.T,
            Keys.U,
            Keys.V,
            Keys.W,
            Keys.X,
            Keys.Y,
            Keys.Z,
            Keys.Apps,
            Keys.NumPad0,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.Multiply,
            Keys.Add,
            Keys.Separator,
            Keys.Subtract,
            Keys.Decimal,
            Keys.Divide,
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
            Keys.F9,
            Keys.F10,
            Keys.F11,
            Keys.F12,
            Keys.F13,
            Keys.F14,
            Keys.F15,
            Keys.F16,
            Keys.F17,
            Keys.F18,
            Keys.F19,
            Keys.F20,
            Keys.F21,
            Keys.F22,
            Keys.F23,
            Keys.F24,
            Keys.NumLock,
            Keys.ScrollLock,
            Keys.BrowserBack,
            Keys.BrowserForward,
            Keys.BrowserRefresh,
            Keys.BrowserStop,
            Keys.BrowserSearch,
            Keys.BrowserFavorites,
            Keys.BrowserHome,
            Keys.VolumeMute,
            Keys.VolumeDown,
            Keys.VolumeUp,
            Keys.MediaNextTrack,
            Keys.MediaPreviousTrack,
            Keys.MediaStop,
            Keys.MediaPlayPause,
            Keys.LaunchMail,
            Keys.SelectMedia,
            Keys.LaunchApplication1,
            Keys.LaunchApplication2,
            Keys.OemSemicolon,
            Keys.OemPlus,
            Keys.OemComma,
            Keys.OemMinus,
            Keys.OemPeriod,
            Keys.OemQuestion,
            Keys.OemTilde,
            Keys.OemOpenBrackets,
            Keys.OemPipe,
            Keys.OemCloseBrackets,
            Keys.OemQuotes,
            Keys.Oem8,
            Keys.OemBackslash,
            Keys.Play,
            Keys.Zoom,
            Keys.CancelKey
        };
    }
    
    [Flags]
    public enum Keys
    {
        None = 0x0,
        
        /// <summary>
        ///  The BACKSPACE key.
        /// </summary>
        Backspace = 0x08,

        /// <summary>
        ///  The TAB key.
        /// </summary>
        Tab = 0x09,

        /// <summary>
        ///  The CLEAR key.
        /// </summary>
        LineFeed = 0x0A,
        
        /// <summary>
        ///  The CLEAR key.
        /// </summary>
        Clear = 0x0C,

        /// <summary>
        ///  The ENTER key.
        /// </summary>
        Enter = 0x0D,

        /// <summary>
        ///  The SHIFT key.
        /// </summary>
        ShiftKey = 0x10,

        /// <summary>
        ///  The CTRL key.
        /// </summary>
        ControlKey = 0x11,

        /// <summary>
        ///  The ALT key.
        /// </summary>
        Menu = 0x12,

        /// <summary>
        ///  The PAUSE key.
        /// </summary>
        Pause = 0x13,

        /// <summary>
        ///  The CAPS LOCK key.
        /// </summary>
        CapsLock = 0x14,
        
        /// <summary>
        ///  The IME Kana mode key.
        /// </summary>
        KanaMode = 0x15,

        /// <summary>
        ///  The IME Hanguel mode key.
        /// </summary>
        /// HanguelMode = 0x15,

        /// <summary>
        ///  The IME Hangul mode key.
        /// </summary>
        /// HangulMode = 0x15,

        /// <summary>
        ///  The IME Junja mode key.
        /// </summary>
        JunjaMode = 0x17,

        /// <summary>
        ///  The IME Final mode key.
        /// </summary>
        FinalMode = 0x18,

        /// <summary>
        ///  The IME Hanja mode key.
        /// </summary>
        /// HanjaMode = 0x19,

        /// <summary>
        ///  The IME Kanji mode key.
        /// </summary>
        KanjiMode = 0x19,

        /// <summary>
        ///  The ESC key.
        /// </summary>
        Escape = 0x1B,
        
        /// <summary>
        ///  The IME Convert key.
        /// </summary>
        IMEConvert = 0x1C,

        /// <summary>
        ///  The IME NonConvert key.
        /// </summary>
        IMENonconvert = 0x1D,

        /// <summary>
        ///  The IME Accept key.
        /// </summary>
        IMEAccept = 0x1E,
        
        /// <summary>
        ///  The IME Mode change request.
        /// </summary>
        IMEModeChange = 0x1F,

        /// <summary>
        ///  The SPACEBAR key.
        /// </summary>
        Space = 0x20,
        
        /// <summary>
        ///  The PAGE UP key.
        /// </summary>
        PageUp = 0x21,

        /// <summary>
        ///  The PAGE DOWN key.
        /// </summary>
        PageDown = 0x22,

        /// <summary>
        ///  The END key.
        /// </summary>
        End = 0x23,

        /// <summary>
        ///  The HOME key.
        /// </summary>
        Home = 0x24,

        /// <summary>
        ///  The LEFT ARROW key.
        /// </summary>
        Left = 0x25,

        /// <summary>
        ///  The UP ARROW key.
        /// </summary>
        Up = 0x26,

        /// <summary>
        ///  The RIGHT ARROW key.
        /// </summary>
        Right = 0x27,

        /// <summary>
        ///  The DOWN ARROW key.
        /// </summary>
        Down = 0x28,

        /// <summary>
        ///  The SELECT key.
        /// </summary>
        Select = 0x29,

        /// <summary>
        ///  The PRINT key.
        /// </summary>
        Print = 0x2A,

        /// <summary>
        ///  The EXECUTE key.
        /// </summary>
        Execute = 0x2B,

        /// <summary>
        ///  The PRINT SCREEN key.
        /// </summary>
        PrintScreen = 0x2C,
        
        /// <summary>
        ///  The INS key.
        /// </summary>
        Insert = 0x2D,

        /// <summary>
        ///  The DEL key.
        /// </summary>
        Delete = 0x2E,

        /// <summary>
        ///  The HELP key.
        /// </summary>
        Help = 0x2F,
        
        /// <summary>
        ///  The 0 key.
        /// </summary>
        Decimal0 = 0x30, // 0

        /// <summary>
        ///  The 1 key.
        /// </summary>
        Decimal1 = 0x31, // 1

        /// <summary>
        ///  The 2 key.
        /// </summary>
        Decimal2 = 0x32, // 2

        /// <summary>
        ///  The 3 key.
        /// </summary>
        Decimal3 = 0x33, // 3

        /// <summary>
        ///  The 4 key.
        /// </summary>
        Decimal4 = 0x34, // 4

        /// <summary>
        ///  The 5 key.
        /// </summary>
        Decimal5 = 0x35, // 5

        /// <summary>
        ///  The 6 key.
        /// </summary>
        Decimal6 = 0x36, // 6

        /// <summary>
        ///  The 7 key.
        /// </summary>
        Decimal7 = 0x37, // 7

        /// <summary>
        ///  The 8 key.
        /// </summary>
        Decimal8 = 0x38, // 8

        /// <summary>
        ///  The 9 key.
        /// </summary>
        Decimal9 = 0x39, // 9

        /// <summary>
        ///  The A key.
        /// </summary>
        A = 0x41,

        /// <summary>
        ///  The B key.
        /// </summary>
        B = 0x42,

        /// <summary>
        ///  The C key.
        /// </summary>
        C = 0x43,

        /// <summary>
        ///  The D key.
        /// </summary>
        D = 0x44,

        /// <summary>
        ///  The E key.
        /// </summary>
        E = 0x45,

        /// <summary>
        ///  The F key.
        /// </summary>
        F = 0x46,

        /// <summary>
        ///  The G key.
        /// </summary>
        G = 0x47,

        /// <summary>
        ///  The H key.
        /// </summary>
        H = 0x48,

        /// <summary>
        ///  The I key.
        /// </summary>
        I = 0x49,

        /// <summary>
        ///  The J key.
        /// </summary>
        J = 0x4A,

        /// <summary>
        ///  The K key.
        /// </summary>
        K = 0x4B,

        /// <summary>
        ///  The L key.
        /// </summary>
        L = 0x4C,

        /// <summary>
        ///  The M key.
        /// </summary>
        M = 0x4D,

        /// <summary>
        ///  The N key.
        /// </summary>
        N = 0x4E,

        /// <summary>
        ///  The O key.
        /// </summary>
        O = 0x4F,

        /// <summary>
        ///  The P key.
        /// </summary>
        P = 0x50,

        /// <summary>
        ///  The Q key.
        /// </summary>
        Q = 0x51,

        /// <summary>
        ///  The R key.
        /// </summary>
        R = 0x52,

        /// <summary>
        ///  The S key.
        /// </summary>
        S = 0x53,

        /// <summary>
        ///  The T key.
        /// </summary>
        T = 0x54,

        /// <summary>
        ///  The U key.
        /// </summary>
        U = 0x55,

        /// <summary>
        ///  The V key.
        /// </summary>
        V = 0x56,

        /// <summary>
        ///  The W key.
        /// </summary>
        W = 0x57,

        /// <summary>
        ///  The X key.
        /// </summary>
        X = 0x58,

        /// <summary>
        ///  The Y key.
        /// </summary>
        Y = 0x59,

        /// <summary>
        ///  The Z key.
        /// </summary>
        Z = 0x5A,
        
        /// <summary>
        ///  The left Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        LWin = 0x5B,

        /// <summary>
        ///  The right Windows logo key (Microsoft Natural Keyboard).
        /// </summary>
        RWin = 0x5C,

        /// <summary>
        ///  The Application key (Microsoft Natural Keyboard).
        /// </summary>
        Apps = 0x5D,

        /// <summary>
        ///  The Computer Sleep key.
        /// </summary>
        Sleep = 0x5F,

        /// <summary>
        ///  The 0 key on the numeric keypad.
        /// </summary>
        NumPad0 = 0x60,

        /// <summary>
        ///  The 1 key on the numeric keypad.
        /// </summary>
        NumPad1 = 0x61,

        /// <summary>
        ///  The 2 key on the numeric keypad.
        /// </summary>
        NumPad2 = 0x62,

        /// <summary>
        ///  The 3 key on the numeric keypad.
        /// </summary>
        NumPad3 = 0x63,

        /// <summary>
        ///  The 4 key on the numeric keypad.
        /// </summary>
        NumPad4 = 0x64,

        /// <summary>
        ///  The 5 key on the numeric keypad.
        /// </summary>
        NumPad5 = 0x65,

        /// <summary>
        ///  The 6 key on the numeric keypad.
        /// </summary>
        NumPad6 = 0x66,

        /// <summary>
        ///  The 7 key on the numeric keypad.
        /// </summary>
        NumPad7 = 0x67,

        /// <summary>
        ///  The 8 key on the numeric keypad.
        /// </summary>
        NumPad8 = 0x68,

        /// <summary>
        ///  The 9 key on the numeric keypad.
        /// </summary>
        NumPad9 = 0x69,

        /// <summary>
        ///  The Multiply key.
        /// </summary>
        Multiply = 0x6A,

        /// <summary>
        ///  The Add key.
        /// </summary>
        Add = 0x6B,

        /// <summary>
        ///  The Separator key.
        /// </summary>
        Separator = 0x6C,

        /// <summary>
        ///  The Subtract key.
        /// </summary>
        Subtract = 0x6D,

        /// <summary>
        ///  The Decimal key.
        /// </summary>
        Decimal = 0x6E,

        /// <summary>
        ///  The Divide key.
        /// </summary>
        Divide = 0x6F,

        /// <summary>
        ///  The F1 key.
        /// </summary>
        F1 = 0x70,

        /// <summary>
        ///  The F2 key.
        /// </summary>
        F2 = 0x71,

        /// <summary>
        ///  The F3 key.
        /// </summary>
        F3 = 0x72,

        /// <summary>
        ///  The F4 key.
        /// </summary>
        F4 = 0x73,

        /// <summary>
        ///  The F5 key.
        /// </summary>
        F5 = 0x74,

        /// <summary>
        ///  The F6 key.
        /// </summary>
        F6 = 0x75,

        /// <summary>
        ///  The F7 key.
        /// </summary>
        F7 = 0x76,

        /// <summary>
        ///  The F8 key.
        /// </summary>
        F8 = 0x77,

        /// <summary>
        ///  The F9 key.
        /// </summary>
        F9 = 0x78,

        /// <summary>
        ///  The F10 key.
        /// </summary>
        F10 = 0x79,

        /// <summary>
        ///  The F11 key.
        /// </summary>
        F11 = 0x7A,

        /// <summary>
        ///  The F12 key.
        /// </summary>
        F12 = 0x7B,

        /// <summary>
        ///  The F13 key.
        /// </summary>
        F13 = 0x7C,

        /// <summary>
        ///  The F14 key.
        /// </summary>
        F14 = 0x7D,

        /// <summary>
        ///  The F15 key.
        /// </summary>
        F15 = 0x7E,

        /// <summary>
        ///  The F16 key.
        /// </summary>
        F16 = 0x7F,

        /// <summary>
        ///  The F17 key.
        /// </summary>
        F17 = 0x80,

        /// <summary>
        ///  The F18 key.
        /// </summary>
        F18 = 0x81,

        /// <summary>
        ///  The F19 key.
        /// </summary>
        F19 = 0x82,

        /// <summary>
        ///  The F20 key.
        /// </summary>
        F20 = 0x83,

        /// <summary>
        ///  The F21 key.
        /// </summary>
        F21 = 0x84,

        /// <summary>
        ///  The F22 key.
        /// </summary>
        F22 = 0x85,

        /// <summary>
        ///  The F23 key.
        /// </summary>
        F23 = 0x86,

        /// <summary>
        ///  The F24 key.
        /// </summary>
        F24 = 0x87,

        /// <summary>
        ///  The NUM LOCK key.
        /// </summary>
        NumLock = 0x90,

        /// <summary>
        ///  The SCROLL LOCK key.
        /// </summary>
        ScrollLock = 0x91,

        /// <summary>
        ///  The left SHIFT key.
        /// </summary>
        LShiftKey = 0xA0,

        /// <summary>
        ///  The right SHIFT key.
        /// </summary>
        RShiftKey = 0xA1,

        /// <summary>
        ///  The left CTRL key.
        /// </summary>
        LControlKey = 0xA2,

        /// <summary>
        ///  The right CTRL key.
        /// </summary>
        RControlKey = 0xA3,

        /// <summary>
        ///  The left ALT key.
        /// </summary>
        LMenu = 0xA4,

        /// <summary>
        ///  The right ALT key.
        /// </summary>
        RMenu = 0xA5,

        /// <summary>
        ///  The Browser Back key.
        /// </summary>
        BrowserBack = 0xA6,

        /// <summary>
        ///  The Browser Forward key.
        /// </summary>
        BrowserForward = 0xA7,

        /// <summary>
        ///  The Browser Refresh key.
        /// </summary>
        BrowserRefresh = 0xA8,

        /// <summary>
        ///  The Browser Stop key.
        /// </summary>
        BrowserStop = 0xA9,

        /// <summary>
        ///  The Browser Search key.
        /// </summary>
        BrowserSearch = 0xAA,

        /// <summary>
        ///  The Browser Favorites key.
        /// </summary>
        BrowserFavorites = 0xAB,

        /// <summary>
        ///  The Browser Home key.
        /// </summary>
        BrowserHome = 0xAC,

        /// <summary>
        ///  The Volume Mute key.
        /// </summary>
        VolumeMute = 0xAD,

        /// <summary>
        ///  The Volume Down key.
        /// </summary>
        VolumeDown = 0xAE,

        /// <summary>
        ///  The Volume Up key.
        /// </summary>
        VolumeUp = 0xAF,

        /// <summary>
        ///  The Media Next Track key.
        /// </summary>
        MediaNextTrack = 0xB0,

        /// <summary>
        ///  The Media Previous Track key.
        /// </summary>
        MediaPreviousTrack = 0xB1,

        /// <summary>
        ///  The Media Stop key.
        /// </summary>
        MediaStop = 0xB2,

        /// <summary>
        ///  The Media Play Pause key.
        /// </summary>
        MediaPlayPause = 0xB3,

        /// <summary>
        ///  The Launch Mail key.
        /// </summary>
        LaunchMail = 0xB4,

        /// <summary>
        ///  The Select Media key.
        /// </summary>
        SelectMedia = 0xB5,

        /// <summary>
        ///  The Launch Application1 key.
        /// </summary>
        LaunchApplication1 = 0xB6,

        /// <summary>
        ///  The Launch Application2 key.
        /// </summary>
        LaunchApplication2 = 0xB7,

        /// <summary>
        ///  The Oem Semicolon key.
        /// </summary>
        OemSemicolon = 0xBA,
        
        /// <summary>
        ///  The Oem plus key.
        /// </summary>
        OemPlus = 0xBB,

        /// <summary>
        ///  The Oem comma key.
        /// </summary>
        OemComma = 0xBC,

        /// <summary>
        ///  The Oem Minus key.
        /// </summary>
        OemMinus = 0xBD,

        /// <summary>
        ///  The Oem Period key.
        /// </summary>
        OemPeriod = 0xBE,

        /// <summary>
        ///  The Oem Question key.
        /// </summary>
        OemQuestion = 0xBF,
        
        /// <summary>
        ///  The Oem tilde key.
        /// </summary>
        OemTilde = 0xC0,

        /// <summary>
        ///  The Oem Open Brackets key.
        /// </summary>
        OemOpenBrackets = 0xDB,

        /// <summary>
        ///  The Oem Pipe key.
        /// </summary>
        OemPipe = 0xDC,

        /// <summary>
        ///  The Oem Close Brackets key.
        /// </summary>
        OemCloseBrackets = 0xDD,

        /// <summary>
        ///  The Oem Quotes key.
        /// </summary>
        OemQuotes = 0xDE,
        
        /// <summary>
        ///  The Oem8 key.
        /// </summary>
        Oem8 = 0xDF,

        /// <summary>
        ///  The Oem Backslash key.
        /// </summary>
        OemBackslash = 0xE2,

        /// <summary>
        ///  The PLAY key.
        /// </summary>
        Play = 0xFA,

        /// <summary>
        ///  The ZOOM key.
        /// </summary>
        Zoom = 0xFB,

        /// <summary>
        ///  The CANCEL key.
        /// </summary>
        CancelKey = 0x03,
    }
    
    public static class KeyExtensions
    {
        /// Convert <c>Avalonia.Input.Key</c> to our <c>Keys</c> enum
        public static Keys ToKeysEnum(this Key key)
        {
            switch (key)
            {
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
                case Key.A:
                    return Keys.A;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
            throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }
}
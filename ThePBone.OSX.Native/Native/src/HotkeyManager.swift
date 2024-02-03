//
// HotkeyManager.swift
// NativeInterop
//
// Created by nift4 on 03.02.2024.
//

import Foundation
import AppKit
import Sauce // for me, xcode claims there is "no such module" but build works fine
import Magnet

// This briding class exists because Magnet package does not export classes into Obj-C
@objc public class HotkeyManager : NSObject {
    @objc public static func unregisterAll() {
        HotKeyCenter.shared.unregisterAll()
    }
    
    @objc public static func register(str: UnsafeMutableRawPointer, identifier: UInt, win32Keyflags: UInt, win32Modflags: UInt) -> Bool {
        var cocoaModflags = (win32Modflags & 1) != 0 ? NSEvent.ModifierFlags.option.rawValue : 0
        cocoaModflags |= (win32Modflags & 2) != 0 ? NSEvent.ModifierFlags.control.rawValue : 0
        cocoaModflags |= (win32Modflags & 4) != 0 ? NSEvent.ModifierFlags.shift.rawValue : 0
        cocoaModflags |= (win32Modflags & 8) != 0 ? NSEvent.ModifierFlags.command.rawValue : 0
        if let win32Key = Keys(rawValue:win32Keyflags) {
            let cocoaKeyflags: Key? = switch (win32Key) {
            case .Backspace: .delete
            case .Tab: .tab
            case .LineFeed: .keypadEnter
            case .Clear: .keypadClear
            case .Enter: .return
            // case .Pause:
            // case .CapsLock:
            case .Escape: .escape
            case .Space: .space
            case .PageUp: .pageUp
            case .PageDown: .pageDown
            case .End: .end
            case .Home: .home
            case .Left: .leftArrow
            case .Up: .upArrow
            case .Right: .rightArrow
            case .Down: .downArrow
            // case .Select:
            // case .Print:
            // case .Execute: what even is that
            // case .PrintScreen:
            // case .Insert:
            case .Delete: .forwardDelete
            case .Help: .help
            case .Decimal0: .zero
            case .Decimal1: .one
            case .Decimal2: .two
            case .Decimal3: .three
            case .Decimal4: .four
            case .Decimal5: .five
            case .Decimal6: .six
            case .Decimal7: .seven
            case .Decimal8: .eight
            case .Decimal9: .nine
            case .A: .a
            case .B: .b
            case .C: .c
            case .D: .d
            case .E: .e
            case .F: .f
            case .G: .g
            case .H: .h
            case .I: .i
            case .J: .j
            case .K: .k
            case .L: .l
            case .M: .m
            case .N: .n
            case .O: .o
            case .P: .p
            case .Q: .q
            case .R: .r
            case .S: .s
            case .U: .u
            case .T: .t
            case .V: .v
            case .W: .w
            case .X: .x
            case .Y: .y
            case .Z: .z
            // case .Apps:
            case .NumPad0: .keypadZero
            case .NumPad1: .keypadOne
            case .NumPad2: .keypadTwo
            case .NumPad3: .keypadThree
            case .NumPad4: .keypadFour
            case .NumPad5: .keypadFive
            case .NumPad6: .keypadSix
            case .NumPad7: .keypadSeven
            case .NumPad8: .keypadEight
            case .NumPad9: .keypadNine
            case .Multiply: .keypadMultiply
            case .Add: .keypadPlus
            case .Separator: .keypadDecimal // <-- mapped twice
            case .Subtract: .keypadMinus
            case .Decimal: .keypadDecimal // <-- mapped twice
            case .Divide: .keypadDivide
            case .F1: .f1
            case .F2: .f2
            case .F3: .f3
            case .F4: .f4
            case .F5: .f5
            case .F6: .f6
            case .F7: .f7
            case .F8: .f8
            case .F9: .f9
            case .F10: .f10
            case .F11: .f11
            case .F12: .f12
            case .F13: .f13
            case .F14: .f14
            case .F15: .f15
            case .F16: .f16
            case .F17: .f17
            case .F18: .f18
            case .F19: .f19
            case .F20: .f20
            // case .F21:
            // case .F22:
            // case .F23:
            // case .F24:
            // case .NumLock:
            // case .ScrollLock:
            // case .BrowserBack:
            // case .BrowserForward:
            // case .BrowserRefresh:
            // case .BrowserStop:
            // case .BrowserSearch:
            // case .BrowserFavorites:
            // case .BrowserHome:
            // case .VolumeMute:
            // case .VolumeDown:
            // case .VolumeUp:
            // case .MediaNextTrack:
            // case .MediaPreviousTrack:
            // case .MediaStop:
            // case .MediaPlayPause:
            // case .LaunchMail:
            // case .SelectMedia:
            // case .LaunchApplication1:
            // case .LaunchApplication2:
            // case .OemSemicolon:
            // case .OemPlus:
            // case .OemComma:
            // case .OemMinus:
            // case .OemPeriod:
            // case .OemQuestion:
            // case .OemTilde:
            // case .OemOpenBrackets:
            // case .OemPipe:
            // case .OemCloseBrackets:
            // case .OemQuotes:
            // case .Oem8:
            // case .OemBackslash:
            // case .Play:
            // case .Zoom:
            // case .CancelKey:
            default: nil
            }
            if (cocoaKeyflags == nil) {
                return false
            }
            if let keyCombo = KeyCombo(key: cocoaKeyflags!, cocoaModifiers: NSEvent.ModifierFlags(rawValue:cocoaModflags)) {
                // this (or more precisely any code using .keyEquivalent)
                // needs to run on main thread to avoid hanging forever
                DispatchQueue.main.sync {
                    NSLog("Registering key combo in HotkeyManager... " + keyCombo.keyEquivalentModifierMaskString + keyCombo.keyEquivalent);
                    let hotKey = HotKey(identifier: String(identifier), keyCombo: keyCombo) { hotKey in
                        onHotKeyReceived(str: str, identifier: identifier)
                    }
                    hotKey.register() // or HotKeyCenter.shared.register(with: hotKey)
                }
                return true
            } else {
                return false
            }
        } else {
            return false
        }
    }
    
    private static func onHotKeyReceived(str: UnsafeMutableRawPointer, identifier: UInt) {
        NIBridge.dispatchHotkeyEvent(str, identifier: NSNumber(value:identifier))
    }
}

// converted from .net using some regex magic
private enum Keys : UInt {
    case None = 0x0
        
    /// <summary>
    ///  The BACKSPACE key.
    /// </summary>
    case Backspace = 0x08

    /// <summary>
    ///  The TAB key.
    /// </summary>
    case Tab = 0x09

    /// <summary>
    ///  The CLEAR key.
    /// </summary>
    case LineFeed = 0x0A
    
    /// <summary>
    ///  The CLEAR key.
    /// </summary>
    case Clear = 0x0C

    /// <summary>
    ///  The ENTER key.
    /// </summary>
    case Enter = 0x0D

    /// <summary>
    ///  The SHIFT key.
    /// </summary>
    /// ShiftKey = 0x10

    /// <summary>
    ///  The CTRL key.
    /// </summary>
    /// ControlKey = 0x11,

    /// <summary>
    ///  The ALT key.
    /// </summary>
    /// Menu = 0x12,

    /// <summary>
    ///  The PAUSE key.
    /// </summary>
    case Pause = 0x13

    /// <summary>
    ///  The CAPS LOCK key.
    /// </summary>
    case CapsLock = 0x14
    
    /// <summary>
    ///  The IME Kana mode key.
    /// </summary>
    /// KanaMode = 0x15,

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
    /// JunjaMode = 0x17,

    /// <summary>
    ///  The IME Final mode key.
    /// </summary>
    /// FinalMode = 0x18,

    /// <summary>
    ///  The IME Hanja mode key.
    /// </summary>
    /// HanjaMode = 0x19,

    /// <summary>
    ///  The IME Kanji mode key.
    /// </summary>
    /// KanjiMode = 0x19,

    /// <summary>
    ///  The ESC key.
    /// </summary>
    case Escape = 0x1B
    
    /// <summary>
    ///  The IME Convert key.
    /// </summary>
    /// IMEConvert = 0x1C,

    /// <summary>
    ///  The IME NonConvert key.
    /// </summary>
    /// IMENonconvert = 0x1D,

    /// <summary>
    ///  The IME Accept key.
    /// </summary>
    /// IMEAccept = 0x1E,
    
    /// <summary>
    ///  The IME Mode change request.
    /// </summary>
    /// IMEModeChange = 0x1F,

    /// <summary>
    ///  The SPACEBAR key.
    /// </summary>
    case Space = 0x20
    
    /// <summary>
    ///  The PAGE UP key.
    /// </summary>
    case PageUp = 0x21

    /// <summary>
    ///  The PAGE DOWN key.
    /// </summary>
    case PageDown = 0x22

    /// <summary>
    ///  The END key.
    /// </summary>
    case End = 0x23

    /// <summary>
    ///  The HOME key.
    /// </summary>
    case Home = 0x24

    /// <summary>
    ///  The LEFT ARROW key.
    /// </summary>
    case Left = 0x25

    /// <summary>
    ///  The UP ARROW key.
    /// </summary>
    case Up = 0x26

    /// <summary>
    ///  The RIGHT ARROW key.
    /// </summary>
    case Right = 0x27

    /// <summary>
    ///  The DOWN ARROW key.
    /// </summary>
    case Down = 0x28

    /// <summary>
    ///  The SELECT key.
    /// </summary>
    case Select = 0x29

    /// <summary>
    ///  The PRINT key.
    /// </summary>
    case Print = 0x2A

    /// <summary>
    ///  The EXECUTE key.
    /// </summary>
    case Execute = 0x2B

    /// <summary>
    ///  The PRINT SCREEN key.
    /// </summary>
    case PrintScreen = 0x2C
    
    /// <summary>
    ///  The INS key.
    /// </summary>
    case Insert = 0x2D

    /// <summary>
    ///  The DEL key.
    /// </summary>
    case Delete = 0x2E

    /// <summary>
    ///  The HELP key.
    /// </summary>
    case Help = 0x2F
    
    /// <summary>
    ///  The 0 key.
    /// </summary>
    case Decimal0 = 0x30 // 0

    /// <summary>
    ///  The 1 key.
    /// </summary>
    case Decimal1 = 0x31 // 1

    /// <summary>
    ///  The 2 key.
    /// </summary>
    case Decimal2 = 0x32 // 2

    /// <summary>
    ///  The 3 key.
    /// </summary>
    case Decimal3 = 0x33 // 3

    /// <summary>
    ///  The 4 key.
    /// </summary>
    case Decimal4 = 0x34 // 4

    /// <summary>
    ///  The 5 key.
    /// </summary>
    case Decimal5 = 0x35 // 5

    /// <summary>
    ///  The 6 key.
    /// </summary>
    case Decimal6 = 0x36 // 6

    /// <summary>
    ///  The 7 key.
    /// </summary>
    case Decimal7 = 0x37 // 7

    /// <summary>
    ///  The 8 key.
    /// </summary>
    case Decimal8 = 0x38 // 8

    /// <summary>
    ///  The 9 key.
    /// </summary>
    case Decimal9 = 0x39 // 9

    /// <summary>
    ///  The A key.
    /// </summary>
    case A = 0x41

    /// <summary>
    ///  The B key.
    /// </summary>
    case B = 0x42

    /// <summary>
    ///  The C key.
    /// </summary>
    case C = 0x43

    /// <summary>
    ///  The D key.
    /// </summary>
    case D = 0x44

    /// <summary>
    ///  The E key.
    /// </summary>
    case E = 0x45

    /// <summary>
    ///  The F key.
    /// </summary>
    case F = 0x46

    /// <summary>
    ///  The G key.
    /// </summary>
    case G = 0x47

    /// <summary>
    ///  The H key.
    /// </summary>
    case H = 0x48

    /// <summary>
    ///  The I key.
    /// </summary>
    case I = 0x49

    /// <summary>
    ///  The J key.
    /// </summary>
    case J = 0x4A

    /// <summary>
    ///  The K key.
    /// </summary>
    case K = 0x4B

    /// <summary>
    ///  The L key.
    /// </summary>
    case L = 0x4C

    /// <summary>
    ///  The M key.
    /// </summary>
    case M = 0x4D

    /// <summary>
    ///  The N key.
    /// </summary>
    case N = 0x4E

    /// <summary>
    ///  The O key.
    /// </summary>
    case O = 0x4F

    /// <summary>
    ///  The P key.
    /// </summary>
    case P = 0x50

    /// <summary>
    ///  The Q key.
    /// </summary>
    case Q = 0x51

    /// <summary>
    ///  The R key.
    /// </summary>
    case R = 0x52

    /// <summary>
    ///  The S key.
    /// </summary>
    case S = 0x53

    /// <summary>
    ///  The T key.
    /// </summary>
    case T = 0x54

    /// <summary>
    ///  The U key.
    /// </summary>
    case U = 0x55

    /// <summary>
    ///  The V key.
    /// </summary>
    case V = 0x56

    /// <summary>
    ///  The W key.
    /// </summary>
    case W = 0x57

    /// <summary>
    ///  The X key.
    /// </summary>
    case X = 0x58

    /// <summary>
    ///  The Y key.
    /// </summary>
    case Y = 0x59

    /// <summary>
    ///  The Z key.
    /// </summary>
    case Z = 0x5A
    
    /// <summary>
    ///  The left Windows logo key (Microsoft Natural Keyboard).
    /// </summary>
    /// LWin = 0x5B,

    /// <summary>
    ///  The right Windows logo key (Microsoft Natural Keyboard).
    /// </summary>
    /// RWin = 0x5C,

    /// <summary>
    ///  The Application key (Microsoft Natural Keyboard).
    /// </summary>
    case Apps = 0x5D

    /// <summary>
    ///  The Computer Sleep key.
    /// </summary>
    /// Sleep = 0x5F,

    /// <summary>
    ///  The 0 key on the numeric keypad.
    /// </summary>
    case NumPad0 = 0x60

    /// <summary>
    ///  The 1 key on the numeric keypad.
    /// </summary>
    case NumPad1 = 0x61

    /// <summary>
    ///  The 2 key on the numeric keypad.
    /// </summary>
    case NumPad2 = 0x62

    /// <summary>
    ///  The 3 key on the numeric keypad.
    /// </summary>
    case NumPad3 = 0x63

    /// <summary>
    ///  The 4 key on the numeric keypad.
    /// </summary>
    case NumPad4 = 0x64

    /// <summary>
    ///  The 5 key on the numeric keypad.
    /// </summary>
    case NumPad5 = 0x65

    /// <summary>
    ///  The 6 key on the numeric keypad.
    /// </summary>
    case NumPad6 = 0x66

    /// <summary>
    ///  The 7 key on the numeric keypad.
    /// </summary>
    case NumPad7 = 0x67

    /// <summary>
    ///  The 8 key on the numeric keypad.
    /// </summary>
    case NumPad8 = 0x68

    /// <summary>
    ///  The 9 key on the numeric keypad.
    /// </summary>
    case NumPad9 = 0x69

    /// <summary>
    ///  The Multiply key.
    /// </summary>
    case Multiply = 0x6A

    /// <summary>
    ///  The Add key.
    /// </summary>
    case Add = 0x6B

    /// <summary>
    ///  The Separator key.
    /// </summary>
    case Separator = 0x6C

    /// <summary>
    ///  The Subtract key.
    /// </summary>
    case Subtract = 0x6D

    /// <summary>
    ///  The Decimal key.
    /// </summary>
    case Decimal = 0x6E

    /// <summary>
    ///  The Divide key.
    /// </summary>
    case Divide = 0x6F

    /// <summary>
    ///  The F1 key.
    /// </summary>
    case F1 = 0x70

    /// <summary>
    ///  The F2 key.
    /// </summary>
    case F2 = 0x71

    /// <summary>
    ///  The F3 key.
    /// </summary>
    case F3 = 0x72

    /// <summary>
    ///  The F4 key.
    /// </summary>
    case F4 = 0x73

    /// <summary>
    ///  The F5 key.
    /// </summary>
    case F5 = 0x74

    /// <summary>
    ///  The F6 key.
    /// </summary>
    case F6 = 0x75

    /// <summary>
    ///  The F7 key.
    /// </summary>
    case F7 = 0x76

    /// <summary>
    ///  The F8 key.
    /// </summary>
    case F8 = 0x77

    /// <summary>
    ///  The F9 key.
    /// </summary>
    case F9 = 0x78

    /// <summary>
    ///  The F10 key.
    /// </summary>
    case F10 = 0x79

    /// <summary>
    ///  The F11 key.
    /// </summary>
    case F11 = 0x7A

    /// <summary>
    ///  The F12 key.
    /// </summary>
    case F12 = 0x7B

    /// <summary>
    ///  The F13 key.
    /// </summary>
    case F13 = 0x7C

    /// <summary>
    ///  The F14 key.
    /// </summary>
    case F14 = 0x7D

    /// <summary>
    ///  The F15 key.
    /// </summary>
    case F15 = 0x7E

    /// <summary>
    ///  The F16 key.
    /// </summary>
    case F16 = 0x7F

    /// <summary>
    ///  The F17 key.
    /// </summary>
    case F17 = 0x80

    /// <summary>
    ///  The F18 key.
    /// </summary>
    case F18 = 0x81

    /// <summary>
    ///  The F19 key.
    /// </summary>
    case F19 = 0x82

    /// <summary>
    ///  The F20 key.
    /// </summary>
    case F20 = 0x83

    /// <summary>
    ///  The F21 key.
    /// </summary>
    case F21 = 0x84

    /// <summary>
    ///  The F22 key.
    /// </summary>
    case F22 = 0x85

    /// <summary>
    ///  The F23 key.
    /// </summary>
    case F23 = 0x86

    /// <summary>
    ///  The F24 key.
    /// </summary>
    case F24 = 0x87

    /// <summary>
    ///  The NUM LOCK key.
    /// </summary>
    case NumLock = 0x90

    /// <summary>
    ///  The SCROLL LOCK key.
    /// </summary>
    case ScrollLock = 0x91

    /// <summary>
    ///  The left SHIFT key.
    /// </summary>
    /// LShiftKey = 0xA0,

    /// <summary>
    ///  The right SHIFT key.
    /// </summary>
    /// RShiftKey = 0xA1,

    /// <summary>
    ///  The left CTRL key.
    /// </summary>
    /// LControlKey = 0xA2,

    /// <summary>
    ///  The right CTRL key.
    /// </summary>
    /// RControlKey = 0xA3,

    /// <summary>
    ///  The left ALT key.
    /// </summary>
    /// LMenu = 0xA4,

    /// <summary>
    ///  The right ALT key.
    /// </summary>
    /// RMenu = 0xA5,

    /// <summary>
    ///  The Browser Back key.
    /// </summary>
    case BrowserBack = 0xA6

    /// <summary>
    ///  The Browser Forward key.
    /// </summary>
    case BrowserForward = 0xA7

    /// <summary>
    ///  The Browser Refresh key.
    /// </summary>
    case BrowserRefresh = 0xA8

    /// <summary>
    ///  The Browser Stop key.
    /// </summary>
    case BrowserStop = 0xA9

    /// <summary>
    ///  The Browser Search key.
    /// </summary>
    case BrowserSearch = 0xAA

    /// <summary>
    ///  The Browser Favorites key.
    /// </summary>
    case BrowserFavorites = 0xAB

    /// <summary>
    ///  The Browser Home key.
    /// </summary>
    case BrowserHome = 0xAC

    /// <summary>
    ///  The Volume Mute key.
    /// </summary>
    case VolumeMute = 0xAD

    /// <summary>
    ///  The Volume Down key.
    /// </summary>
    case VolumeDown = 0xAE

    /// <summary>
    ///  The Volume Up key.
    /// </summary>
    case VolumeUp = 0xAF

    /// <summary>
    ///  The Media Next Track key.
    /// </summary>
    case MediaNextTrack = 0xB0

    /// <summary>
    ///  The Media Previous Track key.
    /// </summary>
    case MediaPreviousTrack = 0xB1

    /// <summary>
    ///  The Media Stop key.
    /// </summary>
    case MediaStop = 0xB2

    /// <summary>
    ///  The Media Play Pause key.
    /// </summary>
    case MediaPlayPause = 0xB3

    /// <summary>
    ///  The Launch Mail key.
    /// </summary>
    case LaunchMail = 0xB4

    /// <summary>
    ///  The Select Media key.
    /// </summary>
    case SelectMedia = 0xB5

    /// <summary>
    ///  The Launch Application1 key.
    /// </summary>
    case LaunchApplication1 = 0xB6

    /// <summary>
    ///  The Launch Application2 key.
    /// </summary>
    case LaunchApplication2 = 0xB7

    /// <summary>
    ///  The Oem Semicolon key.
    /// </summary>
    case OemSemicolon = 0xBA
    
    /// <summary>
    ///  The Oem plus key.
    /// </summary>
    case OemPlus = 0xBB

    /// <summary>
    ///  The Oem comma key.
    /// </summary>
    case OemComma = 0xBC

    /// <summary>
    ///  The Oem Minus key.
    /// </summary>
    case OemMinus = 0xBD

    /// <summary>
    ///  The Oem Period key.
    /// </summary>
    case OemPeriod = 0xBE

    /// <summary>
    ///  The Oem Question key.
    /// </summary>
    case OemQuestion = 0xBF
    
    /// <summary>
    ///  The Oem tilde key.
    /// </summary>
    case OemTilde = 0xC0

    /// <summary>
    ///  The Oem Open Brackets key.
    /// </summary>
    case OemOpenBrackets = 0xDB

    /// <summary>
    ///  The Oem Pipe key.
    /// </summary>
    case OemPipe = 0xDC

    /// <summary>
    ///  The Oem Close Brackets key.
    /// </summary>
    case OemCloseBrackets = 0xDD

    /// <summary>
    ///  The Oem Quotes key.
    /// </summary>
    case OemQuotes = 0xDE
    
    /// <summary>
    ///  The Oem8 key.
    /// </summary>
    case Oem8 = 0xDF

    /// <summary>
    ///  The Oem Backslash key.
    /// </summary>
    case OemBackslash = 0xE2

    /// <summary>
    ///  The PLAY key.
    /// </summary>
    case Play = 0xFA

    /// <summary>
    ///  The ZOOM key.
    /// </summary>
    case Zoom = 0xFB

    /// <summary>
    ///  The CANCEL key.
    /// </summary>
    case CancelKey = 0x03
}


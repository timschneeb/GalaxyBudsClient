// ReSharper disable RedundantUsingDirective
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

namespace GalaxyBudsClient.Platform.OSX;

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
        SendMacKeys(keys.Select(key => key.ToKeysEnum()).ToList());
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
            maskCmd = key is not (Keys.LWin or Keys.RWin) && maskCmd;
            maskShft = key is not (Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey) && maskShft;
            maskOpt = key is not (Keys.Menu or Keys.LMenu or Keys.RMenu) && maskOpt;
            maskCtrl = key is not (Keys.ControlKey or Keys.LControlKey or Keys.RControlKey) && maskCtrl;
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
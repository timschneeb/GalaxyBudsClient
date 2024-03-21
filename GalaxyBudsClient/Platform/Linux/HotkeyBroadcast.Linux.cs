using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using GalaxyBudsClient.Model.Hotkeys;
using GalaxyBudsClient.Platform.Interfaces;
using SharpHook;
using SharpHook.Native;

namespace GalaxyBudsClient.Platform.Linux;

public class HotkeyBroadcast : IHotkeyBroadcast
{
    public void SendKeys(IEnumerable<Key> keys)
    {
        SendKeysInternal(keys
            .Select(key => key.ToKeysEnum().ToLinuxKeyCode())
            .Where(key => key != null)
            .Cast<KeyCode>()
            .ToList());
    }

    private static void SendKeysInternal(List<KeyCode> keys)
    {
        var simulator = new EventSimulator();
        foreach (var key in keys)
        {
            simulator.SimulateKeyPress(key);
        }

        keys.Reverse();
            
        foreach (var key in keys)
        {
            simulator.SimulateKeyRelease(key);
        }
    }
}
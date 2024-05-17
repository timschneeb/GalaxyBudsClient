using System.Collections.Generic;
using Avalonia.Input;
using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

#pragma warning disable CS0067

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyHotkeyBroadcast : IHotkeyBroadcast
{
    public void SendKeys(IEnumerable<Key> keys)
    {
        Log.Warning("Dummy.HotkeyBroadcast: Platform not supported");
    }
}
using System.Collections.Generic;
using Avalonia.Input;

namespace GalaxyBudsClient.Platform.Interfaces;

public interface IHotkeyBroadcast
{
    void SendKeys(IEnumerable<Key> keys);
}
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Input;

namespace GalaxyBudsClient.Platform.Interfaces
{
    public interface IHotkeyBroadcast
    {
        void SendKeys(IEnumerable<Key> keys);
    }
}

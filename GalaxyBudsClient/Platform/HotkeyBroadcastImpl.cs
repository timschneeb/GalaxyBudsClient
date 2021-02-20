using System;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform
{
    public static class HotkeyBroadcastImpl
    {
        public static readonly IHotkeyBroadcast Instance;

        static HotkeyBroadcastImpl()
        {
            if (PlatformUtils.IsWindows)
            {
                Instance = new Windows.HotkeyBroadcast();
            }
            else
            {
                Instance = new Dummy.HotkeyBroadcast();
            }
        }
    }
}

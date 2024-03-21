using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform;

public static class HotkeyBroadcast
{
    public static readonly IHotkeyBroadcast Instance;

    static HotkeyBroadcast()
    {
        if (PlatformUtils.IsWindows)
        {
            Instance = new Windows.HotkeyBroadcast();
        }
        else if (PlatformUtils.IsLinux)
        {
            Instance = new Linux.HotkeyBroadcast();
        }  
        else if (PlatformUtils.IsOSX)
        {
            Instance = new OSX.HotkeyBroadcast();
        }
        else
        {
            Instance = new Dummy.HotkeyBroadcast();
        }
    }
}
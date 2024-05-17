using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Platform.Interfaces;
#if Linux
using GalaxyBudsClient.Platform.Linux;
#endif
#if OSX
using GalaxyBudsClient.Platform.OSX;
#endif
#if Windows
using GalaxyBudsClient.Platform.Windows;
using GalaxyBudsClient.Platform.WindowsRT;
#endif
using GalaxyBudsClient.Platform.Stubs;
using Serilog;

namespace GalaxyBudsClient.Platform;

public static class PlatformImpl
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")] 
    public static IPlatformImplCreator Creator = new DummyPlatformImplCreator();

    public static IAutoStartHelper AutoStart { get; }
    public static IHotkeyBroadcast HotkeyBroadcast { get; }
    public static IHotkeyReceiver HotkeyReceiver { get; }
    public static IMediaKeyRemote MediaKeyRemote { get; }

    static PlatformImpl()
    {
#if Windows
        if (PlatformUtils.IsWindows) SwitchWindowsBackend();
#endif
#if Linux
        if (PlatformUtils.IsLinux) Creator = new LinuxPlatformImplCreator();
#endif
#if OSX
        if (PlatformUtils.IsOSX) Creator = new OsxPlatformImplCreator();
#endif
        
        Log.Information("PlatformImpl: Using {Platform}", Creator.GetType().Name);
        
        EventDispatcher.Instance.EventReceived += OnEventReceived;
        
        AutoStart = Creator.CreateAutoStartHelper() ?? new DummyAutoStartHelper();
        HotkeyBroadcast = Creator.CreateHotkeyBroadcast() ?? new DummyHotkeyBroadcast();
        HotkeyReceiver = Creator.CreateHotkeyReceiver() ?? new DummyHotkeyReceiver();
        MediaKeyRemote = Creator.CreateMediaKeyRemote() ?? new DummyMediaKeyRemote();
    }

    public static void SwitchWindowsBackend()
    {
#if Windows
        if (PlatformUtils.IsWindows && Settings.Data.UseBluetoothWinRt
                                    && PlatformUtils.IsWindowsContractsSdkSupported)
        {
            Creator = new WindowsRtPlatformImplCreator();
        }
        else if (PlatformUtils.IsWindows)
        {
            Creator = new WindowsPlatformImplCreator();
        }
#endif
    } 
    
    private static void OnEventReceived(Event e, object? arg)
    {
        switch (e)
        {
            case Event.Play:
                MediaKeyRemote.Play();
                break;
            case Event.Pause:
                MediaKeyRemote.Pause();
                break;
            case Event.TogglePlayPause:
                MediaKeyRemote.PlayPause();
                break;
        }
    }
}
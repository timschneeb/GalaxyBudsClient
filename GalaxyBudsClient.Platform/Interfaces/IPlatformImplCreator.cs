namespace GalaxyBudsClient.Platform.Interfaces;

/**
 * <summary>Implements a factory for platform-specific implementations</summary>
 */
public interface IPlatformImplCreator
{ 
    IDesktopServices? CreateDesktopServices();
    IBluetoothService? CreateBluetoothService();
    IHotkeyBroadcast? CreateHotkeyBroadcast();
    IHotkeyReceiver? CreateHotkeyReceiver();
    IMediaKeyRemote? CreateMediaKeyRemote();
    INotificationListener? CreateNotificationListener();
    IOfficialAppDetector? CreateOfficialAppDetector();
}
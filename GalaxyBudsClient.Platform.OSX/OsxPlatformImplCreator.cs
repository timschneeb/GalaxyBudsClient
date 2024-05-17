using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.OSX;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class OsxPlatformImplCreator : IPlatformImplCreator
{
    public IAutoStartHelper CreateAutoStartHelper() => new AutoStartHelper();
    public IBluetoothService CreateBluetoothService() => new BluetoothService();
    public IHotkeyBroadcast CreateHotkeyBroadcast() => new HotkeyBroadcast();
    public IHotkeyReceiver CreateHotkeyReceiver() => new HotkeyReceiver();
    public IMediaKeyRemote CreateMediaKeyRemote() => new MediaKeyRemote();
    public INotificationListener? CreateNotificationListener() => null;
}
using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Bluetooth.Windows.Bluetooth;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Windows.Impl;

namespace GalaxyBudsClient.Platform.Windows;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class WindowsPlatformImplCreator : IPlatformImplCreator
{
    public IAutoStartHelper CreateAutoStartHelper() => new AutoStartHelper();
    public virtual IBluetoothService? CreateBluetoothService() => new BluetoothService();
    public IHotkeyBroadcast CreateHotkeyBroadcast() => new HotkeyBroadcast();
    public IHotkeyReceiver CreateHotkeyReceiver() => new HotkeyReceiver();
    public IMediaKeyRemote CreateMediaKeyRemote() => new MediaKeyRemote();
    public INotificationListener? CreateNotificationListener() => null;
}
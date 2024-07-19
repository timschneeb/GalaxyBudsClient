using System.Diagnostics.CodeAnalysis;
using Android.Content;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Android.Impl;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AndroidPlatformImplCreator(Context appContext) : IPlatformImplCreator
{
    public IAutoStartHelper? CreateAutoStartHelper() => null;
    public IBluetoothService? CreateBluetoothService() => new BluetoothService(appContext);
    public IHotkeyBroadcast? CreateHotkeyBroadcast() => null;
    public IHotkeyReceiver? CreateHotkeyReceiver() => null;
    public IMediaKeyRemote? CreateMediaKeyRemote() => null;
    public INotificationListener? CreateNotificationListener() => null;
    public IOfficialAppDetector CreateOfficialAppDetector() => new OfficialAppDetector(appContext);
}
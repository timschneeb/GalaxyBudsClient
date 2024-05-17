using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Platform.Interfaces;
using GalaxyBudsClient.Platform.Windows;

namespace GalaxyBudsClient.Platform.WindowsRT;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class WindowsRtPlatformImplCreator : WindowsPlatformImplCreator
{
    public override IBluetoothService? CreateBluetoothService() => new BluetoothService();
}
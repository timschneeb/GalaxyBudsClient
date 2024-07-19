using System.Diagnostics.CodeAnalysis;

namespace GalaxyBudsClient.Platform.OSX;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class DesktopServices : BaseDesktopServices
{
    public override bool IsAutoStartEnabled
    {
        set => AppUtils.setAutoStartEnabled(value);
        get => AppUtils.isAutoStartEnabled();
    }
}
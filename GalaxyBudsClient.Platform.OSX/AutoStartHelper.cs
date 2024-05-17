using System.Diagnostics.CodeAnalysis;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.OSX;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class AutoStartHelper : IAutoStartHelper
{
    public bool Enabled
    {
        set => AppUtils.setAutoStartEnabled(value);
        get => AppUtils.isAutoStartEnabled();
    }
}
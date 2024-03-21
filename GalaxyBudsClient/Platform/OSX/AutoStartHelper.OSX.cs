// ReSharper disable RedundantUsingDirective
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.OSX;

public class AutoStartHelper : IAutoStartHelper
{
    public bool Enabled
    {
        set
        {
#if OSX
            ThePBone.OSX.Native.Unmanaged.AppUtils.setAutoStartEnabled(value);
#endif
        }
        get
        {
#if OSX
            return ThePBone.OSX.Native.Unmanaged.AppUtils.isAutoStartEnabled();
#else
            return false;
#endif
        }
    }
}
using System;
using Serilog;

namespace GalaxyBudsClient.Platform
{
    public static class AutoStartImpl
    {
        public static IAutoStartHelper Instance;
        
        static AutoStartImpl()
        {
            if (PlatformUtils.IsWindows)
            {
                Instance = new Windows.AutoStartHelper();
            }
            else if (PlatformUtils.IsLinux)
            {
                Instance = new Linux.AutoStartHelper();
            }
            else
            {
                Log.Warning("AutoStartHelper.Dummy: Platform not supported");
                Instance = new Dummy.AutoStartHelper();
            }
        }
    }
}
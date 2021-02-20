using System;

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
                throw new PlatformNotSupportedException();
            }
        }
    }
}
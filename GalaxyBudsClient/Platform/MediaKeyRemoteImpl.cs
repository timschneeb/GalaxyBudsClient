using System;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform
{
    class MediaKeyRemoteImpl
    {
        public static IMediaKeyRemote Instance;

        static MediaKeyRemoteImpl()
        {
            if (PlatformUtils.IsWindows)
            {
                Instance = new Windows.MediaKeyRemote();
            }
            else if (PlatformUtils.IsLinux)
            {
                Instance = new Linux.MediaKeyRemote();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}

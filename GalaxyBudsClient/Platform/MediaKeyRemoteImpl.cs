using System;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform
{
    public static class MediaKeyRemoteImpl
    {
        public static readonly IMediaKeyRemote Instance;

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

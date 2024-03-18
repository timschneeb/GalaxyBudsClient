using GalaxyBudsClient.Model;
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
#if Linux
            else if (PlatformUtils.IsLinux)
            {
                Instance = new Linux.MediaKeyRemote();
            }
#endif
            else if (PlatformUtils.IsOSX)
            {
                Instance = new OSX.MediaKeyRemote();
            }
            else
            {
                Instance = new Dummy.MediaKeyRemote();
            }
            
            EventDispatcher.Instance.EventReceived += OnEventReceived;
        }
        
        public static void Init(){}

        private static void OnEventReceived(Event e, object? arg)
        {
            switch (e)
            {
                case Event.Play:
                    Instance.Play();
                    break;
                case Event.Pause:
                    Instance.Pause();
                    break;
                case Event.TogglePlayPause:
                    Instance.PlayPause();
                    break;
            }
        }
    }
}

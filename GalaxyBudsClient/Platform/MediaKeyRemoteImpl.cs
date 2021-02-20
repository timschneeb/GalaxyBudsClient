using System;
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
            else if (PlatformUtils.IsLinux)
            {
                Instance = new Linux.MediaKeyRemote();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            
            EventDispatcher.Instance.EventReceived += OnEventReceived;
        }
        
        public static void Init(){}

        private static void OnEventReceived(EventDispatcher.Event e, object? arg)
        {
            switch (e)
            {
                case EventDispatcher.Event.Play:
                    Instance.Play();
                    break;
                case EventDispatcher.Event.Pause:
                    Instance.Play();
                    break;
                case EventDispatcher.Event.TogglePlayPause:
                    Instance.PlayPause();
                    break;
            }
        }
    }
}

// ReSharper disable RedundantUsingDirective
using Avalonia.Input;
using GalaxyBudsClient.Platform.Interfaces;
#if OSX
using ThePBone.OSX.Native.Unmanaged;
#endif

namespace GalaxyBudsClient.Platform.OSX
{
    public class MediaKeyRemote : IMediaKeyRemote
    {
        public void Play()
        {
#if OSX
            AppUtils.sendMagicMediaCmd(true);
#endif
        }

        public void Pause()
        {
#if OSX
            AppUtils.sendMagicMediaCmd(false);
#endif
        }

        public void PlayPause()
        {
            HotkeyBroadcastImpl.Instance.SendKeys(new []{Key.MediaPlayPause});
        }
    }
}
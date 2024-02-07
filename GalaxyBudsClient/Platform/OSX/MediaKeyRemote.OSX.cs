using Avalonia.Input;
using GalaxyBudsClient.Platform.Interfaces;
using ThePBone.OSX.Native.Unmanaged;

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
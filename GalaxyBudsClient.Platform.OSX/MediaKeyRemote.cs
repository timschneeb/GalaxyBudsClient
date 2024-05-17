using System.Diagnostics.CodeAnalysis;
using Avalonia.Input;
using GalaxyBudsClient.Platform.Interfaces;

namespace GalaxyBudsClient.Platform.OSX;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class MediaKeyRemote : IMediaKeyRemote
{
    private static readonly HotkeyBroadcast HotkeyBroadcast = new();
    
    public void Play()
    {
        AppUtils.sendMagicMediaCmd(true);
    }

    public void Pause()
    {
        AppUtils.sendMagicMediaCmd(false);
    }

    public void PlayPause()
    {
        HotkeyBroadcast.SendKeys(new []{ Key.MediaPlayPause });
    }
}
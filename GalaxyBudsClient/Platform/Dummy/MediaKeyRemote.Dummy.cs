using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

namespace GalaxyBudsClient.Platform.Dummy;

public class MediaKeyRemote : IMediaKeyRemote
{
    public MediaKeyRemote()
    {
        Log.Warning("MediaKeyRemote.Dummy: Dummy module without functionality selected");
    }
        
    public void Play()
    {
        Log.Warning("MediaKeyRemote.Dummy: Play()");
    }

    public void Pause()
    {
        Log.Warning("MediaKeyRemote.Dummy: Pause()");
    }

    public void PlayPause()
    {
        Log.Warning("MediaKeyRemote.Dummy: PlayPause()");
    }
}
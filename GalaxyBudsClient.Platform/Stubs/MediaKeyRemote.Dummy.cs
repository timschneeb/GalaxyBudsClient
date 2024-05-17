using GalaxyBudsClient.Platform.Interfaces;
using Serilog;

namespace GalaxyBudsClient.Platform.Stubs;

public class DummyMediaKeyRemote : IMediaKeyRemote
{
    public DummyMediaKeyRemote()
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
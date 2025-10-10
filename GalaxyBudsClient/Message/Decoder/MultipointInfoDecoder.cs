using System;
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.MULTIPOINT_INFO)]
public class MultipointInfoDecoder : BaseMessageDecoder
{
    public enum AudioFocusStates
    {
        AudioFocusForeground = 0,
        AudioFocusBackground = 1
    }
    
    public enum MultipointStates
    {
        HaveDevices = 0,
        HaveOneDevice = 1
    }
    
    [Flags]
    public enum StreamingMaskFlags
    {
        A2dpMusicNotCall = 1,
        HfpCall = 2,
        CisMusicNotCall = 4,
        Bis = 8,
        CisCall = 16
    }

    public byte Revision { get; }
    public bool SupportMultipoint { get; }
    public MultipointStates MultipointState { get; }
    public AudioFocusStates AudioFocusState { get; }
    public StreamingMaskFlags StreamingMask { get; }

    public MultipointInfoDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream); 
        
        Revision = SafeReadByte(reader);
        SupportMultipoint = SafeReadByte(reader) == 1;
        MultipointState = (MultipointStates)(SafeReadByte(reader) == 1 ? 1 : 0);
        AudioFocusState = (AudioFocusStates)(SafeReadByte(reader) == 1 ? 1 : 0);
        StreamingMask = (StreamingMaskFlags)SafeReadByte(reader);
    }
}
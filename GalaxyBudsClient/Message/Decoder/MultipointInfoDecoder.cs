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

    public byte Revision { set; get; }
    public bool SupportMultipoint { set; get; }
    public MultipointStates MultipointState { set; get; }
    public AudioFocusStates AudioFocusState { set; get; }
    public StreamingMaskFlags StreamingMask { set; get; }

    public override void Decode(SppMessage msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream); 
        
        Revision = reader.ReadByte();
        SupportMultipoint = reader.ReadByte() == 1;
        MultipointState = (MultipointStates)(reader.ReadByte() == 1 ? 1 : 0);
        AudioFocusState = (AudioFocusStates)(reader.ReadByte() == 1 ? 1 : 0);
        StreamingMask = (StreamingMaskFlags)reader.ReadByte();
    }
}
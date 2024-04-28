using System.IO;

namespace GalaxyBudsClient.Message.Parameter;

// TODO handle in FindMyBuds view model
public class MuteEarbudAckParameter : MessageAsDictionary, IAckParameter
{
    public bool LeftMute { get; }
    public bool RightMute { get; }
    
    public MuteEarbudAckParameter(BinaryReader reader)
    {
        LeftMute = reader.ReadBoolean();
        RightMute = reader.ReadBoolean();
    }
}
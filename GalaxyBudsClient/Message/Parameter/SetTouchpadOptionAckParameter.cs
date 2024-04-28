using System.IO;

namespace GalaxyBudsClient.Message.Parameter;

public class SetTouchpadOptionAckParameter : MessageAsDictionary, IAckParameter
{
    public byte LeftOption { get; }
    public byte RightOption { get; }
    
    public SetTouchpadOptionAckParameter(BinaryReader reader)
    {
        LeftOption = reader.ReadByte();
        RightOption = reader.ReadByte();
    }
}
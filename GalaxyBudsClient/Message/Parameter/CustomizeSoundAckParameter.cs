using System.IO;

namespace GalaxyBudsClient.Message.Parameter;

public class CustomizeSoundAckParameter : MessageAsDictionary, IAckParameter
{
    public bool CustomizeSoundOn { get; }
    public byte LeftLevel { get; }
    public byte RightLevel { get; }
    public byte Tone { get; }
    public bool ConversationBoost { get; }
    public byte NoiseReductionLevel { get; }

    public CustomizeSoundAckParameter(BinaryReader reader)
    {
        try {
            CustomizeSoundOn = reader.ReadBoolean();
            LeftLevel = reader.ReadByte();
            RightLevel = reader.ReadByte();
            Tone = reader.ReadByte();
            ConversationBoost = reader.ReadBoolean();
            NoiseReductionLevel = reader.ReadByte();
        }
        catch (EndOfStreamException) {
        }
    }
}
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_GENERIC_EVENT)]
public class GenericEventDecoder : BaseMessageDecoder
{
    public Devices Device { set; get; }
    public ushort Timestamp { set; get; }
    // Other fields are unknown

    public override void Decode(SppMessage msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Device = reader.ReadChar() == 'L' ? Devices.L : Devices.R;
        Timestamp = reader.ReadUInt16();
    }
}
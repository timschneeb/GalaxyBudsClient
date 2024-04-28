using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_GENERIC_EVENT)]
public class GenericEventDecoder : BaseMessageDecoder
{
    public Devices Device { get; }
    public ushort Timestamp { get; }
    // Other fields are unknown

    public GenericEventDecoder(SppMessage msg) : base(msg)
    {
        using var stream = new MemoryStream(msg.Payload);
        using var reader = new BinaryReader(stream);
        
        Device = reader.ReadChar() == 'L' ? Devices.L : Devices.R;
        Timestamp = reader.ReadUInt16();
    }
}
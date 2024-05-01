using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SET_FMM_CONFIG)]
public class SetFmmConfigDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public byte Response { get; } = msg.Payload.Length > 0 ? msg.Payload[0] : (byte)0xFF;
}
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_OPEN)]
internal class FotaSessionDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public byte ResultCode { get; } = msg.Payload[0];
}
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.RESET)]
internal class ResetResponseDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public int ResultCode { get; } = msg.Payload[0];
}
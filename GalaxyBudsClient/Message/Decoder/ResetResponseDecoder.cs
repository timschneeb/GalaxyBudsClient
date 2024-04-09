using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.RESET)]
internal class ResetResponseDecoder : BaseMessageDecoder
{
    public int ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        ResultCode = msg.Payload[0];
    }
}
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_RESULT)]
internal class FotaResultDecoder : BaseMessageDecoder
{
    public byte Result { set; get; }
    public byte ErrorCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        Result = msg.Payload[0];
        ErrorCode = msg.Payload[1];
    }
}
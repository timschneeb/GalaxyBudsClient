using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_OPEN)]
internal class FotaSessionDecoder : BaseMessageDecoder
{
    public byte ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        ResultCode = msg.Payload[0];
    }
}
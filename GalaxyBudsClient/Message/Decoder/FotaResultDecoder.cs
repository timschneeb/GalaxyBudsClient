using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_RESULT)]
internal class FotaResultDecoder : BaseMessageDecoder
{
    public byte Result { get; }
    public byte ErrorCode { get; }

    public FotaResultDecoder(SppMessage msg) : base(msg)
    {
        Result = msg.Payload[0];
        ErrorCode = msg.Payload[1];
    }
}
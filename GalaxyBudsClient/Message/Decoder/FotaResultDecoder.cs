namespace GalaxyBudsClient.Message.Decoder;

internal class FotaResultDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.FOTA_RESULT;

    public byte Result { set; get; }
    public byte ErrorCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Result = msg.Payload[0];
        ErrorCode = msg.Payload[1];
    }
}
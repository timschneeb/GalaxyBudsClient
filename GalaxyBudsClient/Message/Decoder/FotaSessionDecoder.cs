namespace GalaxyBudsClient.Message.Decoder;

internal class FotaSessionDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.FOTA_OPEN;

    public byte ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
    }
}
namespace GalaxyBudsClient.Message.Decoder;

internal class FotaSessionParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.FOTA_OPEN;

    public byte ResultCode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
    }
}
namespace GalaxyBudsClient.Message.Decoder;

internal class ResetResponseParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.RESET;

    public int ResultCode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
    }
}
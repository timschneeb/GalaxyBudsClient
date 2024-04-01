namespace GalaxyBudsClient.Message.Decoder;

internal class ResetResponseParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.RESET;

    public int ResultCode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
    }
}
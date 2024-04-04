namespace GalaxyBudsClient.Message.Decoder;

internal class ResetResponseDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.RESET;

    public int ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = msg.Payload[0];
    }
}
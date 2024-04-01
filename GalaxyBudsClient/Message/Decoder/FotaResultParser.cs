namespace GalaxyBudsClient.Message.Decoder;

internal class FotaResultParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.FOTA_RESULT;

    public byte Result { set; get; }
    public byte ErrorCode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Result = msg.Payload[0];
        ErrorCode = msg.Payload[1];
    }
}
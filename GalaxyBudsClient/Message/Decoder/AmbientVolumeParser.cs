namespace GalaxyBudsClient.Message.Decoder;

internal class AmbientVolumeParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.AMBIENT_VOLUME;
    public int AmbientVolume { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        AmbientVolume = msg.Payload[0];
    }
}
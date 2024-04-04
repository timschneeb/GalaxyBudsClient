namespace GalaxyBudsClient.Message.Decoder;

internal class AmbientVolumeDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.AMBIENT_VOLUME;
    public int AmbientVolume { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        AmbientVolume = msg.Payload[0];
    }
}
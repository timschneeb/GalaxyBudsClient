using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class SpatialAudioControlParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.SPATIAL_AUDIO_CONTROL;
    public SpatialAudioControl ResultCode { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = (SpatialAudioControl) msg.Payload[0];
    }
}
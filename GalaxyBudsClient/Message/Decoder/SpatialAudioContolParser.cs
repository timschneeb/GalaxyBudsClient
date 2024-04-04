using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class SpatialAudioControlDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.SPATIAL_AUDIO_CONTROL;
    public SpatialAudioControl ResultCode { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        ResultCode = (SpatialAudioControl) msg.Payload[0];
    }
}
using System;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

/*
 * Mostly unused if (versionOfMR < 2). Refer to ExtendedStatusUpdateDecoder
 */
[MessageDecoder((MsgIds)LegacyMsgIds.AMBIENT_WEARING_STATUS_UPDATED)]
internal class AmbientWearingUpdateDecoder : BaseMessageDecoder
{
    public LegacyWearStates WearState { get; }
    public int LeftDetectionCount { get; }
    public int RightDetectionCount { get; }

    public AmbientWearingUpdateDecoder(SppMessage msg) : base(msg)
    {
        WearState = (LegacyWearStates) msg.Payload[0];
        LeftDetectionCount = BitConverter.ToInt16(msg.Payload, 1);
        RightDetectionCount = BitConverter.ToInt16(msg.Payload, 3);
    }
}
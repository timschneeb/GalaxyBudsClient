using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.MUTE_EARBUD_STATUS_UPDATED)]
public class MuteUpdateDecoder : BaseMessageDecoder
{
    public bool LeftMuted { get; }
    public bool RightMuted { get; }

    public MuteUpdateDecoder(SppMessage msg) : base(msg)
    {
        LeftMuted = Convert.ToBoolean(msg.Payload[0]);
        RightMuted = Convert.ToBoolean(msg.Payload[1]);
    }
}
using System;

namespace GalaxyBudsClient.Message.Encoder;

public class FmgMuteEarbudEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.MUTE_EARBUD;
    public bool IsLeftMuted { get; init; }
    public bool IsRightMuted { get; init; }
    
    public override SppMessage Encode()
    {
        return new SppMessage(HandledType, MsgTypes.Request, [
            Convert.ToByte(IsLeftMuted),
            Convert.ToByte(IsRightMuted)
        ]);
    }
}
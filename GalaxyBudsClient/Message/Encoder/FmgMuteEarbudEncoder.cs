using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.MUTE_EARBUD)]
public class FmgMuteEarbudEncoder : BaseMessageEncoder
{
    public bool IsLeftMuted { get; init; }
    public bool IsRightMuted { get; init; }
    
    public override SppMessage Encode()
    {
        return new SppMessage(MsgIds.MUTE_EARBUD, MsgTypes.Request, [
            Convert.ToByte(IsLeftMuted),
            Convert.ToByte(IsRightMuted)
        ]);
    }
}
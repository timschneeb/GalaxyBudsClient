using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.SET_TOUCHPAD_OPTION)]
public class SetTouchOptionsEncoder : BaseMessageEncoder
{
    public TouchOptions LeftAction { get; set; }
    public TouchOptions RightAction { get; set; }
    
    public override SppMessage Encode()
    {
        return new SppMessage(MsgIds.SET_TOUCHPAD_OPTION, MsgTypes.Request, [
            DeviceSpec.TouchMap.ToByte(LeftAction),
            DeviceSpec.TouchMap.ToByte(RightAction)
        ]);
    }
}
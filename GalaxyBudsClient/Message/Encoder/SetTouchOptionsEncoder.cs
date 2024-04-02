using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Encoder;

public class SetTouchOptionsEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.SET_TOUCHPAD_OPTION;
    public TouchOptions LeftAction { get; set; }
    public TouchOptions RightAction { get; set; }
    
    public override SppMessage Encode()
    {
        return new SppMessage(HandledType, MsgTypes.Request, [
            DeviceSpec.TouchMap.ToByte(LeftAction),
            DeviceSpec.TouchMap.ToByte(RightAction)
        ]);
    }
}
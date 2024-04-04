using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class SetOtherOptionDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.SET_TOUCHPAD_OTHER_OPTION;
    public TouchOptions OptionType { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        OptionType = DeviceSpec.TouchMap.FromByte(msg.Payload[0]);
    }
}
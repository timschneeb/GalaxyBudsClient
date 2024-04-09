using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SET_TOUCHPAD_OTHER_OPTION)]
internal class SetOtherOptionDecoder : BaseMessageDecoder
{
    public TouchOptions OptionType { set; get; }

    public override void Decode(SppMessage msg)
    {
        OptionType = DeviceSpec.TouchMap.FromByte(msg.Payload[0]);
    }
}
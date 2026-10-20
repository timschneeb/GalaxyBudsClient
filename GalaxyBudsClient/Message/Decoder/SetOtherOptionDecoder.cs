using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SET_TOUCHPAD_OTHER_OPTION)]
internal class SetOtherOptionDecoder : BaseMessageDecoder
{
    public TouchOptions OptionType { get; }

    public SetOtherOptionDecoder(SppMessage msg) : base(msg)
    {
        // Buds3 Pro reports the side (1 = left, 0 = right), not the configured action.
        OptionType = (TargetModel, msg.Payload[0]) switch
        {
            (Models.Buds3Pro, 1) => TouchOptions.OtherL,
            (Models.Buds3Pro, 0) => TouchOptions.OtherR,
            _ => DeviceSpec.TouchMap.FromByte(msg.Payload[0])
        };
    }
}
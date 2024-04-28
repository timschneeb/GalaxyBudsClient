using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder((MsgIds)LegacyMsgIds.SPP_ROLE_STATE)]
internal class SppRoleStateDecoder : BaseMessageDecoder
{
    public Devices Device { get; }
    public SppRoleStates SppRoleState { get; }

    public SppRoleStateDecoder(SppMessage msg) : base(msg)
    {
        SppRoleState = (SppRoleStates) msg.Payload[0];
        Device = (Devices) msg.Payload[1];
    }
}
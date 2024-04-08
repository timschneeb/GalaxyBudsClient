using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class SppRoleStateDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => (MsgIds)LegacyMsgIds.SPP_ROLE_STATE;

    public Devices Device { set; get; }
    public SppRoleStates SppRoleState { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        SppRoleState = (SppRoleStates) msg.Payload[0];
        Device = (Devices) msg.Payload[1];
    }
}
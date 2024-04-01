using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder;

internal class SppRoleStateParser : BaseMessageParser
{
    public override MsgIds HandledType => (MsgIds)115; //SPPMessage.MessageIds.SPP_ROLE_STATE;

    public Devices Device { set; get; }
    public SppRoleStates SppRoleState { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        SppRoleState = (SppRoleStates) msg.Payload[0];
        Device = (Devices) msg.Payload[1];
    }
}
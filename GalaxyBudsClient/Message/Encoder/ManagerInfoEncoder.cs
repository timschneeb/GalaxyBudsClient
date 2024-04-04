using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Encoder;

public class ManagerInfoEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.MANAGER_INFO;
    public ClientDeviceTypes Type { get; init; } = ClientDeviceTypes.Samsung;
    public int AndroidSdkVersion { get; init; } = 34;
    
    public override SppMessage Encode()
    {
        return new SppMessage(HandledType, MsgTypes.Request, [
            1,
            (byte) Type,
            (byte) AndroidSdkVersion
        ]);
    }
}
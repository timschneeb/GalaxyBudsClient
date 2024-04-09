using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.MANAGER_INFO)]
public class ManagerInfoEncoder : BaseMessageEncoder
{
    public ClientDeviceTypes Type { get; init; } = ClientDeviceTypes.Samsung;
    public int AndroidSdkVersion { get; init; } = 34;
    
    public override SppMessage Encode()
    {
        return new SppMessage(MsgIds.MANAGER_INFO, MsgTypes.Request, [
            1,
            (byte) Type,
            (byte) AndroidSdkVersion
        ]);
    }
}
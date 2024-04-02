using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Encoder;

public class ManagerInfoEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.MANAGER_INFO;
    public ClientDeviceTypes Type { get; set; } = ClientDeviceTypes.Samsung;
    public int AndroidSdkVersion { get; set; } = 34;
    
    public override SppMessage Encode()
    {
        return new SppMessage(HandledType, MsgTypes.Request, [
            1,
            (byte) Type,
            (byte) AndroidSdkVersion
        ]);
    }
}
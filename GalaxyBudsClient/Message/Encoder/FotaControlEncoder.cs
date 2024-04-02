using System.IO;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Encoder;

public class FotaControlEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.FOTA_CONTROL;
    public FirmwareConstants.ControlIds ControlId { get; init; }
    public short Parameter { get; set; }
    
    public override SppMessage Encode()
    {
        using var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        if (ControlId == FirmwareConstants.ControlIds.SendMtu)
        {
            Parameter = Parameter > 650 ? (short)650 : Parameter;
        }
            
        writer.Write((byte) ControlId);
        writer.Write(Parameter); // 16-bit parameter
            
        return new SppMessage(HandledType, MsgTypes.Response, stream.ToArray());
    }
}
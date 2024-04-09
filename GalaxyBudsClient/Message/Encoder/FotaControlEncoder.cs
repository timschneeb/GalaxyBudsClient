using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.FOTA_CONTROL)]
public class FotaControlEncoder : BaseMessageEncoder
{
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
            
        return new SppMessage(MsgIds.FOTA_CONTROL, MsgTypes.Response, stream.ToArray());
    }
}
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_DEVICE_INFO_SW_VERSION)]
internal class SoftwareVersionOtaDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public string? SoftwareVersion { get; } = Encoding.ASCII.GetString(msg.Payload);
}
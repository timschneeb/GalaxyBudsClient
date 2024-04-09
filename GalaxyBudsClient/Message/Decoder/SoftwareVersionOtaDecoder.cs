using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_DEVICE_INFO_SW_VERSION)]
internal class SoftwareVersionOtaDecoder : BaseMessageDecoder
{
    public string? SoftwareVersion { set; get; }

    public override void Decode(SppMessage msg)
    {
        SoftwareVersion = Encoding.ASCII.GetString(msg.Payload);
    }
}
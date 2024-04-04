using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

internal class SoftwareVersionOtaDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.FOTA_DEVICE_INFO_SW_VERSION;

    public string? SoftwareVersion { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        SoftwareVersion = Encoding.ASCII.GetString(msg.Payload);
    }
}
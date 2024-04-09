using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.FOTA_DOWNLOAD_DATA)]
internal class FotaDownloadDataDecoder : BaseMessageDecoder
{
    public bool Nak { set; get; }
    public long ReceivedOffset { set; get; }
    public byte RequestPacketNumber { set; get; }

    public override void Decode(SppMessage msg)
    {
        Nak = (msg.Payload[3] & -128) != 0;
        ReceivedOffset = (((long) msg.Payload[1] & 255) << 8) | ((long) msg.Payload[0] & 255) |
                         (((long) msg.Payload[2] & 255) << 16) | (((long) msg.Payload[3] & 127) << 24);
        RequestPacketNumber = msg.Payload[4];
    }
}
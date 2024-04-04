using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

internal class BondedDevicesDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.UNK_BONDED_DEVICES;

    public string? Content { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        Content = Encoding.ASCII.GetString(msg.Payload);
    }
}
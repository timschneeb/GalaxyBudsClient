using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_BONDED_DEVICES)]
internal class BondedDevicesDecoder : BaseMessageDecoder
{
    public string? Content { set; get; }

    public override void Decode(SppMessage msg)
    {
        Content = Encoding.ASCII.GetString(msg.Payload);
    }
}
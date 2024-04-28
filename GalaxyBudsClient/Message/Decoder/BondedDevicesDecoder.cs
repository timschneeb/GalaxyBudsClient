using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.UNK_BONDED_DEVICES)]
internal class BondedDevicesDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public string? Content { get; } = Encoding.ASCII.GetString(msg.Payload);
}
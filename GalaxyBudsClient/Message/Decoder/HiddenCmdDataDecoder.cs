using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.HIDDEN_CMD_DATA)]
internal class HiddenCmdDataDecoder : BaseMessageDecoder
{
    public string? Content { set; get; }

    public override void Decode(SppMessage msg)
    {
        Content = Encoding.ASCII.GetString(msg.Payload);
    }
}
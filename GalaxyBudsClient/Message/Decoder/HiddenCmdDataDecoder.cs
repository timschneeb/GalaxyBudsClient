using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.HIDDEN_CMD_DATA)]
public class HiddenCmdDataDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public string? Content { get; } = Encoding.ASCII.GetString(msg.Payload);
}
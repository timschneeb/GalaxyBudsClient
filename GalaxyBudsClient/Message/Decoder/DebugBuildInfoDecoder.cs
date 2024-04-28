using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_BUILD_INFO)]
internal class DebugBuildInfoDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public string? BuildString { get; } = Encoding.ASCII.GetString(msg.Payload);
}
using System.Text;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.DEBUG_BUILD_INFO)]
internal class DebugBuildInfoDecoder : BaseMessageDecoder
{
    public string? BuildString { set; get; }

    public override void Decode(SppMessage msg)
    {
        BuildString = Encoding.ASCII.GetString(msg.Payload);
    }
}
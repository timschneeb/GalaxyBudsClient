using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

internal class DebugBuildInfoDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.DEBUG_BUILD_INFO;
        
    public string? BuildString { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        BuildString = Encoding.ASCII.GetString(msg.Payload);
    }
}
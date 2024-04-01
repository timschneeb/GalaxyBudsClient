using System.Text;

namespace GalaxyBudsClient.Message.Decoder;

internal class DebugBuildInfoParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.DEBUG_BUILD_INFO;
        
    public string? BuildString { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        BuildString = Encoding.ASCII.GetString(msg.Payload);
    }
}
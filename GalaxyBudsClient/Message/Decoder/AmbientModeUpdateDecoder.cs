using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;
 
[MessageDecoder(MsgIds.AMBIENT_MODE_UPDATED)]
internal class AmbientModeUpdateDecoder(SppMessage msg) : BaseMessageDecoder(msg)
{
    public bool Enabled { get; } = msg.Payload[0] == 1;
}
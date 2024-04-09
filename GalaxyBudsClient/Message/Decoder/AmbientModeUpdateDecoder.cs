using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;
 
[MessageDecoder(MsgIds.AMBIENT_MODE_UPDATED)]
internal class AmbientModeUpdateDecoder : BaseMessageDecoder
{
    public bool Enabled { set; get; }

    public override void Decode(SppMessage msg)
    {
        Enabled = Convert.ToBoolean(msg.Payload[0]);
    }
}
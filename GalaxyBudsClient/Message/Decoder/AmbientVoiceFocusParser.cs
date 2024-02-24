using System;

namespace GalaxyBudsClient.Message.Decoder
{
    class AmbientVoiceFocusParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.AMBIENT_VOICE_FOCUS;
        public bool VoiceFocusEnabled { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            VoiceFocusEnabled = Convert.ToBoolean(msg.Payload[0]);
        }
    }
}

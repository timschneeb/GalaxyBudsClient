using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class SocBatteryCycleParser : BaseMessageParser
{
    public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.SOC_BATTERY_CYCLE;
        
    public long LeftCycles { set; get; }
    public long RightCycles { set; get; }

    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;
        
        var payload = msg.Payload.AsSpan();
        LeftCycles = BitConverter.ToInt64(payload[0..7]);
        RightCycles = BitConverter.ToInt64(payload[8..15]);
    }
}
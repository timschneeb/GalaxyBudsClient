using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class SocBatteryCycleParser : BaseMessageParser
{
    public override MsgIds HandledType => MsgIds.SOC_BATTERY_CYCLE;
        
    public long LeftCycles { set; get; }
    public long RightCycles { set; get; }
    
    public bool IsLeftBatteryBad { set; get; }
    public bool IsRightBatteryBad { set; get; }
    
    public override void ParseMessage(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;
        
        LeftCycles = BitConverter.ToInt64(msg.Payload, 0);
        RightCycles = BitConverter.ToInt64(msg.Payload, 8);
        
        IsLeftBatteryBad = LeftCycles / 10000 >= 150;
        IsRightBatteryBad = RightCycles / 10000 >= 150;
    }
}
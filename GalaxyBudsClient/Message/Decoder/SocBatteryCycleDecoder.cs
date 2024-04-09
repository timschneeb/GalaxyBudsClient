using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SOC_BATTERY_CYCLE)]
internal class SocBatteryCycleDecoder : BaseMessageDecoder
{
    public long LeftCycles { set; get; }
    public long RightCycles { set; get; }
    
    public bool IsLeftBatteryBad { set; get; }
    public bool IsRightBatteryBad { set; get; }
    
    public override void Decode(SppMessage msg)
    {
        LeftCycles = BitConverter.ToInt64(msg.Payload, 0);
        RightCycles = BitConverter.ToInt64(msg.Payload, 8);
        
        IsLeftBatteryBad = LeftCycles / 10000 >= 150;
        IsRightBatteryBad = RightCycles / 10000 >= 150;
    }
}
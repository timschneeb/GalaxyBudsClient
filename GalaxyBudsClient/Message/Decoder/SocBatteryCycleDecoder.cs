using System;
using System.Linq;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SOC_BATTERY_CYCLE)]
internal class SocBatteryCycleDecoder : BaseMessageDecoder
{
    public long LeftCycles { get; }
    public long RightCycles { get; }
    
    public bool IsLeftBatteryBad { get; }
    public bool IsRightBatteryBad { get; }
    
    public SocBatteryCycleDecoder(SppMessage msg) : base(msg)
    {
        LeftCycles = BitConverter.ToInt64(msg.Payload.Take(8).Reverse().ToArray()) / 10000;
        RightCycles = BitConverter.ToInt64(msg.Payload.TakeLast(8).Reverse().ToArray()) / 10000;
        
        IsLeftBatteryBad = LeftCycles >= 150;
        IsRightBatteryBad = RightCycles >= 150;
    }
}
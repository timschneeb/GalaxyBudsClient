using System;
using System.Linq;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SOC_BATTERY_CYCLE)]
internal class SocBatteryCycleDecoder : BaseMessageDecoder
{
    public ulong LeftCycles { set; get; }
    public ulong RightCycles { set; get; }
    
    public bool IsLeftBatteryBad { set; get; }
    public bool IsRightBatteryBad { set; get; }
    
    public override void Decode(SppMessage msg)
    {
        LeftCycles = BitConverter.ToUInt64(msg.Payload.Take(8).Reverse().ToArray());
        RightCycles = BitConverter.ToUInt64(msg.Payload.TakeLast(8).Reverse().ToArray());
        
        IsLeftBatteryBad = LeftCycles / 10000 >= 150;
        IsRightBatteryBad = RightCycles / 10000 >= 150;
    }
}
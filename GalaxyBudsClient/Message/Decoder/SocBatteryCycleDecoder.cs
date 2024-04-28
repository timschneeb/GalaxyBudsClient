using System;
using System.Linq;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.SOC_BATTERY_CYCLE)]
internal class SocBatteryCycleDecoder : BaseMessageDecoder
{
    public ulong LeftCycles { get; }
    public ulong RightCycles { get; }
    
    public bool IsLeftBatteryBad { get; }
    public bool IsRightBatteryBad { get; }
    
    public SocBatteryCycleDecoder(SppMessage msg) : base(msg)
    {
        LeftCycles = BitConverter.ToUInt64(msg.Payload.Take(8).Reverse().ToArray());
        RightCycles = BitConverter.ToUInt64(msg.Payload.TakeLast(8).Reverse().ToArray());
        
        IsLeftBatteryBad = LeftCycles / 10000 >= 150;
        IsRightBatteryBad = RightCycles / 10000 >= 150;
    }
}
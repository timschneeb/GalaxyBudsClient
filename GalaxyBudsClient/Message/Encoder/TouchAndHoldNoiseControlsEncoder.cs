using System.Linq;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;

namespace GalaxyBudsClient.Message.Encoder;

public class TouchAndHoldNoiseControls : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS;
    public NoiseControlCycleModes CycleMode { get; set; }
    public NoiseControlCycleModes CycleModeRight { get; set; }
    
    public override SppMessage Encode()
    {
        var states = DeviceSpec.Supports(Features.NoiseControlModeDualSide) ? 
            GetValues(CycleMode).Concat(GetValues(CycleModeRight)).ToArray() : 
            GetValues(CycleMode);
            
        return new SppMessage(HandledType, MsgTypes.Request, states);
    }
    
    private static byte[] GetValues(NoiseControlCycleModes mode)
    {
        return mode switch
        {
            NoiseControlCycleModes.AncOff => [1, 0, 1],
            NoiseControlCycleModes.AmbOff => [0, 1, 1],
            NoiseControlCycleModes.AncAmb => [1, 1, 0],
            _ => [0, 0, 0]
        };
    }
}
using System.Linq;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS)]
public class TouchAndHoldNoiseControls : BaseMessageEncoder
{
    public NoiseControlCycleModes CycleMode { get; init; }
    public NoiseControlCycleModes CycleModeRight { get; init; }
    
    public override SppMessage Encode()
    {
        var states = DeviceSpec.Supports(Features.NoiseControlModeDualSide) ? 
            GetValues(CycleMode).Concat(GetValues(CycleModeRight)).ToArray() : 
            GetValues(CycleMode);
            
        return new SppMessage(MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, MsgTypes.Request, states);
    }
    
    private byte[] GetValues(NoiseControlCycleModes mode)
    {
        if (DeviceSpec.Device >= Models.Buds3)
        {
            // New format
            return mode switch
            {
                NoiseControlCycleModes.AncOff => [8 + 4],
                NoiseControlCycleModes.AmbOff => [0 + 4],
                NoiseControlCycleModes.AncAmb => [8 + 0],
                _ => [0, 0]
            };
            // TODO implement Adaptive mode
        }
        else
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
}
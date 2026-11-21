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
        if (DeviceSpec.Device >= Models.Buds4)
        {
            // Buds4 uses one bitmask byte per side:
            // Ambient=1, Adaptive=2, Off=4, ANC=8 (see NoiseControlCycleModes)
            return new SppMessage(MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, MsgTypes.Request,
                [(byte)CycleMode, (byte)CycleModeRight]);
        }

        var states = GetValues(CycleMode);
        if (DeviceSpec.Supports(Features.NoiseControlModeDualSide))
        {
            states = [.. states, .. GetValues(CycleModeRight)];
        }

        return new SppMessage(MsgIds.SET_TOUCH_AND_HOLD_NOISE_CONTROLS, MsgTypes.Request, states);
    }

    private byte[] GetValues(NoiseControlCycleModes mode)
    {
        if (DeviceSpec.Device >= Models.Buds3)
        {
            // Buds3 format: one byte per side, legacy values (no Adaptive bit)
            return mode switch
            {
                NoiseControlCycleModes.AncOff => [8 + 4],
                NoiseControlCycleModes.AmbOff => [0 + 4],
                NoiseControlCycleModes.AncAmb => [8 + 0],
                _ => [0, 0]
            };
        }

        return mode switch
        {
            NoiseControlCycleModes.AncOff => [1, 0, 1],
            NoiseControlCycleModes.AmbOff => [0, 1, 1],
            NoiseControlCycleModes.AncAmb => [1, 1, 0],
            _ => [0, 0, 0]
        };
    }
}

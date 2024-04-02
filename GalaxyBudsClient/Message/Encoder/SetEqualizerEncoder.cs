using System;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Encoder;

public class SetEqualizerEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.EQUALIZER;
    public bool IsEnabled { get; init; }
    public int Preset { get; init; }
    private bool DolbyMode { get; } = false; // Unused
    
    public override SppMessage Encode()
    {
        byte[] payload;
        
        // Dolby mode has no effect on the Buds+/Live/Pro
        if (TargetModel == Models.Buds)
        {
            var rawPreset = Preset;
            if (!DolbyMode)
                rawPreset += 5;

            payload = [Convert.ToByte(IsEnabled), (byte)rawPreset];
        }
        else
        {
            payload = [!IsEnabled ? (byte) 0 : Convert.ToByte(Preset + 1)];
        }
        
        return new SppMessage(HandledType, MsgTypes.Request, payload);
    }
}
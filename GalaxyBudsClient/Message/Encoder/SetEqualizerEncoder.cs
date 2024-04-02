using System;
using System.Threading.Tasks;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Encoder;

public class SetEqualizerEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.EQUALIZER;
    public bool IsEnabled { get; set; }
    public int Preset { get; set; }
    private bool DolbyMode { get; set; } = false;
    
    public override SppMessage Encode()
    {
        byte[] payload;
        
        // Dolby mode has no effect on the Buds+/Live/Pro
        if (TargetModel == Models.Buds)
        {
            var rawPreset = Preset;
            if (!DolbyMode)
                rawPreset += 5;

            payload = new byte[2];
            payload[0] = Convert.ToByte(IsEnabled);
            payload[1] = (byte)rawPreset;
        }
        else
        {
            payload = new byte[1];
            payload[0] = !IsEnabled ? (byte) 0 : Convert.ToByte(Preset + 1);
        }
        
        return new SppMessage(HandledType, MsgTypes.Request, payload);
    }
}
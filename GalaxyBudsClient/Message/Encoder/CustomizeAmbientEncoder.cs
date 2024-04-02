using System.IO;

namespace GalaxyBudsClient.Message.Encoder;

public class CustomizeAmbientEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.CUSTOMIZE_AMBIENT_SOUND;
    public bool IsEnabled { get; set; }
    public byte AmbientVolumeLeft { get; set; }
    public byte AmbientVolumeRight { get; set; }
    public byte AmbientTone { get; set; }
    
    public override SppMessage Encode()
    {
        using var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write(IsEnabled);
        writer.Write(AmbientVolumeLeft);
        writer.Write(AmbientVolumeRight);
        writer.Write(AmbientTone);
            
        return new SppMessage(HandledType, MsgTypes.Request, stream.ToArray());
    }
}
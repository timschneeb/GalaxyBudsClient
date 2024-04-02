using System.IO;

namespace GalaxyBudsClient.Message.Encoder;

public class CustomizeAmbientEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.CUSTOMIZE_AMBIENT_SOUND;
    public bool IsEnabled { get; init; }
    public byte AmbientVolumeLeft { get; init; }
    public byte AmbientVolumeRight { get; init; }
    public byte AmbientTone { get; init; }
    
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
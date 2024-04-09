using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.CUSTOMIZE_AMBIENT_SOUND)]
public class CustomizeAmbientEncoder : BaseMessageEncoder
{
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
            
        return new SppMessage(MsgIds.CUSTOMIZE_AMBIENT_SOUND, MsgTypes.Request, stream.ToArray());
    }
}
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Specifications;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.LOCK_TOUCHPAD)]
public class LockTouchpadEncoder : BaseMessageEncoder
{
    public bool LockAll { get; init; }
    public bool TapOn { get; init; }
    public bool DoubleTapOn { get; init; }
    public bool TripleTapOn { get; init; }
    public bool HoldTapOn { get; init; }
    public bool DoubleTapCallOn { get; init; }
    public bool HoldTapCallOn { get; init; }
    
    public override SppMessage Encode()
    {
        using var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write(!LockAll);
        writer.Write(TapOn);
        writer.Write(DoubleTapOn);
        writer.Write(TripleTapOn);
        writer.Write(HoldTapOn);

        if (DeviceSpec.Supports(Features.AdvancedTouchLockForCalls))
        {
            writer.Write(DoubleTapCallOn);
            writer.Write(HoldTapCallOn);
        }
            
        return new SppMessage(MsgIds.LOCK_TOUCHPAD, MsgTypes.Request, stream.ToArray());
    }
}
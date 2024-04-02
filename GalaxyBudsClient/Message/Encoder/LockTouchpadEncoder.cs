using System.IO;
using GalaxyBudsClient.Model.Specifications;

namespace GalaxyBudsClient.Message.Encoder;

public class LockTouchpadEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.LOCK_TOUCHPAD;
    public bool LockAll { get; set; }
    public bool TapOn { get; set; }
    public bool DoubleTapOn { get; set; }
    public bool TripleTapOn { get; set; }
    public bool HoldTapOn { get; set; }
    public bool DoubleTapCallOn { get; set; }
    public bool HoldTapCallOn { get; set; }
    
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
            
        return new SppMessage(HandledType, MsgTypes.Request, stream.ToArray());
    }
}
using System.IO;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
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

    /// <summary>Buds4 only: volume swipe gesture.</summary>
    public bool SwipeOn { get; init; }

    /// <summary>
    /// Buds4 only: current lighting style byte. It occupies the trailing
    /// "earbuds control" payload and must be echoed back unmodified to
    /// avoid resetting the blade light configuration.
    /// </summary>
    public byte Lighting { get; init; }

    /// <summary>
    /// Buds4 only: quick launch (double pinch and hold) state byte.
    /// Echoed back unmodified like <see cref="Lighting"/>.
    /// </summary>
    public byte QuickLaunchAdvanced { get; init; }

    public override SppMessage Encode()
    {
        if (DeviceSpec.Device >= Models.Buds4)
        {
            // Buds4 "earbuds control" layout, verified against the official
            // Galaxy Buds app (ar.a / nk.a):
            // [0] reserved, [1] touch controls master, [2] single tap,
            // [3] double tap, [4] triple tap, [5] pinch and hold,
            // [6] double tap during call, [7] pinch and hold during call,
            // [8] volume swipe, [9] duplicate of [6], [10] lighting style,
            // [11] quick launch.
            return new SppMessage(MsgIds.LOCK_TOUCHPAD, MsgTypes.Request,
            [
                (byte)0,
                (byte)1,
                (byte)(TapOn ? 1 : 0),
                (byte)(DoubleTapOn ? 1 : 0),
                (byte)(TripleTapOn ? 1 : 0),
                (byte)(HoldTapOn ? 1 : 0),
                (byte)(DoubleTapCallOn ? 1 : 0),
                (byte)(HoldTapCallOn ? 1 : 0),
                (byte)(SwipeOn ? 1 : 0),
                (byte)(DoubleTapCallOn ? 1 : 0),
                Lighting,
                QuickLaunchAdvanced
            ]);
        }

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
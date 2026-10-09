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

    /// <summary>
    /// Buds4 only: mask bits of gestures without a dedicated property
    /// (bits 0 and 6 of the ESU touch mask), echoed back unmodified.
    /// </summary>
    public byte GestureEchoBits { get; init; }

    public override SppMessage Encode()
    {
        if (DeviceSpec.Device >= Models.Buds4)
        {
            // Buds4 "earbuds control" layout, verified against the official
            // Galaxy Buds app and experimentally:
            // [0] reserved, [1] single tap, [2] double tap, [3] triple tap,
            // [4] pinch and hold, [5] additional gesture (echoed), [6] double
            // tap during call, [7] pinch and hold during call, [8] additional
            // gesture (echoed), [9] duplicate of [6], [10] lighting style,
            // [11] quick launch
            return new SppMessage(MsgIds.LOCK_TOUCHPAD, MsgTypes.Request,
            [
                (byte)0,
                (byte)(TapOn ? 1 : 0),
                (byte)(DoubleTapOn ? 1 : 0),
                (byte)(TripleTapOn ? 1 : 0),
                (byte)(HoldTapOn ? 1 : 0),
                (byte)((GestureEchoBits & 0x01) != 0 ? 1 : 0),
                (byte)(DoubleTapCallOn ? 1 : 0),
                (byte)(HoldTapCallOn ? 1 : 0),
                (byte)((GestureEchoBits & 0x40) != 0 ? 1 : 0),
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
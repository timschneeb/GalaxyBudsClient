using System.IO;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Message.Encoder;

public static class LockTouchpadEncoder
{
    public static SppMessage Build(bool lockAll, bool tapOn, bool doubleTapOn, bool tripleTapOn, bool holdTapOn, 
        bool doubleTapCallOn, bool holdTapCallOn)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write(!lockAll);
        writer.Write(tapOn);
        writer.Write(doubleTapOn);
        writer.Write(tripleTapOn);
        writer.Write(holdTapOn);

        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AdvancedTouchLockForCalls))
        {
            writer.Write(doubleTapCallOn);
            writer.Write(holdTapCallOn);
        }

        var data = stream.ToArray();
        stream.Close();
            
        return new SppMessage(MsgIds.LOCK_TOUCHPAD, MsgTypes.Request, data);
    }
}
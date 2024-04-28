using System.IO;

namespace GalaxyBudsClient.Message.Parameter;

public class LockTouchpadAckParameter : MessageAsDictionary, IAckParameter
{
    public bool TouchpadLock { get; }
    
    public bool TapOn { get; }
    public bool DoubleTapOn { get; }
    public bool TripleTapOn { get; }
    public bool TouchAndHoldOn { get; }
    public bool ForCallDoubleTap { get; }
    public bool ForCallTouchAndHold { get; }
    
    public bool SupportsAdvancedTouchLock { get; }

    public LockTouchpadAckParameter(BinaryReader reader)
    {
        TouchpadLock = reader.ReadBoolean();
        
        try {
            TapOn = reader.ReadBoolean();
            DoubleTapOn = reader.ReadBoolean();
            TripleTapOn = reader.ReadBoolean();
            TouchAndHoldOn = reader.ReadBoolean();
            ForCallDoubleTap = reader.ReadBoolean();
            ForCallTouchAndHold = reader.ReadBoolean();
            SupportsAdvancedTouchLock = true;
        }
        catch (EndOfStreamException) {
            SupportsAdvancedTouchLock = false;
        }
    }
}
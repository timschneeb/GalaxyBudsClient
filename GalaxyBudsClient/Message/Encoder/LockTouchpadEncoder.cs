using System.IO;

namespace GalaxyBudsClient.Message.Encoder
{
    public class LockTouchpadEncoder
    {
        public static SPPMessage Build(bool lockAll, bool tapOn, bool doubleTapOn, bool tripleTapOn, bool holdTapOn)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(lockAll);
            writer.Write(tapOn);
            writer.Write(doubleTapOn);
            writer.Write(tripleTapOn);
            writer.Write(holdTapOn);
            
            var data = stream.ToArray();
            stream.Close();
            
            return new SPPMessage(SPPMessage.MessageIds.LOCK_TOUCHPAD, SPPMessage.MsgType.Response, data);
        }
    }
}
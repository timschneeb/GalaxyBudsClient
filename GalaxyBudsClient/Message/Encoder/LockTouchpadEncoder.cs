using System.IO;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class LockTouchpadEncoder
    {
        public static SppMessage Build(bool lockAll, bool tapOn, bool doubleTapOn, bool tripleTapOn, bool holdTapOn)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(!lockAll);
            writer.Write(tapOn);
            writer.Write(doubleTapOn);
            writer.Write(tripleTapOn);
            writer.Write(holdTapOn);
            
            var data = stream.ToArray();
            stream.Close();
            
            return new SppMessage(SppMessage.MessageIds.LOCK_TOUCHPAD, SppMessage.MsgType.Request, data);
        }
    }
}

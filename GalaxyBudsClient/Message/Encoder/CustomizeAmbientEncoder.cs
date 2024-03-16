using System.IO;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class CustomizeAmbientEncoder
    {
        public static SppMessage Build(bool enable, byte ambientVolumeLeft, byte ambientVolumeRight, byte ambientTone)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(enable);
            writer.Write(ambientVolumeLeft);
            writer.Write(ambientVolumeRight);
            writer.Write(ambientTone);
            
            var data = stream.ToArray();
            stream.Close();
            
            return new SppMessage(SppMessage.MessageIds.CUSTOMIZE_AMBIENT_SOUND, SppMessage.MsgType.Request, data);
        }
    }
}
using System.IO;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class CustomizeAmbientEncoder
    {
        public static SPPMessage Build(bool enable, byte ambientVolumeLeft, byte ambientVolumeRight, byte ambientTone)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(enable);
            writer.Write(ambientVolumeLeft);
            writer.Write(ambientVolumeRight);
            writer.Write(ambientTone);
            
            var data = stream.ToArray();
            stream.Close();
            
            return new SPPMessage(SPPMessage.MessageIds.CUSTOMIZE_AMBIENT_SOUND, SPPMessage.MsgType.Request, data);
        }
    }
}
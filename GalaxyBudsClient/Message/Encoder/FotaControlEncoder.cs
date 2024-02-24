using System.IO;
using GalaxyBudsClient.Model.Firmware;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class FotaControlEncoder
    {
        public static SppMessage Build(FirmwareConstants.ControlIds controlId, short parameter)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            if (controlId == FirmwareConstants.ControlIds.SendMtu)
            {
                parameter = parameter > 650 ? (short)650 : parameter;
            }
            
            writer.Write((byte) controlId);
            writer.Write((short) parameter);
            
            var data = stream.ToArray();
            stream.Close();
            
            return new SppMessage(SppMessage.MessageIds.FOTA_CONTROL, SppMessage.MsgType.Response, data);
        }
    }
}
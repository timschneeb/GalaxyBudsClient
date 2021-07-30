using System;
using System.IO;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder
{
    public class FotaControlEncoder
    {
        public static SPPMessage Build(FirmwareConstants.ControlIds controlId, short parameter)
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
            
            return new SPPMessage(SPPMessage.MessageIds.FOTA_CONTROL, SPPMessage.MsgType.Response, data);
        }
    }
}
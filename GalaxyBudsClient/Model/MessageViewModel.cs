using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Model
{
    public class RecvMsgViewHolder
    {
        public String Id { get; set; }
        public String Payload { get; set; }
        public String PayloadAscii { get; set; }
        public String Type { set; get; }
        public String Size { set; get; }
        public String CRC16 { set; get; }
        public SPPMessage Message { set; get; }

        public RecvMsgViewHolder(SPPMessage msg)
        {
            Id = GetEnumName(typeof(SPPMessage.MessageIds),msg.Id);
            Payload = BitConverter.ToString(msg.Payload).Replace("-", " ");
            PayloadAscii = Hex.DumpAscii(msg.Payload);
            Type = GetEnumName(typeof(SPPMessage.MsgType), msg.Type);
            Size = $"{msg.Size} bytes";
            CRC16 = (msg.Crc16 == 0 ? "Pass" : "Fail");
            Message = msg;
        }

        private String GetEnumName(Type t, object i)
        {
            try
            {
                var name = Enum.GetName(t, i);
                if (name == null)
                    throw new ArgumentNullException();
                else if(name.StartsWith("UNKNOWN_"))
                    return $"Unknown ({Convert.ToInt32(i)})";

                return $"{name} ({Convert.ToInt32(i)})";
            }
            catch (ArgumentNullException)
            {
                return $"Unknown ({Convert.ToInt32(i)})";
            }
        }
    }
}

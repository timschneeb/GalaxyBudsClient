using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Model.ViewModels
{
    public class RecvMsgViewHolder
    {
        public String Id { get; }
        public String Payload { get; }
        public String PayloadAscii { get; }
        public String Type { get; }
        public String Size { get; }
        public String CRC16 { get; }
        public SppMessage Message { get; }

        public RecvMsgViewHolder(SppMessage msg)
        {
            Id = GetEnumName(typeof(SppMessage.MessageIds),msg.Id);
            Payload = BitConverter.ToString(msg.Payload).Replace("-", " ");
            PayloadAscii = HexUtils.DumpAscii(msg.Payload);
            Type = msg.IsFragment ? "Fragment/" : string.Empty + GetEnumName(typeof(SppMessage.MsgType), msg.Type);
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

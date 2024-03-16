using System;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Interface.ViewModels
{
    public class RecvMsgViewHolder
    {
        public string Id { get; }
        public string Payload { get; }
        public string PayloadAscii { get; }
        public string Type { get; }
        public string Size { get; }
        public string CRC16 { get; }
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

        private static string GetEnumName(Type t, object i)
        {
            try
            {
                var name = Enum.GetName(t, i);
                if (name == null)
                    throw new ArgumentException("Enum member name is null");
                if (name.StartsWith("UNKNOWN_"))
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

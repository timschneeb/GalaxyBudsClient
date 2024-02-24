using System;
using System.Collections.Generic;
using System.Reflection;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Message.Decoder
{
    class LogCoredumpDataParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.LOG_COREDUMP_DATA;
        
        public int PartialDataOffset { set; get; }
        public short PartialDataSize { set; get; }
        public byte[] RawData { set; get; } = Array.Empty<byte>();

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            PartialDataOffset = BitConverter.ToInt32(msg.Payload, 0);
            PartialDataSize = BitConverter.ToInt16(msg.Payload, 4);
            RawData = new byte[PartialDataSize];
            Array.Copy(msg.Payload, 6, RawData, 0, PartialDataSize);
        }
    }
}

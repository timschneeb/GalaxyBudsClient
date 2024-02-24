using System;

namespace GalaxyBudsClient.Message.Decoder
{
    class LogCoredumpDataSizeParser : BaseMessageParser
    {
        public override SppMessage.MessageIds HandledType => SppMessage.MessageIds.LOG_COREDUMP_DATA_SIZE;
        
        public int DataSize { set; get; }
        public short PartialDataMaxSize { set; get; }
        public int FragmentCount { set; get; }

        public override void ParseMessage(SppMessage msg)
        {
            if (msg.Id != HandledType)
                return;

            DataSize = BitConverter.ToInt32(msg.Payload, 0);
            PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
            FragmentCount = (int)Math.Ceiling((double)DataSize/(double)PartialDataMaxSize);
        }
    }
}

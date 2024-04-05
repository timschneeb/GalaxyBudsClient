using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class LogCoredumpDataSizeDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.LOG_COREDUMP_DATA_SIZE;
        
    public int DataSize { set; get; }
    public short PartialDataMaxSize { set; get; }
    public int FragmentCount { set; get; }

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        DataSize = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
        FragmentCount = (int)Math.Ceiling(DataSize/(double)PartialDataMaxSize);
    }
}
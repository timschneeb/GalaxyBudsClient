using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.LOG_COREDUMP_DATA_SIZE)]
internal class LogCoredumpDataSizeDecoder : BaseMessageDecoder
{
    public int DataSize { get; }
    public short PartialDataMaxSize { get; }
    public int FragmentCount { get; }

    public LogCoredumpDataSizeDecoder(SppMessage msg) : base(msg)
    {
        DataSize = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
        FragmentCount = (int)Math.Ceiling(DataSize/(double)PartialDataMaxSize);
    }
}
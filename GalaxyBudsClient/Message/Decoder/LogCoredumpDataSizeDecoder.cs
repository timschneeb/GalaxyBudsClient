using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.LOG_COREDUMP_DATA_SIZE)]
internal class LogCoredumpDataSizeDecoder : BaseMessageDecoder
{
    public int DataSize { set; get; }
    public short PartialDataMaxSize { set; get; }
    public int FragmentCount { set; get; }

    public override void Decode(SppMessage msg)
    {
        DataSize = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataMaxSize = BitConverter.ToInt16(msg.Payload, 4);
        FragmentCount = (int)Math.Ceiling(DataSize/(double)PartialDataMaxSize);
    }
}
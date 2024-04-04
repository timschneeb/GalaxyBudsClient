using System;

namespace GalaxyBudsClient.Message.Decoder;

internal class LogTraceDataDecoder : BaseMessageDecoder
{
    public override MsgIds HandledType => MsgIds.LOG_TRACE_DATA;
        
    public int PartialDataOffset { set; get; }
    public short PartialDataSize { set; get; }
    public byte[] RawData { set; get; } = Array.Empty<byte>();

    public override void Decode(SppMessage msg)
    {
        if (msg.Id != HandledType)
            return;

        PartialDataOffset = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataSize = BitConverter.ToInt16(msg.Payload, 4);
        RawData = new byte[PartialDataSize];
        Array.Copy(msg.Payload, 6, RawData, 0, PartialDataSize);
    }
}
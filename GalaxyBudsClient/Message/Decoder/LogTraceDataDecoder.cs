using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.LOG_TRACE_DATA)]
internal class LogTraceDataDecoder : BaseMessageDecoder
{
    public int PartialDataOffset { set; get; }
    public short PartialDataSize { set; get; }
    public byte[] RawData { set; get; } = Array.Empty<byte>();

    public override void Decode(SppMessage msg)
    {
        PartialDataOffset = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataSize = BitConverter.ToInt16(msg.Payload, 4);
        RawData = new byte[PartialDataSize];
        Array.Copy(msg.Payload, 6, RawData, 0, PartialDataSize);
    }
}
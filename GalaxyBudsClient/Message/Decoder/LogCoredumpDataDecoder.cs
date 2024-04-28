using System;
using GalaxyBudsClient.Generated.Model.Attributes;

namespace GalaxyBudsClient.Message.Decoder;

[MessageDecoder(MsgIds.LOG_COREDUMP_DATA)]
internal class LogCoredumpDataDecoder : BaseMessageDecoder
{
    public int PartialDataOffset { get; }
    public short PartialDataSize { get; }
    public byte[] RawData { get; }

    public LogCoredumpDataDecoder(SppMessage msg) : base(msg)
    {
        PartialDataOffset = BitConverter.ToInt32(msg.Payload, 0);
        PartialDataSize = BitConverter.ToInt16(msg.Payload, 4);
        RawData = new byte[PartialDataSize];
        Array.Copy(msg.Payload, 6, RawData, 0, PartialDataSize);
    }
}
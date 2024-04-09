using System;
using GalaxyBudsClient.Generated.Model.Attributes;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder;

[MessageEncoder(MsgIds.LOG_TRACE_DATA)]
public class LogTraceDataEncoder : BaseMessageEncoder
{
    public int Offset { get; init; }
    public int Size { get; init; }
    
    public override SppMessage Encode()
    {
        var bytes = ByteArrayUtils.Combine(
            BitConverter.GetBytes(Offset),
            BitConverter.GetBytes(Size)
        );
        return new SppMessage(MsgIds.LOG_TRACE_DATA, MsgTypes.Request, bytes);
    }
}
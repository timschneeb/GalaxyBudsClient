using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder;

public class LogTraceDataEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.LOG_TRACE_DATA;
    public int Offset { get; set; }
    public int Size { get; set; }
    
    public override SppMessage Encode()
    {
        var bytes = ByteArrayUtils.Combine(
            BitConverter.GetBytes(Offset),
            BitConverter.GetBytes(Size)
        );
        return new SppMessage(HandledType, MsgTypes.Request, bytes);
    }
}
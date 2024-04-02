using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder;

public class LogCoredumpDataEncoder : BaseMessageEncoder
{
    public override MsgIds HandledType => MsgIds.LOG_COREDUMP_DATA;
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
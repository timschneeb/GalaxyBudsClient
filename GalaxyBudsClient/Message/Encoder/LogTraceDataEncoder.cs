using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder;

public static class LogTraceDataEncoder
{
    public static SppMessage Build(int offset, int size)
    {
        var bytes = ByteArrayUtils.Combine(
            BitConverter.GetBytes(offset),
            BitConverter.GetBytes(size)
        );
        return new SppMessage(MsgIds.LOG_TRACE_DATA, MsgTypes.Request, bytes);
    }
}
using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class LogTraceDataEncoder
    {
        public static SPPMessage Build(int offset, int size)
        {
            var bytes = ByteArrayUtils.Combine(
                BitConverter.GetBytes(offset),
                BitConverter.GetBytes(size)
            );
            return new SPPMessage(SPPMessage.MessageIds.LOG_TRACE_DATA, SPPMessage.MsgType.Request, bytes);
        }
    }
}
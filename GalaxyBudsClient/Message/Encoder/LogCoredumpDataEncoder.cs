using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class LogCoredumpDataEncoder
    {
        public static SppMessage Build(int offset, int size)
        {
            var bytes = ByteArrayUtils.Combine(
                BitConverter.GetBytes(offset),
                BitConverter.GetBytes(size)
            );
            return new SppMessage(SppMessage.MessageIds.LOG_COREDUMP_DATA, SppMessage.MsgType.Request, bytes);
        }
    }
}
using System;
using GalaxyBudsClient.Utils;

namespace GalaxyBudsClient.Message.Encoder
{
    public static class LogCoredumpDataEncoder
    {
        public static SPPMessage Build(int offset, int size)
        {
            var bytes = ByteArrayUtils.Combine(
                BitConverter.GetBytes(offset),
                BitConverter.GetBytes(size)
            );
            return new SPPMessage(SPPMessage.MessageIds.MSG_ID_LOG_COREDUMP_DATA, SPPMessage.MsgType.Request, bytes);
        }
    }
}
using System;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Model
{
    public class InvalidPacketException : Exception
    {
        public enum ErrorCodes
        {
            SOM,
            EOM,
            Checksum,
            SizeMismatch,
            TooSmall,

            OutOfRange,
            Overflow
        }

        public ErrorCodes ErrorCode;
        public SppMessage? Draft;

        public InvalidPacketException(ErrorCodes errorCode, string message, SppMessage? draft = null) : base(message)
        {
            ErrorCode = errorCode;
            Draft = draft;
        }
    }
}
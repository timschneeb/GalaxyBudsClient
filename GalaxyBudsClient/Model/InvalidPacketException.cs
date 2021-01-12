using System;

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

        public InvalidPacketException(ErrorCodes errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
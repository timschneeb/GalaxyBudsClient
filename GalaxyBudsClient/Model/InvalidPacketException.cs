using System;
using GalaxyBudsClient.Message;

namespace GalaxyBudsClient.Model;

public class InvalidPacketException(
    InvalidPacketException.ErrorCodes errorCode,
    string? message = null,
    SppMessage? draft = null) : Exception(message)
{
    public enum ErrorCodes
    {
        Som,
        Eom,
        Checksum,
        SizeMismatch,
        TooSmall,

        OutOfRange,
        Overflow
    }

    public readonly ErrorCodes ErrorCode = errorCode;
    public readonly SppMessage? Draft = draft;
}
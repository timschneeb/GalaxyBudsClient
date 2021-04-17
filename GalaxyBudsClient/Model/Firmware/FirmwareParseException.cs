using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareParseException : Exception
    {
        public enum ErrorCodes
        {
            InvalidMagic,
            SizeZero,
            NoSegmentsFound,
            
            Unknown
        }
        
        public readonly string ErrorName;
        public readonly string ErrorMessage;
        public readonly ErrorCodes ErrorCode;

        public FirmwareParseException(ErrorCodes code, string message)
            : base($"{code.ToString()}: {message}")
        {
            ErrorName = code.ToString();
            ErrorMessage = message;
            ErrorCode = code;
        }
    }
}
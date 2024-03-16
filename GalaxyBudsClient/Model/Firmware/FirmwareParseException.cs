using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareParseException(FirmwareParseException.ErrorCodes code, string message)
        : Exception($"{code.ToString()}: {message}")
    {
        public enum ErrorCodes
        {
            InvalidMagic,
            SizeZero,
            NoSegmentsFound,
            
            Unknown
        }
        
        public readonly string ErrorName = code.ToString();
        public readonly string ErrorMessage = message;
        public readonly ErrorCodes ErrorCode = code;
    }
}
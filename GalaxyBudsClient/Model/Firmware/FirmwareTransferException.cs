using System;

namespace GalaxyBudsClient.Model.Firmware
{
    public class FirmwareTransferException : Exception
    {
        public enum ErrorCodes
        {
            SessionTimeout,
            ControlTimeout,
            CopyTimeout,
            
            ParseFail,
            SessionFail,
            CopyFail,
            VerifyFail,
            
            BatteryLow,
            InProgress,
            Disconnected,
            Unknown
        }
        
        public readonly string ErrorName;
        public readonly string ErrorMessage;
        public readonly ErrorCodes ErrorCode;

        public FirmwareTransferException(FirmwareParseException ex)
            : base($"{ex.ErrorCode.ToString()}: {ex.ErrorMessage}", ex)
        {
            ErrorName = ex.ErrorName;
            ErrorMessage = ex.ErrorMessage;
            ErrorCode = ErrorCodes.ParseFail;
        }
        
        public FirmwareTransferException(ErrorCodes code, string message)
            : base($"{code.ToString()}: {message}")
        {
            ErrorName = code.ToString();
            ErrorMessage = message;
            ErrorCode = code;
        }
    }
}
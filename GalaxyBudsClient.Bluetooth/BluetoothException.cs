using System;

namespace GalaxyBudsClient.Bluetooth.Linux
{
    public class BluetoothException : Exception
    {
        public enum ErrorCodes
        {
            NoAdaptersAvailable,
            UnsupportedDevice,
            TimedOut,
            ConnectFailed,
            Unknown
        }
        
        public readonly string ErrorName;
        public readonly string? ErrorMessage;
        public readonly ErrorCodes ErrorCode;
        
        public BluetoothException(ErrorCodes code)
            : base(code.ToString())
        {
            ErrorName = code.ToString();
            ErrorMessage = null;
            ErrorCode = code;
        }
        
        public BluetoothException(ErrorCodes code, string message)
            : base($"{code.ToString()}: {message}")
        {
            ErrorName = code.ToString();
            ErrorMessage = message;
            ErrorCode = code;
        }
    }
}
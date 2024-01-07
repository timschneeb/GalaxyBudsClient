using System;

namespace ThePBone.OSX.Native
{
    public class BluetoothException : Exception
    {
        public enum ErrorCodes
        {
            NoAdaptersAvailable,
            UnsupportedDevice,
            TimedOut,
            ConnectFailed,
            ReceiveFailed,
            SendFailed,
            AllocationFailed,
            Unknown
        }
        
        public readonly string? ErrorMessage;
        public readonly ErrorCodes ErrorCode;
        
        public BluetoothException(ErrorCodes code)
            : base(code.ToString())
        {
            ErrorMessage = null;
            ErrorCode = code;
        }
        
        public BluetoothException(ErrorCodes code, string message)
            : base($"{code.ToString()}: {message}")
        {
            ErrorMessage = message;
            ErrorCode = code;
        }
    }
}
using System;
using Tmds.DBus;

namespace ThePBone.BlueZNet
{
    public class BlueZException : Exception
    {
        public enum ErrorCodes
        {
            InProgress,
            AlreadyConnected,
            AlreadyExists,
            InvalidArguments,
            ConnectFailed,
            DoesNotExist,
            Failed,
            NotPermitted,
            
            NoAdaptersAvailable,
            Unknown
        }
        
        public readonly string ErrorName;
        public readonly string ErrorMessage;
        public readonly ErrorCodes ErrorCode;

        public BlueZException(DBusException inner)
            : base($"{inner.ErrorName}: {inner.ErrorMessage}", inner)
        {
            ErrorName = inner.ErrorName;
            ErrorMessage = inner.ErrorMessage;

            switch (ErrorName)
            {
                case "org.bluez.Error.InProgress":
                    ErrorCode = ErrorCodes.InProgress;
                    break;
                case "org.bluez.Error.AlreadyConnected":
                    ErrorCode = ErrorCodes.AlreadyConnected;
                    break;
                case "org.bluez.Error.NotPermitted":
                    ErrorCode = ErrorCodes.NotPermitted;
                    break;
                case "org.bluez.Error.AlreadyExists":
                    ErrorCode = ErrorCodes.AlreadyExists;
                    break;
                case "org.bluez.Error.InvalidArguments":
                    ErrorCode = ErrorCodes.InvalidArguments;
                    break;
                case "org.bluez.Error.ConnectFailed":
                    ErrorCode = ErrorCodes.ConnectFailed;
                    break;
                case "org.bluez.Error.Failed":
                    ErrorCode = ErrorCodes.Failed;
                    break;
                case "org.bluez.Error.DoesNotExist":
                    ErrorCode = ErrorCodes.DoesNotExist;
                    break;
                default:
                    ErrorCode = ErrorCodes.Unknown;
                    break;
            }
        }

        public BlueZException(ErrorCodes code, string message)
            : base($"{code.ToString()}: {message}")
        {
            ErrorName = code.ToString();
            ErrorMessage = message;
            ErrorCode = code;
        }
    }
}
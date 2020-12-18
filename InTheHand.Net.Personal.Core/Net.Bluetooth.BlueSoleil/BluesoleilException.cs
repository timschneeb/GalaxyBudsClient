// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Diagnostics;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    /// <summary>
    /// SocketException holding a BlueSoleil error code from the original error,
    /// which is added to the exception message.
    /// </summary>
    /// -
    /// <remarks>
    /// Will always be internal so just catch SocketException as for the other stacks.
    /// </remarks>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    internal class BlueSoleilSocketException : SocketException
    {
        const string Key_BtSdkError = "BtSdkError";

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal BlueSoleilSocketException(BtSdkError bsError, int socketErr)
            : base(socketErr)
        {
            int iError = (int)bsError;
            Set(iError);
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal BlueSoleilSocketException(BtSdkError bsError, SocketError socketErr)
            : this(bsError, (int)socketErr)
        {
        }

        private void Set(int iError)
        {
            this.Data.Add(Key_BtSdkError, iError);
        }

        #region Serializable
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected BlueSoleilSocketException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            int iError = serializationInfo.GetInt32(Key_BtSdkError);
            Set(iError);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(Key_BtSdkError, this.BlueSoleilErrorCode);
        }
        #endregion

        //----
        public string BlueSoleilError
        {
            get
            {
                var iErr = BlueSoleilErrorCode;
                if (Enum.IsDefined(typeof(BtSdkError), iErr)) {
                    return unchecked((BtSdkError)iErr).ToString();
                } else {
                    return "0x" + unchecked((UInt32)iErr).ToString("X", CultureInfo.InvariantCulture);
                }
            }
        }

        private BtSdkError BlueSoleilErrorCodeEnum
        {
            get { return (BtSdkError)BlueSoleilErrorCode; }
        }

        public int BlueSoleilErrorCode
        {
            get
            {
#if DEBUG
                Debug.Assert(Data.Contains(Key_BtSdkError), "contains");
                Debug.Assert(Data[Key_BtSdkError] != null, "contains NULL");
                object dbg = Data[Key_BtSdkError];
                Type dbgType = dbg == null ? typeof(void) : dbg.GetType();
                Debug.Assert(dbgType == typeof(Int32), "type");
#endif
                int iError = (int)Data[Key_BtSdkError];
                return iError;
            }
        }

        public override string Message
        {
            get
            {
                return base.Message
                + string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " (BlueSoleil: {0} (0x{1:X4})).", BlueSoleilErrorCodeEnum,
                    unchecked((UInt32)BlueSoleilErrorCode));
            }
        }
    }

}

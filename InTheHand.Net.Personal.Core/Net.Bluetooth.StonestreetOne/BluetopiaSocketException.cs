// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaSocketException
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Sockets;
#if !NETCF
using System.Runtime.Serialization;
#endif
using System.Security.Permissions;
using System.Text;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{

    [Serializable]
    class BluetopiaSocketException2
        : Widcomm.GenericReturnCodeWidcommSocketException<BluetopiaError>
    {
        internal BluetopiaSocketException2(int errorCode, BluetopiaError ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected BluetopiaSocketException2(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }


    [Serializable]
    internal class BluetopiaSocketException : SocketException
    {
        const string Key_BluetopiaError = "BluetopiaError";

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal BluetopiaSocketException(BluetopiaError bluetopiaError, int socketErr)
            : base(socketErr)
        {
            int iError = (int)bluetopiaError;
            Set(iError);
        }

#if !NETCF
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal BluetopiaSocketException(BluetopiaError bluetopiaError, SocketError socketErr)
            : this(bluetopiaError, (int)socketErr)
        {
        }
#endif

#if NETCF
        // Arghhhhhh
        private System.Collections.Hashtable Data = new System.Collections.Hashtable();
#endif

        private void Set(int iError)
        {
            this.Data.Add(Key_BluetopiaError, iError);
        }

        #region Serializable
#if !NETCF
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected BluetopiaSocketException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            int iError = serializationInfo.GetInt32(Key_BluetopiaError);
            Set(iError);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(Key_BluetopiaError, this.BluetopiaErrorCode);
        }
#endif
        #endregion

        //----
        public string BluetopiaError
        {
            get
            {
                var iErr = BluetopiaErrorCode;
                if (Enum.IsDefined(typeof(BluetopiaError), iErr)) {
                    return unchecked((BluetopiaError)iErr).ToString();
                } else {
                    return unchecked((Int32)iErr).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private BluetopiaError BluetopiaErrorCodeEnum
        {
            get { return (BluetopiaError)BluetopiaErrorCode; }
        }

        public int BluetopiaErrorCode
        {
            get
            {
#if DEBUG
                Debug.Assert(Data.Contains(Key_BluetopiaError), "contains");
                Debug.Assert(Data[Key_BluetopiaError] != null, "contains NULL");
                object dbg = Data[Key_BluetopiaError];
                Type dbgType = dbg == null ? typeof(void) : dbg.GetType();
                Debug.Assert(dbgType == typeof(Int32), "type");
#endif
                int iError = (int)Data[Key_BluetopiaError];
                return iError;
            }
        }

        public override string Message
        {
            get
            {
                return base.Message
                + string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    " (Bluetopia: {0} ({1})).", BluetopiaErrorCodeEnum,
                    unchecked((Int32)BluetopiaErrorCode));
            }
        }
    }
}

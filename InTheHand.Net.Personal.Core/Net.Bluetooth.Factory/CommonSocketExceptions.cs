// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2008-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
#if FX3_5
using System.Linq;
#endif
using System.Text;
using System.Net.Sockets;
#if !NETCF
using System.Runtime.Serialization;
using System.Security.Permissions;
#else
using InTheHand.Net.Sockets;
#endif
using InTheHand.Net.Bluetooth.Widcomm;

namespace InTheHand.Net.Bluetooth.Factory
{
    static class CommonSocketExceptions
    {
        internal static SocketException Create_NoResultCode(int errorCode, string location)
        {
            return new NoResultCodeWidcommSocketException(errorCode, location);
        }

        internal static SocketException Create_StartInquiry(string location)
        {
            return Create_NoResultCode(SocketError_StartInquiry_Failed, location);
        }

        internal static SocketException CreateConnectFailed(string location, int? socketErrorCode)
        {
            int errorCode = socketErrorCode ?? (int)SocketError.ConnectionRefused;// SocketError_ConnectFailed;
            return Create_NoResultCode(errorCode, location);
        }

        internal static SocketException ConnectionIsPeerClosed()
        {
            return Create_NoResultCode((int)SocketError.NotConnected, "RfcommStream_Closed");
        }

        //--------------------------------------------------------------
#if WinXP
        //static SocketError ___err;
#endif
        //const int SocketError_ConnectFailed = 10061; //ConnectionRefused = 10061,
        //
        //const int SocketError_SystemNotReady10091 = 10091;
        //const int SocketError_VersionNotSupported10092 = 10092;
        //const int SocketError_Fault10014 = 10014;
        //
        internal const int SocketError_StartInquiry_Failed = (int)SocketError.SystemNotReady; //SocketError_SystemNotReady10091;
        internal const int SocketError_SetSecurityLevel_Client_Fail = -1;
        internal const int SocketError_StartDiscovery_Failed = (int)SocketError.VersionNotSupported; //SocketError_VersionNotSupported10092;
        internal const int SocketError_NoSuchService = (int)SocketError.AddressNotAvailable; //10049;
        internal const int SocketError_ServiceNoneRfcommScn = (int)SocketError.HostDown; //10064;
        //
        internal const int SocketError_ConnectionClosed = (int)SocketError.NotConnected; //10057;
        //
        internal const int SocketError_Listener_SdpError = (int)SocketError.Fault; //SocketError_Fault10014;

    }
}


namespace InTheHand.Net.Bluetooth.Widcomm
{
    /// <summary>
    /// Note that this exception will always be internal, just catch SocketException.
    /// </summary>
    [Serializable]
    abstract class WidcommSocketException
        : SocketException
    {
        //--------------------------------------------------------------
        readonly string m_location;

        protected WidcommSocketException(int errorCode, string location)
            : base(errorCode)
        {
            m_location = location;
        }

        public override string Message
        {
            get
            {
                return /*base.Message
                    + "; " +*/
                               ErrorCodeAndDescription
                    + (m_location == null ? null : ("; " + m_location));
            }
        }
        protected abstract string ErrorCodeAndDescription { get; }

        //----
#region Serializable
#if !NETCF
        private const string SzName_location = "_location";

        [Obsolete("Binary formatter serialization is obsolete.")]
        protected WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_location = info.GetString(SzName_location);
        }

        [Obsolete("Binary formatter serialization is obsolete.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(SzName_location, m_location);
        }
#endif
#endregion
    }


    [Serializable]
    class NoResultCodeWidcommSocketException
        : WidcommSocketException
    {
        internal NoResultCodeWidcommSocketException(int errorCode, string location)
            : base(errorCode, location)
        {
        }

        protected override string ErrorCodeAndDescription
        {
            get { return null; }
        }

        //----
#region Serializable
#if !NETCF
        [Obsolete("Binary formatter serialization is obsolete.")]
        protected NoResultCodeWidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
#endregion
    }
}

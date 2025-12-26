// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
#if !NETCF
using System.Runtime.Serialization;
using System.Security.Permissions;
#else
using InTheHand.Net.Sockets;
#endif
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal static class WidcommSocketExceptions
    {
        // HACK HIDE_WIDCOMM_WIDCOMM_EXCEPTIONS Split into common and Widcomm parts
#if !HIDE_WIDCOMM_WIDCOMM_EXCEPTIONS
        internal static SocketException Create(REM_DEV_INFO_RETURN_CODE err, string location)
        {
            int errorCode = 10000;
            return new REM_DEV_INFO_RETURN_CODE_WidcommSocketException(errorCode, err, location);
        }

        internal static SocketException Create(DISCOVERY_RESULT result, string location)
        {
            return new DISCOVERY_RESULT_WidcommSocketException(CommonSocketExceptions.SocketError_StartDiscovery_Failed, result, location);
        }

        internal static SocketException Create(PORT_RETURN_CODE result, string location)
        {
            return new PORT_RETURN_CODE_WidcommSocketException(CommonSocketExceptions.SocketError_StartDiscovery_Failed, result, location);
        }

        internal static SocketException Create_SDP_RETURN_CODE(SdpService.SDP_RETURN_CODE ret, string location)
        {
            return new SDP_RETURN_CODE_WidcommSocketException(
                CommonSocketExceptions.SocketError_Listener_SdpError, ret, location);
        }
#endif
        //-- 
#if !HIDE_WIDCOMM_WIDCOMM_EXCEPTIONS
        internal static SocketException Create_StartDiscovery(WBtRc ee)
        {
            return CommonSocketExceptions.Create_NoResultCode(CommonSocketExceptions.SocketError_StartDiscovery_Failed, "StartDiscoverySDP"
                + ((ee == unchecked((WBtRc)(-1)) /*|| ee == WBtRc.WBT_SUCCESS*/) ? string.Empty
                    : $", {ee} = 0x{(uint)ee:X}"));
        }
#endif
    }

#if !HIDE_WIDCOMM_WIDCOMM_EXCEPTIONS
    //----
    abstract class GenericReturnCodeWidcommSocketException<T>
        : WidcommSocketException
        where T : IConvertible // Really want a constraint of "enum", see SetEnum...
    {
        protected readonly Int32 m_ret;  // MUST call SetEnum after setting this.
        protected string m_retName;

        internal GenericReturnCodeWidcommSocketException(int errorCode, T ret, string location)
            : base(errorCode, location)
        {
            if (!typeof(T).IsEnum) { // Need to check the constraint at runtime. :-(
                throw new InvalidOperationException("Internal error -- The generic parameter must be an Enum type.");
            }
            //
            m_ret = ret.ToInt32(System.Globalization.CultureInfo.InvariantCulture);
            SetEnum();
        }

        protected void SetEnum()
        {
            // Would like to do: "m_retName = (T)m_ret;"  But that would need a 
            // constraint of "where T : enum" which isn't possible in C# (but is
            // in IL).  So have to do something else...
#if !NETCF
            m_retName = Enum.Format(typeof(T), m_ret, "G");
#else
            object ee = Enum.Parse(typeof(T), m_ret.ToString(), false);
            m_retName = ee.ToString();
#endif
        }

        protected override string ErrorCodeAndDescription
        {
            get
            {
                return typeof(T).Name + $"={m_retName}=0x{m_ret:X}";
            }
        }

        //----
        #region Serializable
#if !NETCF
        private const string SzName_ret = "_ret";
        [Obsolete("Binary formatter serialization is obsolete.")]
        protected GenericReturnCodeWidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_ret = info.GetInt32(SzName_ret);
            SetEnum();
        }

        [Obsolete("Binary formatter serialization is obsolete.")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(SzName_ret, m_ret);
        }
#endif
        #endregion
    }

    /**************
    // REPLACE XX four times
    [Serializable]
    class XX_WidcommSocketException
        : GenericReturnCodeWidcommSocketException<XX>
    {
        internal XX_WidcommSocketException(int errorCode, XX ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected XX_WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }
    ****************/

    
    //----
    [Serializable]
    class REM_DEV_INFO_RETURN_CODE_WidcommSocketException
        : GenericReturnCodeWidcommSocketException<REM_DEV_INFO_RETURN_CODE>
    {
        internal REM_DEV_INFO_RETURN_CODE_WidcommSocketException(int errorCode, REM_DEV_INFO_RETURN_CODE ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected REM_DEV_INFO_RETURN_CODE_WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }


    [Serializable]
    class PORT_RETURN_CODE_WidcommSocketException
        : GenericReturnCodeWidcommSocketException<PORT_RETURN_CODE>
    {
        internal PORT_RETURN_CODE_WidcommSocketException(int errorCode, PORT_RETURN_CODE ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected PORT_RETURN_CODE_WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }


    [Serializable]
    class DISCOVERY_RESULT_WidcommSocketException
        : GenericReturnCodeWidcommSocketException<DISCOVERY_RESULT>
    {
        internal DISCOVERY_RESULT_WidcommSocketException(int errorCode, DISCOVERY_RESULT ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected DISCOVERY_RESULT_WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }


    [Serializable]
    class SDP_RETURN_CODE_WidcommSocketException
        : GenericReturnCodeWidcommSocketException<SdpService.SDP_RETURN_CODE>
    {
        internal SDP_RETURN_CODE_WidcommSocketException(int errorCode, SdpService.SDP_RETURN_CODE ret, string location)
            : base(errorCode, ret, location)
        {
        }

        #region Serializable
#if !NETCF
        protected SDP_RETURN_CODE_WidcommSocketException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
        #endregion
    }
#endif

}

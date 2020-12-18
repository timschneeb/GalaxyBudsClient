// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.StackConsts
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    static class StackConsts
    {
        internal const int BdaddrSize = 6;

        #region bluetooth.h
        // #define AF_BLUETOOTH	31

        //internal const ProtocolType ...
        // #define BTPROTO_L2CAP	0
        // #define BTPROTO_HCI	1
        // #define BTPROTO_SCO	2
        // #define BTPROTO_RFCOMM	3
        // #define BTPROTO_BNEP	4
        // #define BTPROTO_CMTP	5
        // #define BTPROTO_HIDP	6
        // #define BTPROTO_AVDTP	7

        internal const SocketOptionLevel SOL_HCI = 0;
        internal const SocketOptionLevel SOL_L2CAP = (SocketOptionLevel)6;
        internal const SocketOptionLevel SOL_SCO = (SocketOptionLevel)17;
        internal const SocketOptionLevel SOL_RFCOMM = (SocketOptionLevel)18;
        internal const SocketOptionLevel SOL_BLUETOOTH = (SocketOptionLevel)274;

        /**/
        internal static readonly byte[] BDADDR_ANY = { 0, 0, 0, 0, 0, 0 };
        internal static readonly byte[] BDADDR_ALL = { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };
        internal static readonly byte[] BDADDR_LOCAL = { 0, 0, 0, 0xff, 0xff, 0xff };
        #endregion

        #region hci.h
        internal enum IREQ_int
        {
            IREQ_CACHE_FLUSH = 0x0001
        }
        #endregion

        #region sdp.h
        /*
         * The Data representation in SDP PDUs (pps 339, 340 of BT SDP Spec)
         * These are the exact data type+size descriptor values
         * that go into the PDU buffer.
         *
         * The datatype (leading 5bits) + size descriptor (last 3 bits)
         * is 8 bits. The size descriptor is critical to extract the
         * right number of bytes for the data value from the PDU.
         *
         * For most basic types, the datatype+size descriptor is
         * straightforward. However for constructed types and strings,
         * the size of the data is in the next "n" bytes following the
         * 8 bits (datatype+size) descriptor. Exactly what the "n" is
         * specified in the 3 bits of the data size descriptor.
         *
         * TextString and URLString can be of size 2^{8, 16, 32} bytes
         * DataSequence and DataSequenceAlternates can be of size 2^{8, 16, 32}
         * The size are computed post-facto in the API and are not known apriori
         */
        internal enum SdpType_uint8_t : byte
        {
            DATA_NIL = 0x00,
            UINT8 = 0x08,
            UINT16 = 0x09,
            UINT32 = 0x0A,
            UINT64 = 0x0B,
            UINT128 = 0x0C,
            INT8 = 0x10,
            INT16 = 0x11,
            INT32 = 0x12,
            INT64 = 0x13,
            INT128 = 0x14,
            UUID_UNSPEC = 0x18,
            UUID16 = 0x19,
            UUID32 = 0x1A,
            UUID128 = 0x1C,
            TEXT_STR_UNSPEC = 0x20,
            TEXT_STR8 = 0x25,
            TEXT_STR16 = 0x26,
            TEXT_STR32 = 0x27,
            BOOL = 0x28,
            SEQ_UNSPEC = 0x30,
            SEQ8 = 0x35,
            SEQ16 = 0x36,
            SEQ32 = 0x37,
            ALT_UNSPEC = 0x38,
            ALT8 = 0x3D,
            ALT16 = 0x3E,
            ALT32 = 0x3F,
            URL_STR_UNSPEC = 0x40,
            URL_STR8 = 0x45,
            URL_STR16 = 0x46,
            URL_STR32 = 0x47,
        }
        #endregion

        #region sdp_lib.h
        /// <summary>
        /// Values of the flags parameter to sdp_record_register
        /// </summary>
        [Flags]
        internal enum SdpRecordRegisterFlags
        {
            SDP_RECORD_PERSIST = 0x01,
            SDP_DEVICE_RECORD = 0x02
        }

        /// <summary>
        /// Values of the flags parameter to sdp_connect
        /// </summary>
        [Flags]
        internal enum SdpConnectFlags
        {
            SDP_RETRY_IF_BUSY = 0x01,
            SDP_WAIT_ON_CLOSE = 0x02,
            SDP_NON_BLOCKING = 0x04
        }

        internal enum sdp_attrreq_type_t
        {
            /// <summary>
            /// Attributes are specified as individual elements
            /// </summary>
            SDP_ATTR_REQ_INDIVIDUAL = 1,
            /// <summary>
            /// Attributes are specified as a range
            /// </summary>
            SDP_ATTR_REQ_RANGE
        }
        #endregion

        #region rfcomm.h
        /* RFCOMM defaults */
        internal const int RFCOMM_DEFAULT_MTU = 127;

        internal const int RFCOMM_PSM = 3;

        /* RFCOMM socket options */
        /// <summary>
        /// Use with struct rfcomm_conninfo{hci_handle, dev_class}.
        /// </summary>
        internal const SocketOptionName so_RFCOMM_CONNINFO = (SocketOptionName)0x02;

        internal const SocketOptionName so_RFCOMM_LM = (SocketOptionName)0x03;
        [Flags]
        internal enum RFCOMM_LM
        {
            Master = 0x0001,
            Auth = 0x0002,
            Encrypt = 0x0004,
            Trusted = 0x0008,
            Reliable = 0x0010,
            Secure = 0x0020
        }

#if RFCOMM_TTY
        /* RFCOMM TTY support */
        internal const int RFCOMM_MAX_DEV = 256;

        internal const int RFCOMMCREATEDEV = 0x400452c8; // _IOW('R', 200, int)
        internal const int RFCOMMRELEASEDEV = 0x400452c9; // _IOW('R', 201, int)
        internal const int RFCOMMGETDEVLIST = unchecked((int)0x800452d2); // _IOR('R', 210, int)
        internal const int RFCOMMGETDEVINFO = unchecked((int)0x800452d3); // _IOR('R', 211, int)

        //
        internal const int RFCOMM_REUSE_DLC = 0;
        internal const int RFCOMM_RELEASE_ONHUP = 1;
        internal const int RFCOMM_HANGUP_NOW = 2;
        internal const int RFCOMM_TTY_ATTACHED = 3;
#endif
        #endregion

    }
}

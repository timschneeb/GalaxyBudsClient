// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.Structs
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using uint8_t = System.Byte;
using uint16_t = System.UInt16;
using int16_t = System.Int16;
using uint32_t = System.UInt32;
//
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    static class Structs
    {

#pragma warning disable 169 // The field '...' is never used
#pragma warning disable 649 // Field '...' is never assigned to, and will always have its default value ...

        #region hci.h
        internal struct inquiry_info
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            internal readonly byte[] bdaddr;
            internal readonly byte pscan_rep_mode;
            internal readonly byte pscan_period_mode;
            internal readonly byte pscan_mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal readonly byte[] dev_class;
            internal readonly UInt16 clock_offset;
        }

        #endregion

        #region hci_lib.h
        //struct hci_request {
        //    uint16_t ogf;
        //    uint16_t ocf;
        //    int      event;
        //    void     *cparam;
        //    int      clen;
        //    void     *rparam;
        //    int      rlen;
        //};

        internal struct hci_version
        {
            internal readonly UInt16 manufacturer;
            internal readonly byte hci_ver;
            internal readonly UInt16 hci_rev;
            internal readonly byte lmp_ver;
            internal readonly UInt16 lmp_subver;

            internal hci_version(HciVersion fake)
                : this()
            {
                Debug.Assert(fake == HciVersion.Unknown, "This ctor only used to fill with UNKNOWN values.");
                InTheHand.Net.Bluetooth.Widcomm.DEV_VER_INFO.SetManufacturerAndVersionsToUnknown(
                    out manufacturer, out hci_ver, out lmp_ver);
            }
        }
        #endregion

        #region sdp.h
        internal const string SDP_UNIX_PATH = "/var/run/sdp";
        internal const int SDP_REQ_BUFFER_SIZE = 2048;
        internal const int SDP_RSP_BUFFER_SIZE = 65535;
        internal const int SDP_PDU_CHUNK_SIZE = 1024;

#if false
        /*
         * The PDU identifiers of SDP packets between client and server
         */
        define SDP_ERROR_RSP		0x01
        define SDP_SVC_SEARCH_REQ	0x02
        define SDP_SVC_SEARCH_RSP	0x03
        define SDP_SVC_ATTR_REQ	0x04
        define SDP_SVC_ATTR_RSP	0x05
        define SDP_SVC_SEARCH_ATTR_REQ	0x06
        define SDP_SVC_SEARCH_ATTR_RSP	0x07

        /*
         * Some additions to support service registration.
         * These are outside the scope of the Bluetooth specification
         */
        define SDP_SVC_REGISTER_REQ	0x75
        define SDP_SVC_REGISTER_RSP	0x76
        define SDP_SVC_UPDATE_REQ	0x77
        define SDP_SVC_UPDATE_RSP	0x78
        define SDP_SVC_REMOVE_REQ	0x79
        define SDP_SVC_REMOVE_RSP	0x80

        /*
         * SDP Error codes
         */
        define SDP_INVALID_VERSION		0x0001
        define SDP_INVALID_RECORD_HANDLE	0x0002
        define SDP_INVALID_SYNTAX		0x0003
        define SDP_INVALID_PDU_SIZE		0x0004
        define SDP_INVALID_CSTATE		0x0005


        /*
         * SDP PDU
         */
        typedef struct {
	        uint8_t  pdu_id;
	        uint16_t tid;
	        uint16_t plen;
        } __attribute__ ((packed)) sdp_pdu_hdr_t;

#endif
        /*
         * Common definitions for attributes in the SDP.
         * Should the type of any of these change, you need only make a change here.
         */
        //struct uint128_t{
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst=16)]
        //    uint8_t[] data;
        //} ;

        internal struct uuid_t
        {
            internal readonly StackConsts.SdpType_uint8_t type;
            const int PADDING_SIZEa = 3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PADDING_SIZEa)]
            uint8_t[] PADDING;
            const int ValueLength = 16;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ValueLength)]
            internal readonly uint8_t[] value;
            //union {
            //    uint16_t  uuid16;
            //    uint32_t  uuid32;
            //    uint128_t uuid128;
            //} value;

            internal uuid_t(Guid uuid)
            {
                type = StackConsts.SdpType_uint8_t.UUID128;
                PADDING = new byte[PADDING_SIZEa];
                var netOrder = InTheHand.Net.Sockets.BluetoothListener.HostToNetworkOrder(uuid);
                var arr = netOrder.ToByteArray(); // Should be ok to not copying this.
                Debug.Assert(arr.Length == ValueLength);
                value = arr;
            }

            internal uuid_t(UInt32 uuid)
            {
                type = StackConsts.SdpType_uint8_t.UUID32;
                PADDING = new byte[PADDING_SIZEa];
                var arr = BitConverter.GetBytes(uuid);
                value = new byte[ValueLength];
                Array.Copy(arr, value, arr.Length);
            }

            internal uuid_t(UInt16 uuid)
            {
                type = StackConsts.SdpType_uint8_t.UUID16;
                PADDING = new byte[PADDING_SIZEa];
                var arr = BitConverter.GetBytes(uuid);
                value = new byte[ValueLength];
                Array.Copy(arr, value, arr.Length);
            }
        }

        static class TestUuids
        {
            static bool _TestUuidsOnce = false;

            internal static void Test()
            {
                if (!_TestUuidsOnce) {
                    _TestUuidsOnce = true;
                    var u16 = new Structs.uuid_t((UInt16)0x0100);
                    DumpUuidt(u16);
                    var u32 = new Structs.uuid_t((UInt32)0x00000100);
                    DumpUuidt(u32);
                    var u128 = new Structs.uuid_t(BluetoothService.L2CapProtocol);
                    DumpUuidt(u128);
                }
            }

            internal static void DumpUuidt(Structs.uuid_t uuid)
            {
                var sizeOf = Marshal.SizeOf(uuid);
                var p = Marshal.AllocHGlobal(sizeOf);
                Marshal.StructureToPtr(uuid, p, false);
                try {
                    var arr = new byte[sizeOf];
                    Marshal.Copy(p, arr, 0, arr.Length);
                    Console.WriteLine("uuid_t: {0} (len: {1})",
                        BitConverter.ToString(arr), sizeOf);
                } finally {
                    Marshal.FreeHGlobal(p);
                }
            }

        }

        //define SDP_IS_UUID(x) ((x) == SDP_UUID16 || (x) == SDP_UUID32 || (x) ==SDP_UUID128)


        internal struct sdp_list_t
        {
            internal readonly IntPtr/*"sdp_list_t *"*/next;
            internal readonly IntPtr data;
        }

        /*
         * User-visible strings can be in many languages
         * in addition to the universal language.
         *
         * Language meta-data includes language code in ISO639
         * followed by the encoding format. The third field in this
         * structure is the attribute offset for the language.
         * User-visible strings in the specified language can be
         * obtained at this offset.
         */
        struct sdp_lang_attr_t
        {
            internal readonly uint16_t code_ISO639;
            internal readonly uint16_t encoding;
            internal readonly uint16_t base_offset;
        } ;

#if false
/*
 * Profile descriptor is the Bluetooth profile metadata. If a
 * service conforms to a well-known profile, then its profile
 * identifier (UUID) is an attribute of the service. In addition,
 * if the profile has a version number it is specified here.
 */
typedef struct {
	uuid_t uuid;
	uint16_t version;
} sdp_profile_desc_t;

typedef struct {
	uint8_t major;
	uint8_t minor;
} sdp_version_t;

typedef struct {
	uint8_t *data;
	uint32_t data_size;
	uint32_t buf_size;
} sdp_buf_t;
#endif

        internal struct sdp_record_t
        {
            internal readonly uint32_t handle;

            /*
             * Search pattern: a sequence of all UUIDs seen in this record
             */
            internal readonly IntPtr/*"sdp_list_t*"*/ pattern;
            internal readonly IntPtr/*"sdp_list_t*"*/ attrlist;
        }

        internal struct sdp_data_struct__Bytes
        {
            //----
            internal readonly StackConsts.SdpType_uint8_t dtd;
            internal readonly uint16_t attrId;
            // uuid_t is size 20 (= 1 + 3 + 16).
            internal const int SizeOfVal = 20;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SizeOfVal)]
            internal readonly byte[] val;
            //union {
            //    int8_t    int8;
            //    int16_t   int16;
            //    int32_t   int32;
            //    int64_t   int64;
            //    uint128_t int128;
            //    uint8_t   uint8;
            //    uint16_t  uint16;
            //    uint32_t  uint32;
            //    uint64_t  uint64;
            //    uint128_t uint128;
            //    uuid_t    uuid;
            //    char     *str;
            //    sdp_data_t *dataseq;
            //} val;
            internal readonly IntPtr/*"sdp_data_t *"*/next;
            internal readonly int unitSize;

            //----
            static sdp_data_struct__Bytes()
            {
                AssertAll(typeof(sdp_data_struct__Bytes));
            }

            internal static void AssertSize(Type type)
            {
                var expSize = 32 + 2 * (IntPtr.Size - 4);
                var size = Marshal.SizeOf(type);
                if (size != expSize)
                    throw new InvalidOperationException("Wrong size of type " +
                            type.Name + " expected: " + expSize + " but was: " + size); ;
            }
            internal static void AssertAll(Type type)
            {
                AssertSize(type);
                var exp = new[] {
                    new { Name = "dtd",      Offset = 0 },
                    new { Name = "attrId",   Offset = 2 },
                    new { Name = "val",      Offset = 4 },
                    new { Name = "next",     Offset = 24 },
                    new { Name = "unitSize", Offset = 28 },
                };
                foreach (var cur in exp) {
                    var offset = Marshal.OffsetOf(type, cur.Name).ToInt64();
                    if (offset != cur.Offset)
                        throw new InvalidOperationException("Wrong field offset in type " +
                            type.Name + " expected: " + cur.Offset + " but was: " + offset);
                }
            }

            //----
            internal IntPtr ReadIntPtr()
            {
                IntPtr p;
                switch (IntPtr.Size) {
                    case 4:
                        Int32 p32 = BitConverter.ToInt32(val, 0);
                        p = new IntPtr(p32);
                        break;
                    case 8:
                        Int64 p64 = BitConverter.ToInt64(val, 0);
                        p = new IntPtr(p64);
                        break;
                    default:
                        throw new NotImplementedException("Pointer size is not 4 or 8 bytes: " + IntPtr.Size + ".");
                }
                return p;
            }
        }
        internal struct sdp_data_struct__Debug
        {
            static sdp_data_struct__Debug()
            {
                sdp_data_struct__Bytes.AssertSize(typeof(sdp_data_struct__Debug));
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal readonly byte[] all;
        }
        internal struct sdp_data_struct__uuid_t
        {
            static sdp_data_struct__uuid_t()
            {
                sdp_data_struct__Bytes.AssertAll(typeof(sdp_data_struct__uuid_t));
            }

            internal readonly StackConsts.SdpType_uint8_t dtd;
            internal readonly uint16_t attrId;
            internal readonly uuid_t val;
            internal readonly IntPtr/*"sdp_data_t *"*/next;
            internal readonly int unitSize;
        }
        #endregion

        #region sdp_lib.h
        /*
         * a session with an SDP server
         */
        struct sdp_session_t_Real
        {
            int sock;
            int state;
            int local;
            int flags;
            uint16_t tid;	// Current transaction ID
            IntPtr priv;
        }
        #endregion

        #region rfcomm.h
        //* RFCOMM socket address */
        //struct sockaddr_rc { //sizeof=10
        //    sa_family_t	rc_family;
        //    bdaddr_t	rc_bdaddr;
        //    uint8_t		rc_channel;
        //};

        //* RFCOMM socket options */
        /// <summary>
        /// Use with so_RFCOMM_CONNINFO.
        /// </summary>
        struct rfcomm_conninfo
        {
            UInt16 hci_handle;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] dev_class;
        }

#if RFCOMM_TTY
        internal struct rfcomm_dev_req // sizeof=24
        {
            int16_t dev_id;
            uint32_t flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            byte[] src;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            byte[] dst;
            uint8_t channel;
        };

        //
        internal struct rfcomm_dev_info // sizeof=24
        {
            internal Int16 id;
            internal UInt32 flags;
            internal UInt16 state;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BdaddrSize)]
            internal byte[] src;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BdaddrSize)]
            internal byte[] dst;
            internal byte channel;
            //----
            internal const int Offset_id = 0;
            internal const int Offset_flags = 4;
            internal const int Offset_state = 6;
            internal const int Offset_src = 8;
            internal const int Offset_dst = 14;
            internal const int Offset_channel = 20;
            internal const int SizeOf = 24; // per test C program

            //
            internal void AssertLayout()
            {
                StructUtils.AssertSize(ref this, SizeOf);
            }

            //
            public override string ToString()
            {
                return $"rfcomm_dev_info id: {id}, flags: {flags}, state: {state}, src: {BitConverter.ToString(src)}, dst: {BitConverter.ToString(dst)}, channel: {channel}, ";
        };

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        internal struct rfcomm_dev_list_req // sizeof=4
        {
            internal uint16_t dev_num;
            //struct rfcomm_dev_info dev_info[0];

            const int SizeOf = 4; // per test C program

            //
            internal void AssertLayout()
            {
                StructUtils.AssertSize(ref this, SizeOf);
            }
        };
#endif
        #endregion

        static class StructUtils
        {
            internal static void AssertSize<T>(ref T stru, int expectedSize)
                  where T : struct
            {
                int actual = Marshal.SizeOf(stru);
                if (actual != expectedSize)
                    throw new InvalidOperationException(
                        $"Wrong STRUCT size {stru.GetType().Name}, expected {expectedSize} but was {actual}.");
            }
        }


    }
}

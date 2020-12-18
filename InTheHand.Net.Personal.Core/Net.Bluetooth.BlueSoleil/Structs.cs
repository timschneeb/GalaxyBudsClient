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
//
using BtSdkUUIDStru = System.Guid;
using System.Runtime.InteropServices;
using BTDEVHDL = System.UInt32;
using BTSVCHDL = System.UInt32;
using BTCONNHDL = System.UInt32;
using BTSHCHDL = System.UInt32;
using BTSDKHANDLE = System.UInt32;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    // D:\Documents and Settings\alan\My Documents\Suppliers\IVT_BlueSoleil\BlueSoleil_SDK_2.0.5\BlueSoleil_SDK_2.0.5\SDKheaders\include\Btsdk_Stru.h
    static class Structs
    {
        internal struct BtSdkLocalLMPInfoStru
        {
            internal readonly byte lmp_feature_0; /* LMP features */
            internal readonly byte lmp_feature_1, lmp_feature_2, lmp_feature_3,
                lmp_feature_4, lmp_feature_5, lmp_feature_6, lmp_feature_7;
            internal readonly UInt16 manuf_name; /* the name of the manufacturer */
            internal readonly UInt16 lmp_subversion; /* the sub version of the LMP firmware */
            internal readonly byte lmp_version; /* the main version of the LMP firmware */
            internal readonly byte hci_version; /* HCI version */
            internal readonly UInt16 hci_revision; /* HCI revision */
            internal readonly byte country_code; /* country code */

            //
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "fake")]
            internal BtSdkLocalLMPInfoStru(HciVersion fake)
                : this()
            {
                Debug.Assert(fake == HciVersion.Unknown, "This ctor only used to fill with UNKNOWN values.");
                InTheHand.Net.Bluetooth.Widcomm.DEV_VER_INFO.SetManufacturerAndVersionsToUnknown(
                    out manuf_name, out hci_version, out lmp_version);
            }

            internal LmpFeatures GetLmpFeatures()
            {
                var arr = new byte[6];
                arr[0] = lmp_feature_0;
                arr[1] = lmp_feature_1;
                arr[2] = lmp_feature_2;
                arr[3] = lmp_feature_3;
                arr[4] = lmp_feature_4;
                arr[5] = lmp_feature_5;
                arr[6] = lmp_feature_6;
                arr[7] = lmp_feature_7;
                var fea = (LmpFeatures)BitConverter.ToInt64(arr, 0);
                return fea;
            }

        }

        internal struct BtSdkCallbackStru
        {
            internal readonly StackConsts.CallbackType/*UInt16*/ _type; /*type of callback*/
            readonly Delegate _func; /*callback function*/

            internal BtSdkCallbackStru(NativeMethods.Btsdk_Inquiry_Result_Ind_Func inquiryResultIndFunc)
            {
                _type = StackConsts.CallbackType.INQUIRY_RESULT_IND;
                _func = inquiryResultIndFunc;
            }

            internal BtSdkCallbackStru(NativeMethods.Btsdk_Inquiry_Complete_Ind_Func inquiryCompleteIndFunc)
            {
                _type = StackConsts.CallbackType.INQUIRY_COMPLETE_IND;
                _func = inquiryCompleteIndFunc;
            }

            internal BtSdkCallbackStru(NativeMethods.Btsdk_Connection_Event_Ind_Func connectionEventIndFunc)
            {
                _type = StackConsts.CallbackType.CONNECTION_EVENT_IND;
                _func = connectionEventIndFunc;
            }

            internal BtSdkCallbackStru(NativeMethods.Btsdk_UserHandle_Pin_Req_Ind_Func pinReqIndFunc)
            {
                _type = StackConsts.CallbackType.PIN_CODE_IND;
                _func = pinReqIndFunc;
            }
        }//struct

        internal struct BtSdkAppExtSPPAttrStru
        {
            internal readonly Int32 size; /* Size of this structure */
            internal readonly UInt32 sdp_record_handle; /* 32bit integer specifies the SDP service record handle */
            internal readonly BtSdkUUIDStru service_class_128; /* 128bit UUID specifies the service class of this service record */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BTSDK_SERVICENAME_MAXLENGTH)]
            internal readonly byte[] svc_name; /* Service name, in UTF-8 */
            internal readonly byte rf_svr_chnl; /* RFCOMM server channel assigned to this service record */
            internal readonly byte com_index; /* Index of the local COM port assigned to this service record */

            internal BtSdkAppExtSPPAttrStru(BluetoothEndPoint remoteEP)
            {
                size = Marshal.SizeOf(typeof(BtSdkAppExtSPPAttrStru));
                sdp_record_handle = 0;
                if (remoteEP.Port != 0 && remoteEP.Port != -1) {
                    // This does NOT work for Connect.  When tested we got error NO_SERVICE 0xC1. :-(
                    Debug.Fail("This does NOT work for Connect.  When tested we got error NO_SERVICE 0xC1. :-(");
                    service_class_128 = Guid.Empty;
                    rf_svr_chnl = checked((byte)remoteEP.Port);
                } else {
                    service_class_128 = remoteEP.Service;
                    rf_svr_chnl = 0;
                }
                svc_name = new byte[StackConsts.BTSDK_SERVICENAME_MAXLENGTH];
                com_index = 0;
            }

            internal BtSdkAppExtSPPAttrStru(Guid serviceClass)
            {
                size = Marshal.SizeOf(typeof(BtSdkAppExtSPPAttrStru));
                sdp_record_handle = 0;
                service_class_128 = serviceClass;
                rf_svr_chnl = 0;
                svc_name = new byte[StackConsts.BTSDK_SERVICENAME_MAXLENGTH];
                com_index = 0;
            }

            public override string ToString()
            {
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                StringBuilder bldr = new StringBuilder(this.GetType().Name);
                bldr.AppendFormat(culture, " sdp_record_handle: 0x{0:X}", sdp_record_handle);
                bldr.AppendLine();
                bldr.AppendFormat(culture, "   service_class_128: {0}", service_class_128);
                bldr.AppendFormat(culture, " svc_name: \"{0}...\"", ToPrintable(svc_name[0]));
                bldr.AppendLine();
                bldr.AppendFormat(culture, "   rf_svr_chnl: {0}", rf_svr_chnl);
                bldr.AppendFormat(culture, " com_index: {0}", com_index);
                bldr.Append(".");
                return bldr.ToString();
            }

            private static string ToPrintable(byte b)
            {
                return char.IsControl((char)b)
                    ? "\\x" + b.ToString("X2", CultureInfo.InvariantCulture)
                    : ((char)b).ToString();
            }
        }

        /* lParam for SPP */
        internal struct BtSdkSPPConnParamStru
        {
            Int32 size;
            UInt16 mask;	//Reserved set 0
            byte com_index;

            internal BtSdkSPPConnParamStru(UInt32 osComPort)
            {
                this.size = System.Runtime.InteropServices.Marshal.SizeOf(
                    typeof(Structs.BtSdkSPPConnParamStru));
                this.com_index = checked((byte)osComPort);
                this.mask = 0;
            }
        }

        internal struct BtSdkSDPSearchPatternStru
        {
            enum UuidTypeMask : uint
            {
                BTSDK_SSPM_UUID16 = 0x0001,
                BTSDK_SSPM_UUID32 = 0x0002,
                BTSDK_SSPM_UUID128 = 0x0004,
            }

            readonly UuidTypeMask mask; /*Specifies the valid bytes in the uuid*/
            readonly BtSdkUUIDStru uuid; /*UUID value*/

            internal BtSdkSDPSearchPatternStru(Guid serviceClass)
            {
                uuid = serviceClass;
                mask = UuidTypeMask.BTSDK_SSPM_UUID128;
                Debug.Assert(mask == UuidTypeMask.BTSDK_SSPM_UUID128);
            }
        }

        /* Remote service record attributes */
        internal struct BtSdkRemoteServiceAttrStru
        {
            internal readonly StackConsts.AttributeLookup mask; /*Decide which parameter to be retrieved*/
            //union {
            internal readonly UInt16 svc_class; /* For Compatibility */
            //BTUINT16 service_class;
            //}; /*Type of this service record*/
            internal readonly BTDEVHDL dev_hdl; /*Handle to the remote device which provides this service.*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BTSDK_SERVICENAME_MAXLENGTH)]
            internal readonly byte[] svc_name; /*Service name in UTF-8*/
            internal readonly IntPtr ext_attributes; /*Free by the APP*/
            internal readonly UInt16 status;

            public BtSdkRemoteServiceAttrStru(StackConsts.AttributeLookup mask)
            {
                this.mask = mask;
                //
                this.svc_class = 0; this.dev_hdl = 0;
                this.svc_name = new byte[StackConsts.BTSDK_SERVICENAME_MAXLENGTH];
                this.ext_attributes = IntPtr.Zero;
                this.status = 0;
            }

            /// <summary>
            /// for Test.
            /// </summary>
            public BtSdkRemoteServiceAttrStru(StackConsts.AttributeLookup mask,
                UInt16 svc_class, byte[] svcName, IntPtr pExtAttr)
            {
                this.mask = mask;
                this.svc_class = svc_class;
                this.svc_name = new byte[StackConsts.BTSDK_SERVICENAME_MAXLENGTH];
                svcName.CopyTo(this.svc_name, 0);
                //
                this.status = 0;
                this.ext_attributes = pExtAttr;
                this.dev_hdl = 0;
            }

        }

        internal struct BtSdkRmtSPPSvcExtAttrStru
        {
            internal readonly Int32 size; /*Size of BtSdkRmtSPPSvcExtAttrStru*/
            internal readonly byte server_channel; /*Server channel value of this SPP service record*/

            /// <summary>
            /// for Test.
            /// </summary>
            internal BtSdkRmtSPPSvcExtAttrStru(byte scn)
            {
                size = Marshal.SizeOf(typeof(BtSdkRmtSPPSvcExtAttrStru));
                server_channel = scn;
            }
        }

        internal struct BtSdkConnectionPropertyStru
        {
            //UInt32 role : 2;
            //UInt32 result : 30;
            internal readonly UInt32 role_AND_result;
            // "Possible roles for member 'role' in _BtSdkConnectionPropertyStru"
            //#define BTSDK_CONNROLE_INITIATOR				0x2
            //#define BTSDK_CONNROLE_ACCEPTOR					0x1
            //
            internal readonly BTDEVHDL device_handle;
            internal readonly BTSVCHDL service_handle;
            internal readonly UInt16 service_class;
            internal readonly UInt32 duration;
            internal readonly UInt32 received_bytes;
            internal readonly UInt32 sent_bytes;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            private BtSdkConnectionPropertyStru(Version shutUpCompiler)
            {
                role_AND_result = 0;
                device_handle = 0;
                service_handle = 0;
                service_class = 0;
                duration = 0;
                received_bytes = 0;
                sent_bytes = 0;
            }

            internal StackConsts.BTSDK_CONNROLE Role
            {
                get
                {
                    return (StackConsts.BTSDK_CONNROLE)(
                        role_AND_result & StackConsts.BTSDK_CONNROLE_Mask);
                }
            }

        }

        //* lParam for SPP */
        //struct BtSdkSPPConnParamStru
        //{
        //    UInt32 size;
        //    UInt16 mask; //Reserved set 0
        //    byte com_index;
        //}

        internal struct BtSdkRmtDISvcExtAttrStru
        {
            internal readonly Int32 size;
            internal readonly UInt16 mask;
            internal readonly UInt16 spec_id;
            internal readonly UInt16 vendor_id;
            internal readonly UInt16 product_id;
            internal readonly UInt16 version;
            [MarshalAs(UnmanagedType.I1)]
            internal readonly bool primary_record;
            internal readonly UInt16 vendor_id_source;
            internal readonly UInt16 list_size;
            internal readonly byte str_url_list__ARRAY;
            //
            internal const int StackMiscountsPaddingSize = -1;

            internal BtSdkRmtDISvcExtAttrStru(
                UInt16 spec_id, UInt16 vendor_id, UInt16 product_id, UInt16 version,
                bool primary_record, UInt16 vendor_id_source,
                UInt16 mask)
            {
                this.mask = mask;
                this.spec_id = spec_id;
                this.vendor_id = vendor_id;
                this.product_id = product_id;
                this.version = version;
                this.primary_record = primary_record;
                this.vendor_id_source = vendor_id_source;
                this.size = Marshal.SizeOf(typeof(BtSdkRmtDISvcExtAttrStru)) + StackMiscountsPaddingSize;
                //
                this.list_size = 0;
                this.str_url_list__ARRAY = 0;
            }
        }


        internal struct BtSdkRemoteDevicePropertyStru
        {
            internal readonly Mask mask;								/*Specifies members available.*/
            internal readonly BTDEVHDL dev_hdl;							/*Handle assigned to the device record*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BTSDK_BDADDR_LEN)]
            internal readonly byte[] bd_addr;			/*BT address of the device record*/
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BTSDK_DEVNAME_LEN)]
            internal readonly byte[] name;			/*Name of the device record, must be in UTF-8*/
            internal readonly UInt32 dev_class;							/*Device class*/
            internal readonly BtSdkRemoteLMPInfoStru lmp_info;			/* LMP info */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BTSDK_LINKKEY_LEN)]
            internal readonly byte[] link_key;		/* link key for this device. */

            /* Possible values for "mask" member of BtSdkRemoteDevicePropertyStru structure. */
            internal enum Mask : uint // BTUINT32
            {
                Handle = 0x0001,
                Address = 0x0002,
                Name = 0x0004,
                Class = 0x0008,
                LmpInfo = 0x0010,
                LinkKey = 0x0020,
            }
        }

        internal struct BtSdkRemoteLMPInfoStru
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal readonly byte[] lmp_feature; // Would using 'LmpFeatures' affect alignment??
            internal readonly Manufacturer manuf_name;
            internal readonly UInt16 lmp_subversion;
            internal readonly LmpVersion lmp_version;
        }

        internal struct BtSdkRmtHidSvcExtAttrStru_HACK
        {
            internal readonly Int32 size;
            internal readonly UInt16 mask;
            internal readonly UInt16 deviceReleaseNumber;
            internal readonly UInt16 unknown0a;
            internal readonly byte deviceSubclass;
            internal readonly byte countryCode;
            internal readonly UInt32 unknownA;
            internal readonly UInt32 unknownB;
            internal readonly UInt32 unknownC;
            internal readonly UInt16 unknownD;
            internal readonly byte unknownE;
            //
            internal const int StackMiscountsPaddingSize = -1;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "shutUpCompiler")]
            private BtSdkRmtHidSvcExtAttrStru_HACK(Version shutUpCompiler)
            {
                size = 0;
                mask = 0;
                deviceReleaseNumber = 0;
                unknown0a = 0;
                deviceSubclass = 0;
                countryCode = 0;
                unknownA = 0;
                unknownB = 0;
                unknownC = 0;
                unknownD = 0;
                unknownE = 0;
            }
        }

    }
}

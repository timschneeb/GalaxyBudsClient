// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    static class StackConsts
    {
        /* Max size value used in service attribute structures */
        internal const int BTSDK_SERVICENAME_MAXLENGTH = 80;
        internal const int BTSDK_MAX_SUPPORT_FORMAT = 6;/* OPP format number */
        internal const int BTSDK_PATH_MAXLENGTH = 256;/* Shall not be larger than FTP_MAX_PATH and OPP_MAX_PATH */
        internal const int BTSDK_CARDNAME_MAXLENGTH = 256;/* Shall not be larger than OPP_MAX_NAME */
        internal const int BTSDK_PACKETTYPE_MAXNUM = 10;/* PAN supported network packet type */

        /* Max size value used in device attribute structures */
        internal const int BTSDK_DEVNAME_LEN = 64;/* Shall not be larger than MAX_NAME_LEN */
        internal const int BTSDK_SHORTCUT_NAME_LEN = 100;
        internal const int BTSDK_BDADDR_LEN = 6;
        internal const int BTSDK_LINKKEY_LEN = 16;
        internal const int BTSDK_PINCODE_LEN = 16;

        /* Invalid handle value for all handle type */
        internal const UInt32 BTSDK_INVALID_HANDLE = 0x00000000;

        internal const UInt16 BTSDK_BLUETOOTH_STATUS_FLAG = 0x0002; //status change about Bluetooth

        //status change about Bluetooth
        internal enum BTSDK_BTSTATUS : uint
        {
            TurnOn = 0x0001,
            TurnOff = 0x0002,
            HwPlugged = 0x0003,
            HwPulled = 0x0004
        }

        // "Possible roles for member 'role' in _BtSdkConnectionPropertyStru"
        internal enum BTSDK_CONNROLE
        {
            Initiator = 0x2,
            Acceptor = 0x1
        }

        internal const UInt32 BTSDK_CONNROLE_Mask = 0x03;


        /* Type of Callback Indication */
        internal enum CallbackType : ushort
        {
            INQUIRY_RESULT_IND = 0x04,
            INQUIRY_COMPLETE_IND = 0x05,
            //
            CONNECTION_EVENT_IND = 0x09,
            //
            PIN_CODE_IND = 0x00,
            AUTHORIZATION_IND = 0x06,
            LINK_KEY_NOTIF_IND = 0x02,
            AUTHENTICATION_FAIL_IND = 0x03,
        }

        /* Discovery Mode for Btsdk_SetDiscoveryMode() and Btsdk_GetDiscoveryMode() */
        [Flags]
        internal enum DiscoveryMode : ushort
        {
            /// <summary>
            /// &#x201C;Sets the device into general discoverable mode. This is
            /// the default discoverable mode.&#x201D;
            /// </summary>
            BTSDK_GENERAL_DISCOVERABLE = 0x01,
            /// <summary>
            /// &#x201C;Sets the device into limited discoverable mode. If this
            /// value is specified, BTSDK_GENERAL_DISCOVERABLE
            /// mode value is ignored by BlueSoleil.&#x201D;
            /// </summary>
            BTSDK_LIMITED_DISCOVERABLE = 0x02,
            /// <summary>
            /// &#x201C;Makes the device discoverable. This is equivalent to
            /// BTSDK_GENERAL_DISCOVERABLE.&#x201D;
            /// </summary>
            BTSDK_DISCOVERABLE = BTSDK_GENERAL_DISCOVERABLE,
            /// <summary>
            /// &#x201C;Makes the device connectable. This is the default
            /// connectable mode.&#x201D;
            /// </summary>
            BTSDK_CONNECTABLE = 0x04,
            /// <summary>
            /// &#x201C;Makes the device pairable. This is the default pairable
            /// mode.&#x201D;
            /// </summary>
            BTSDK_PAIRABLE = 0x08,
        }

        //
        internal const DiscoveryMode BTSDK_DISCOVERY_DEFAULT_MODE = (DiscoveryMode.BTSDK_DISCOVERABLE | DiscoveryMode.BTSDK_CONNECTABLE | DiscoveryMode.BTSDK_PAIRABLE);


        /* Type of Connection Event */
        internal enum ConnectionEventType : ushort
        {
            /// <summary>
            /// &#x201C;A remote device connects to a local service record.&#x201D;
            /// </summary>
            CONN_IND = 0x01,
            /// <summary>
            /// &#x201C;The remote device disconnects the connection, or the
            /// connection is lost due to radio communication problems,
            /// e.g. the remote device is out of communication range.&#x201D;
            /// </summary>
            DISC_IND = 0x02,
            /// <summary>
            /// &#x201C;A local device connects to a remote service record.&#x201D;
            /// </summary>
            CONN_CFM = 0x07,
            /// <summary>
            /// &#x201C;The local device disconnects the connection from remote
            /// service.&#x201D;
            /// </summary>
            DISC_CFM = 0x08,

            /* Definitions for Compatibility */
            //#define BTSDK_APP_EV_CONN						0x01	
            //#define BTSDK_APP_EV_DISC						0x02	
        }

        //Call back user priority
        const uint BTSDK_CLIENTCBK_PRIORITY_HIGH = 3;
        const uint BTSDK_CLIENTCBK_PRIORITY_MEDIUM = 2;

        //Whether user handle pin code and authorization callback
        internal enum CallbackResult : byte
        {
            Handled = 1,
            NotHandled = 0
        }

        /* Authorization Result */
        //#define BTSDK_AUTHORIZATION_GRANT				0x01
        //#define BTSDK_AUTHORIZATION_DENY				0x02

        /// <summary>
        /// "Possible flags for member 'mask' in _BtSdkRemoteServiceAttrStru"
        /// </summary>
        [Flags]
        internal enum AttributeLookup : ushort
        {
            ServiceName = 0x0001,
            ExtAttributes = 0x0002,
        }

        //==
        /* Parameters for Btsdk_PlugOutVComm and Btsdk_PlugInVComm */
        internal enum COMM_SET : uint
        {
            UsageType = 0x00000001,
            Record = 0x00000010
        }

    } //class
}
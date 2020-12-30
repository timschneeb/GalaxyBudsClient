// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.StackConsts
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    static class StackConsts
    {
        internal const int Class_of_Device_SIZE = 3;

        #region BTTypes.h
        internal const int BD_ADDR_SIZE = 6;
        //   /* The following type declaration represents the structure of a      */
        //   /* single Bluetooth Board Address.                                   */
        //typedef struct _tagBD_ADDR_t
        //{
        //   Byte_t BD_ADDR0;
        //   Byte_t BD_ADDR1;
        //   Byte_t BD_ADDR2;
        //   Byte_t BD_ADDR3;
        //   Byte_t BD_ADDR4;
        //   Byte_t BD_ADDR5;
        //} BD_ADDR_t;

        /// <summary>
        /// The following constant represents the Maximum Number of Bytes     
        /// that a Name Variable an occupy.  It should be noted that for      
        /// Names that have lengths less than this maximum, a NULL terminating
        /// character should be used and IS counted as part of the Length.    
        /// For a Name of 248 Bytes, there is NO NULL terminating character   
        /// at the end of the Name Data.                                      
        /// </summary>
        internal const int MAX_NAME_LENGTH = 248;
        #endregion

        #region HCITypes.h
        internal enum HCI_DriverType
        {
            /// <summary>
            /// COM/Serial Port HCI Connection Type.
            /// </summary>
            COMM,
            /// <summary>
            /// USB HCI Connection Type.
            /// </summary>
            USB
        }

        internal enum HCI_PacketType
        {
            /// <summary>
            /// Simple HCI Command Packet Type.
            /// </summary>
            HCICommandPacket = 0x01,
            /// <summary>
            /// HCI ACL Data Packet Type.
            /// </summary>
            HCIACLDataPacket = 0x02,
            /// <summary>
            /// HCI SCO Data Packet Type.
            /// </summary>
            HCISCODataPacket = 0x03,
            /// <summary>
            /// HCI eSCO Data Packet Type.
            /// </summary>
            HCIeSCODataPacket = 0x03,
            /// <summary>
            /// HCI Event Packet Type.
            /// </summary>
            HCIEventPacket = 0x04,
            /// <summary>
            /// Starting Point for Additional
            /// HCI Packet Types that are
            /// Implementation Specific (for
            /// example RS-232 HCI defines
            /// two Additional HCI Packet
            /// Types which are numbered
            /// 0x05 and 0x06 for RS-232 HCI.
            /// </summary>
            __HCIAdditional = 0x05
        }

        /// <summary>
        /// HCI Error Code Definitions/Constants.
        /// </summary>
        internal enum HCI_ERROR_CODE : byte
        {
            NO_ERROR = 0x00,
            UNKNOWN_HCI_COMMAND = 0x01,
            NO_CONNECTION = 0x02,
            HARDWARE_FAILURE = 0x03,
            PAGE_TIMEOUT = 0x04,
            AUTHENTICATION_FAILURE = 0x05,
            KEY_MISSING = 0x06,
            MEMORY_FULL = 0x07,
            CONNECTION_TIMEOUT = 0x08,
            MAX_NUMBER_OF_CONNECTIONS = 0x09,
            MAX_NUMBER_OF_SCO_CONNECTIONS_TO_A_DEVICE = 0x0A,
            ACL_CONNECTION_ALREADY_EXISTS = 0x0B,
            COMMAND_DISALLOWED = 0x0C,
            HOST_REJECTED_DUE_TO_LIMITED_RESOURCES = 0x0D,
            HOST_REJECTED_DUE_TO_SECURITY_REASONS = 0x0E,
            HOST_REJECTED_DUE_TO_REMOTE_DEVICE_IS_PERSONAL = 0x0F,
            HOST_TIMEOUT = 0x10,
            UNSUPPORTED_FEATURE_OR_PARAMETER_VALUE = 0x11,
            INVALID_HCI_COMMAND_PARAMETERS = 0x12,
            OTHER_END_TERMINATED_CONNECTION_USER_ENDED = 0x13,
            OTHER_END_TERMINATED_CONNECTION_LOW_RESOURCES = 0x14,
            OTHER_END_TERMINATED_CONNECTION_ABOUT_TO_PWR_OFF = 0x15,
            CONNECTION_TERMINATED_BY_LOCAL_HOST = 0x16,
            REPEATED_ATTEMPTS = 0x17,
            PAIRING_NOT_ALLOWED = 0x18,
            UNKNOWN_LMP_PDU = 0x19,
            UNSUPPORTED_REMOTE_FEATURE = 0x1A,
            SCO_OFFSET_REJECTED = 0x1B,
            SCO_INTERVAL_REJECTED = 0x1C,
            SCO_AIR_MODE_REJECTED = 0x1D,
            INVALID_LMP_PARAMETERS = 0x1E,
            UNSPECIFIED_ERROR = 0x1F,
            UNSUPPORTED_LMP_PARAMETER_VALUE = 0x20,
            ROLE_CHANGE_NOT_ALLOWED = 0x21,
            LMP_RESPONSE_TIMEOUT = 0x22,
            LMP_ERROR_TRANSACTION_COLLISION = 0x23,

            /* HCI Error Code Definitions/Constants (Version 1.1).               */
            LMP_PDU_NOT_ALLOWED = 0x24,
            ENCRYPTION_MODE_NOT_ACCEPTABLE = 0x25,
            UNIT_KEY_USED = 0x26,
            QOS_NOT_SUPPORTED = 0x27,
            INSTANT_PASSED = 0x28,
            PAIRING_WITH_UNIT_KEY_NOT_SUPPORTED = 0x29,

            /* HCI Error Code Definitions/Constants (Version 1.2).               */
            SUCCESS = 0x00,
            UNKNOWN_CONNECTION_IDENTIFIER = 0x02,
            PIN_MISSING = 0x06,
            MEMORY_CAPACITY_EXCEEDED = 0x07,
            CONNECTION_LIMIT_EXCEEDED = 0x09,
            SYNCHRONOUS_CONNECTION_LIMIT_TO_A_DEVICE_EXCEEDED = 0x0A,
            CONNECTION_REJECTED_DUE_TO_LIMITED_RESOURCES = 0x0D,
            CONNECTION_REJECTED_DUE_TO_SECURITY_REASONS = 0x0E,
            CONNECTION_REJECTED_DUE_TO_UNACCEPTABLE_BD_ADDR = 0x0F,
            CONNECTION_ACCEPT_TIMEOUT_EXCEEDED = 0x10,
            REMOTE_USER_TERMINATED_CONNECTION = 0x13,
            REMOTE_DEVICE_TERMINATED_CONNECTION_LOW_RESOURCES = 0x14,
            REMOTE_DEVICE_TERMINATED_CONNECTION_DUE_TO_PWR_OFF = 0x15,
            LINK_KEY_CANNOT_BE_CHANGED = 0x26,
            REQUESTED_QOS_NOT_SUPPORTED = 0x27,
            DIFFERENT_TRANSACTION_COLLISION = 0x2A,
            QOS_UNACCEPTABLE_PARAMETER = 0x2C,
            QOS_REJECTED = 0x2D,
            CHANNEL_CLASSIFICATION_NOT_SUPPORTED = 0x2E,
            INSUFFICIENT_SECURITY = 0x2F,
            PARAMETER_OUT_OF_MANDATORY_RANGE = 0x30,
            ROLE_SWITCH_PENDING = 0x32,
            RESERVED_SLOT_VIOLATION = 0x34,
            ROLE_SWITCH_FAILED = 0x35,

            /* HCI Error Code Definitions/Constants (Version 2.1).               */
            EXTENDED_INQUIRY_RESPONSE_TOO_LARGE = 0x36,
            SECURE_SIMPLE_PAIRING_NOT_SUPPORTED_BY_HOST = 0x37,
            HOST_BUSY_PAIRING = 0x38,
        }

        /* The following Constants represent the defined Bluetooth HCI Link  */
        /* Type Types.                                                       */
        internal enum HCI_LINK_TYPE : byte
        {
            SCO_CONNECTION = 0x00,
            ACL_CONNECTION = 0x01,

            /* The following Constants represent the defined Bluetooth HCI Link  */
            /* Type Types (Version 1.2).                                         */
            ESCO_CONNECTION = 0x02
        }

        /// <summary>
        /// The following Constants represent the defined Bluetooth HCI SCO Packet Types.
        /// </summary>
        [Flags]
        internal enum HCI_PACKET_SCO_TYPE__u16
        {
            HV1 = 0x0020,
            HV2 = 0x0040,
            HV3 = 0x0080,
        }
        internal const HCI_PACKET_SCO_TYPE__u16 HCI_PACKET_SCO_TYPE__AllThree
            = HCI_PACKET_SCO_TYPE__u16.HV1
            | HCI_PACKET_SCO_TYPE__u16.HV2
            | HCI_PACKET_SCO_TYPE__u16.HV3;

        //   /* The following Constants represent the defined Bluetooth HCI       */
        //   /* Synchronous Connection Packet Types (SCO and eSCO in Version 1.2  */
        //   /* only).                                                            */
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_HV1                      0x0001
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_HV2                      0x0002
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_HV3                      0x0004
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_EV3                      0x0008
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_EV4                      0x0010
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_EV5                      0x0020

        //   /* The following constants represent the defined Bluetooth HCI eSCO  */
        //   /* extended packet types (Version 2.0).                              */
        //   /* * NOTE * These types are different in that they specify packet    */
        //   /*          type that MAY NOT be used (rather than packet types that */
        //   /*          MAY be used).                                            */
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_2_EV3_MAY_NOT_BE_USED    0x0040
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_3_EV3_MAY_NOT_BE_USED    0x0080

        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_2_EV5_MAY_NOT_BE_USED    0x0100
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_3_EV5_MAY_NOT_BE_USED    0x0200

        //   /* The following constants are placeholders for the reserved bits    */
        //   /* in the packet type field.                                         */
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED0                0x0400
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED1                0x0800
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED2                0x1000
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED3                0x2000
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED4                0x4000
        //#define HCI_PACKET_SYNCHRONOUS_CONNECTION_TYPE_RESERVED5                0x8000

        #endregion

        #region HCICommT.h
        /// <summary>
        /// The following type declaration defines the HCI Serial Protocol
        /// that will be used as the physical HCI Transport Protocol on the
        /// actual COM Port that is opened.  This type declaration is used in
        /// the HCI_COMMDriverInformation_t structure that is required when
        /// an HCI COMM Port is opened.
        /// </summary>
        internal enum HCI_COMM_Protocol
        {
            UART,
            UART_RTS_CTS,
            BCSP,
            BCSP_Muzzled
        }

        internal const int HCI_COMM_DRIVER_NAME_MAX = 4;
        #endregion

        #region BSCAPI.h
        /// <summary>
        /// The following definitions, represent BIT Flags that can be used 
        /// with the BSC_Initialize() function.  These Bit Flags can be used
        /// to inform the Bluetooth Stack Controller of various Requested   
        /// options.                                                        
        /// </summary>
        [Flags]
        internal enum BSC_INITIALIZE_FLAGs : uint
        {
            NO_L2CAP = 0x00000001,
            NO_SCO = 0x00000002,
            NO_SDP = 0x00000004,
            NO_RFCOMM = 0x00000008,
            NO_GAP = 0x00001000,
            NO_SPP = 0x00002000,
            NO_GOEP = 0x00004000,
            NO_OTP = 0x00008000,
        }
        #endregion

        #region HCIAPI.h
        internal enum HCI_Event_Type_t
        {
            etInquiry_Complete_Event,
            etInquiry_Result_Event,
            etConnection_Complete_Event,
            etConnection_Request_Event,
            etDisconnection_Complete_Event,
            etAuthentication_Complete_Event,
            etRemote_Name_Request_Complete_Event,
            etEncryption_Change_Event,
            etChange_Connection_Link_Key_Complete_Event,
            etMaster_Link_Key_Complete_Event,
            etRead_Remote_Supported_Features_Complete_Event,
            etRead_Remote_Version_Information_Complete_Event,
            etQoS_Setup_Complete_Event,
            etHardware_Error_Event,
            etFlush_Occurred_Event,
            etRole_Change_Event,
            etNumber_Of_Completed_Packets_Event,
            etMode_Change_Event,
            etReturn_Link_Keys_Event,
            etPIN_Code_Request_Event,
            etLink_Key_Request_Event,
            etLink_Key_Notification_Event,
            etLoopback_Command_Event,
            etData_Buffer_Overflow_Event,
            etMax_Slots_Change_Event,
            etRead_Clock_Offset_Complete_Event,
            etConnection_Packet_Type_Changed_Event,
            etQoS_Violation_Event,
            etPage_Scan_Mode_Change_Event,
            etPage_Scan_Repetition_Mode_Change_Event,
            etBluetooth_Logo_Testing_Event,
            etVendor_Specific_Debug_Event,
            etDevice_Reset_Event,
            etFlow_Specification_Complete_Event,
            etInquiry_Result_With_RSSI_Event,
            etRead_Remote_Extended_Features_Complete_Event,
            etSynchronous_Connection_Complete_Event,
            etSynchronous_Connection_Changed_Event,
            etSniff_Subrating_Event,
            etExtended_Inquiry_Result_Event,
            etEncryption_Key_Refresh_Complete_Event,
            etIO_Capability_Request_Event,
            etIO_Capability_Response_Event,
            etUser_Confirmation_Request_Event,
            etUser_Passkey_Request_Event,
            etRemote_OOB_Data_Request_Event,
            etSimple_Pairing_Complete_Event,
            etLink_Supervision_Timeout_Changed_Event,
            etEnhanced_Flush_Complete_Event,
            etUser_Passkey_Notification_Event,
            etKeypress_Notification_Event,
            etRemote_Host_Supported_Features_Notification_Event,
            etDevice_Power_Event,
            etFlight_Mode_Turn_Off_Event
        }
        #endregion

        #region GAPAPI.h
        /* The following enumerated type represents the supported Discovery  */
        /* Modes that a Bluetooth Device can be set to.  These types are     */
        /* used with the GAP_Set_Discoverability_Mode() and the              */
        /* GAP_Query_Discoverability_Mode() functions.                       */
        internal enum GAP_Discoverability_Mode
        {
            NonDiscoverableMode,
            LimitedDiscoverableMode,
            GeneralDiscoverableMode
        }

        /* The following enumerated type represents the supported            */
        /* Connectability Modes that a Bluetooth Device can be set to.  These*/
        /* types are used with the GAP_Set_Connectability_Mode() and the     */
        /* GAP_Query_Connectability_Mode() functions.                        */
        internal enum GAP_Connectability_Mode
        {
            NonConnectableMode,
            ConnectableMode
        }

        /* The following enumerated type represents the supported            */
        /* Pairability Modes that a Bluetooth Device can be set to.  These   */
        /* types are used with the GAP_Set_Pairability_Mode() and the        */
        /* GAP_Query_Pairability_Mode() functions.                           */
        internal enum GAP_Pairability_Mode
        {
            NonPairableMode,
            PairableMode
        }

        /* The following enumerated type represents the supported            */
        /* Authentication Modes that a Bluetooth Device can be set to.  These*/
        /* types are used with the GAP_Set_Authentication_Mode() and the     */
        /* GAP_Query_Authentication_Mode() functions.                        */
        internal enum GAP_Authentication_Mode
        {
            Disabled,
            Enabled
        }

        /* The following enumerated type represents the supported            */
        /* Encryption Modes that a Bluetooth Device can be set to.  These    */
        /* types are used with the GAP_Set_Encryption_Mode() and the         */
        /* GAP_Query_Encryption_Mode() functions.                            */
        internal enum GAP_Encryption_Mode
        {
            Disabled,
            Enabled
        }

        /* The following enumerated type represents the supported Bonding    */
        /* Types that the Bluetooth Device can be instructed to perform.     */
        /* These types are used with the GAP_Initiate_Bonding() function.    */
        internal enum GAP_Bonding_Type
        {
            General,
            Dedicated
        }

        /* The following enumerated type represents the supported Inquiry    */
        /* Types that can be used when performing an Inquiry Process of      */
        /* Bluetooth Device(s).  These types are used with the               */
        /* GAP_Perform_Inquiry() function.                                   */
        internal enum GAP_Inquiry_Type
        {
            GeneralInquiry,
            LimitedInquiry
        }

        /* The following enumerated type represents the GAP Event Reason     */
        /* (and valid Data) and is used with the GAP Event Callback.         */
        internal enum GAP_Event_Type
        {
            Inquiry_Result,
            Encryption_Change_Result,
            Authentication,
            Remote_Name_Result,
            Inquiry_Entry_Result
        }

        /* The following enumerated type is used with the Authentication     */
        /* Event Data Structure and defines the reason that the              */
        /* Authentication Callback was issued, this defines what data in the */
        /* structure is pertinant.                                           */
        internal enum GAP_Authentication_Event_Type
        {
            LinkKeyRequest,
            PINCodeRequest,
            AuthenticationStatus,
            LinkKeyCreation,
            IOCapabilityRequest,
            UserConfirmationRequest,
            UserPasskeyRequest,
            RemoteOOBRequest,
            PasskeyNotification,
            KeypressNotification,
            SimplePairingComplete,
        }

        /* The following enumerated type represents the different            */
        /* Authentication Methods that can be used.                          */
        internal enum GAP_Authentication_Type_t
        {
            atLinkKey,
            atPINCode,
            atUserConfirmation,
            atPassKey,
            atOutOfBandData,
        }

        #endregion

        #region SPPAPI.h
        /// <summary>
        /// The following constants represent the Port Open Status Values
        /// that are possible in the SPP Open Port Confirmation Event Data
        /// Information.
        /// </summary>
        internal enum SPP_OPEN_PORT_STATUS : uint
        {
            Success = 0x00,
            ConnectionTimeout = 0x01,
            ConnectionRefused = 0x02,
            UnknownError = 0x03,
        }


        /// <summary>
        /// SPP Event API Types.
        /// </summary>
        internal enum SPP_Event_Type
        {
            Port_Open_Indication,
            Port_Open_Confirmation,
            Port_Close_Port_Indication,
            Port_Status_Indication,
            Port_Data_Indication,
            Port_Transmit_Buffer_Empty_Indication,
            Port_Line_Status_Indication,
            Port_Send_Port_Information_Indication,
            Port_Send_Port_Information_Confirmation,
            Port_Query_Port_Information_Indication,
            Port_Query_Port_Information_Confirmation,
            Port_Open_Request_Indication
        }
        #endregion

        #region SDPAPI.h
        /// <summary>
        /// The following enumerated type represents the different Connection
        /// Modes that are supported by SDP.  These constants are used with
        /// the SDP_Set_Disconnect_Mode() function.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal enum SDP_Disconnect_Mode
        {
            Automatic,
            Manual
        }

        /// <summary>
        /// The following enumerated type represents all the allowable SDP
        /// Data Element Types that can be used with the SDP API.
        /// </summary>
        internal enum SDP_Data_Element_Type
        {
            NIL,
            NULL,
            UnsignedInteger1Byte,
            UnsignedInteger2Bytes,
            UnsignedInteger4Bytes,
            UnsignedInteger8Bytes,
            UnsignedInteger16Bytes,
            SignedInteger1Byte,
            SignedInteger2Bytes,
            SignedInteger4Bytes,
            SignedInteger8Bytes,
            SignedInteger16Bytes,
            TextString,
            Boolean,
            URL,
            UUID_16,
            UUID_32,
            UUID_128,
            Sequence,
            Alternative
        }

        /// <summary>
        /// The following enumerated type represents all the allowable SDP
        /// Request Response Data Types that will be returned in the SDP
        /// Response Callback Function.
        /// </summary>
        internal enum SDP_Response_Data_Type
        {
            Timeout,
            ConnectionError,
            ErrorResponse,
            ServiceSearchResponse,
            ServiceAttributeResponse,
            ServiceSearchAttributeResponse
        }

        #endregion

    }
}

// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.Structs
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

//using Byte_t = System.Byte;
//using Word_t = System.UInt16;   /* Generic 16 bit Container.  */
//using DWord_t = System.UInt32;  /* Generic 32 bit Container.  */

//
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using Utils;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    static class Structs
    {
#pragma warning disable 649 //Field '...' is never assigned to, and will always have its default value ...

        #region HCITypes.h
        /* pack(1) */
        //internal struct HCI_DriverInformation_t
        //{
        //    internal readonly StackConsts.HCI_DriverType_t DriverType;
        //    union
        //    {
        //      HCI_COMMDriverInformation_t COMMDriverInformation;
        //      HCI_USBDriverInformation_t  USBDriverInformation;
        //    } DriverInformation;
        //}
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct HCI_DriverInformation__HCI_COMMDriverInformation
        {
            internal readonly StackConsts.HCI_DriverType DriverType;
            private const int SizeOfOuterStruct = 4;
            //
            /* HCI_COMMDriverInformation_t */
            /// <summary>
            /// Physical Size of this structure.
            /// </summary>
            internal uint DriverInformationSize;
            /// <summary>
            /// Physical COM Port Number of the Windows COM Port to
            /// Open.
            /// </summary>
            internal uint COMPortNumber;
            /// <summary>
            /// Baud Rate to Open COM Port.
            /// </summary>
            internal uint BaudRate;
            /// <summary>
            /// HCI Protocol that will be used for communication over
            /// Opened COM Port.
            /// </summary>
            internal StackConsts.HCI_COMM_Protocol Protocol;
            /// <summary>
            /// Time (In Milliseconds) to Delay after the Port is
            /// opened before any data is sent over the Port.  This
            /// member is present because some PCMCIA/Compact Flash
            /// Cards have been seen that require a delay because the
            /// card does not function for some specified period of
            /// time.
            /// </summary>
            internal uint InitializationDelay;
            /// <summary>
            /// Baud Rate to initially Open the Port and
            /// initialize the device.
            /// During initialization the Baud Rate will be changed
            /// to the actual Baud Rate specified as the BaudRate
            /// member of this structure.
            /// </summary>
            internal uint InitializationBaudRate;
            /// <summary>
            /// The name of the driver prefix to be
            /// opened.  For example specifying "COM" for this
            /// value and 8 for the COMPortNumber would result
            /// in the COM8: device being used as the HCI Driver
            /// Transport.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = StackConsts.HCI_COMM_DRIVER_NAME_MAX)]
            internal string DriverName;

            public HCI_DriverInformation__HCI_COMMDriverInformation(
                int COMPortNumber, int BaudRate,
                StackConsts.HCI_COMM_Protocol Protocol,
                int InitializationDelay, int InitializationBaudRate,
                string DriverName)
            {
                this.DriverType = StackConsts.HCI_DriverType.COMM;
                //
                this.COMPortNumber = checked((uint)COMPortNumber);
                this.BaudRate = checked((uint)BaudRate);
                this.Protocol = Protocol;
                this.InitializationDelay = checked((uint)InitializationDelay);
                this.InitializationBaudRate = checked((uint)InitializationBaudRate);
                this.DriverName = DriverName;
                //
                this.DriverInformationSize = 0;
                this.DriverInformationSize = checked((uint)Marshal.SizeOf(this) - SizeOfOuterStruct);
            }

            //--
            public override string ToString()
            {
                return "HCI_DriverInformation/COMM ("
                    + this.DriverType + ", " + this.DriverInformationSize + ")"
                    + ", '" + this.DriverName + "'" + this.COMPortNumber
                    + ", " + this.Protocol + ", " + this.BaudRate
                    + "; " + this.InitializationBaudRate
                    + ", " + this.InitializationDelay
                    + ".";
            }

            //--
            public static HCI_DriverInformation__HCI_COMMDriverInformation FromRegistry()
            {
                //these are known defaults
                int portNumber = 6;
                int baudRate = 57600;
                StackConsts.HCI_COMM_Protocol protocol = StackConsts.HCI_COMM_Protocol.UART_RTS_CTS;
                int initDelay = 500;
                int initBaud = 115200;
                string driverName = "BTS";

                //try to retrieve actual values from the registry
                Microsoft.Win32.RegistryKey rkStoneStreet = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Stonestreet One\\BTExplorer\\Device Settings", false);
                if (rkStoneStreet != null) {
                    object val;
                    val = rkStoneStreet.GetValue("PortNumber", 0);
                    portNumber = Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    val = rkStoneStreet.GetValue("PortBaudRate", 0);
                    baudRate = Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    val = rkStoneStreet.GetValue("PortProtocol", 1);
                    protocol = (StackConsts.HCI_COMM_Protocol)Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    val = rkStoneStreet.GetValue("InitializationPortBaudRate", "115200");
                    initBaud = Convert.ToInt32(val, CultureInfo.InvariantCulture);
                    driverName = rkStoneStreet.GetValue("PortPrefix", "BTS").ToString();
                    rkStoneStreet.Close();
                    Utils.MiscUtils.Trace_WriteLine("Bluetopia did get init values from the Registry.");
                }
                return new HCI_DriverInformation__HCI_COMMDriverInformation(portNumber, baudRate, protocol, initDelay, initBaud, driverName);
            }
        }

        internal struct HCI_Packet
        {
            internal readonly StackConsts.HCI_PacketType _HCIPacketType;
            internal readonly uint _HCIPacketLength;
            internal readonly byte _startOf_HCIPacketData;
        }
        #endregion

        #region HCIAPI.h
        internal struct HCI_Event_Data_t
        {
            internal readonly StackConsts.HCI_Event_Type_t Event_Data_Type;
            internal readonly ushort Event_Data_Size;
            internal readonly IntPtr pData;
            //union
            //{
            //   HCI_Inquiry_Complete_Event_Data_t                              *HCI_Inquiry_Complete_Event_Data;
            //   HCI_Inquiry_Result_Event_Data_t                                *HCI_Inquiry_Result_Event_Data;
            //   HCI_Connection_Complete_Event_Data_t                           *HCI_Connection_Complete_Event_Data;
            //   HCI_Connection_Request_Event_Data_t                            *HCI_Connection_Request_Event_Data;
            //   HCI_Disconnection_Complete_Event_Data_t                        *HCI_Disconnection_Complete_Event_Data;
            //   HCI_Authentication_Complete_Event_Data_t                       *HCI_Authentication_Complete_Event_Data;
            //   HCI_Remote_Name_Request_Complete_Event_Data_t                  *HCI_Remote_Name_Request_Complete_Event_Data;
            //   HCI_Encryption_Change_Event_Data_t                             *HCI_Encryption_Change_Event_Data;
            //   HCI_Change_Connection_Link_Key_Complete_Event_Data_t           *HCI_Change_Connection_Link_Key_Complete_Event_Data;
            //   HCI_Master_Link_Key_Complete_Event_Data_t                      *HCI_Master_Link_Key_Complete_Event_Data;
            //   HCI_Read_Remote_Supported_Features_Complete_Event_Data_t       *HCI_Read_Remote_Supported_Features_Complete_Event_Data;
            //   HCI_Read_Remote_Version_Information_Complete_Event_Data_t      *HCI_Read_Remote_Version_Information_Complete_Event_Data;
            //   HCI_QoS_Setup_Complete_Event_Data_t                            *HCI_QoS_Setup_Complete_Event_Data;
            //   HCI_Hardware_Error_Event_Data_t                                *HCI_Hardware_Error_Event_Data;
            //   HCI_Flush_Occurred_Event_Data_t                                *HCI_Flush_Occurred_Event_Data;
            //   HCI_Role_Change_Event_Data_t                                   *HCI_Role_Change_Event_Data;
            //   HCI_Number_Of_Completed_Packets_Event_Data_t                   *HCI_Number_Of_Completed_Packets_Event_Data;
            //   HCI_Mode_Change_Event_Data_t                                   *HCI_Mode_Change_Event_Data;
            //   HCI_Return_Link_Keys_Event_Data_t                              *HCI_Return_Link_Keys_Event_Data;
            //   HCI_PIN_Code_Request_Event_Data_t                              *HCI_PIN_Code_Request_Event_Data;
            //   HCI_Link_Key_Request_Event_Data_t                              *HCI_Link_Key_Request_Event_Data;
            //   HCI_Link_Key_Notification_Event_Data_t                         *HCI_Link_Key_Notification_Event_Data;
            //   HCI_Loopback_Command_Event_Data_t                              *HCI_Loopback_Command_Event_Data;
            //   HCI_Data_Buffer_Overflow_Event_Data_t                          *HCI_Data_Buffer_Overflow_Event_Data;
            //   HCI_Max_Slots_Change_Event_Data_t                              *HCI_Max_Slots_Change_Event_Data;
            //   HCI_Read_Clock_Offset_Complete_Event_Data_t                    *HCI_Read_Clock_Offset_Complete_Event_Data;
            //   HCI_Connection_Packet_Type_Changed_Event_Data_t                *HCI_Connection_Packet_Type_Changed_Event_Data;
            //   HCI_QoS_Violation_Event_Data_t                                 *HCI_QoS_Violation_Event_Data;
            //   HCI_Page_Scan_Repetition_Mode_Change_Event_Data_t              *HCI_Page_Scan_Repetition_Mode_Change_Event_Data;
            //   HCI_Page_Scan_Mode_Change_Event_Data_t                         *HCI_Page_Scan_Mode_Change_Event_Data;
            //   HCI_Flow_Specification_Complete_Event_Data_t                   *HCI_Flow_Specification_Complete_Event_Data;
            //   HCI_Inquiry_Result_With_RSSI_Event_Data_t                      *HCI_Inquiry_Result_With_RSSI_Event_Data;
            //   HCI_Read_Remote_Extended_Features_Complete_Event_Data_t        *HCI_Read_Remote_Extended_Features_Complete_Event_Data;
            //   HCI_Synchronous_Connection_Complete_Event_Data_t               *HCI_Synchronous_Connection_Complete_Event_Data;
            //   HCI_Synchronous_Connection_Changed_Event_Data_t                *HCI_Synchronous_Connection_Changed_Event_Data;
            //   HCI_Sniff_Subrating_Event_Data_t                               *HCI_Sniff_Subrating_Event_Data;
            //   HCI_Extended_Inquiry_Result_Event_Data_t                       *HCI_Extended_Inquiry_Result_Event_Data;
            //   HCI_Encryption_Key_Refresh_Complete_Event_Data_t               *HCI_Encryption_Key_Refresh_Complete_Event_Data;
            //   HCI_IO_Capability_Request_Event_Data_t                         *HCI_IO_Capability_Request_Event_Data;
            //   HCI_IO_Capability_Response_Event_Data_t                        *HCI_IO_Capability_Response_Event_Data;
            //   HCI_User_Confirmation_Request_Event_Data_t                     *HCI_User_Confirmation_Request_Event_Data;
            //   HCI_User_Passkey_Request_Event_Data_t                          *HCI_User_Passkey_Request_Event_Data;
            //   HCI_Remote_OOB_Data_Request_Event_Data_t                       *HCI_Remote_OOB_Data_Request_Event_Data;
            //   HCI_Simple_Pairing_Complete_Event_Data_t                       *HCI_Simple_Pairing_Complete_Event_Data;
            //   HCI_Link_Supervision_Timeout_Changed_Event_Data_t              *HCI_Link_Supervision_Timeout_Changed_Event_Data;
            //   HCI_Enhanced_Flush_Complete_Event_Data_t                       *HCI_Enhanced_Flush_Complete_Event_Data;
            //   HCI_User_Passkey_Notification_Event_Data_t                     *HCI_User_Passkey_Notification_Event_Data;
            //   HCI_Keypress_Notification_Event_Data_t                         *HCI_Keypress_Notification_Event_Data;
            //   HCI_Remote_Host_Supported_Features_Notification_Event_Data_t   *HCI_Remote_Host_Supported_Features_Notification_Event_Data;
            //   void                                                      *HCI_Unknown_Event_Data;
            //} Event_Data;
        }

        internal struct HCI_Connection_Complete_Event_Data_t
        {
            internal readonly StackConsts.HCI_ERROR_CODE Status;
            internal readonly ushort Connection_Handle;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] BD_ADDR;
            internal readonly StackConsts.HCI_LINK_TYPE Link_Type;
            internal readonly byte Encryption_Mode;
        }
        #endregion

        #region GAPAPI.h
        /* No pack */

        /// <summary>
        /// The following type declaration defines an Individual Inquiry
        /// Result Entry.  This information forms the Data Portion of the
        /// GAP_Inquiry_Event_Data_t structure.
        /// </summary>
        internal struct GAP_Inquiry_Data
        {
#pragma warning disable 169 // "warning CS0169: The field '...' is never used"
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] BD_ADDR;
            internal readonly byte Page_Scan_Repetition_Mode;
            internal readonly byte Page_Scan_Period_Mode;
            internal readonly byte Page_Scan_Mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.Class_of_Device_SIZE)]
            internal readonly byte[] /*Class_of_Device_t*/ Class_of_Device;
            internal readonly ushort Clock_Offset;
#pragma warning restore 169
        }

        //#define GAP_INQUIRY_DATA_SIZE                   (sizeof(GAP_Inquiry_Data_t))

        /* The following type declaration defines the result of an Inquiry   */
        /* Process that was started via the GAP_Perform_Inquiry() function.  */
        /* The Number of Devices Entry defines the number of Inquiry Data    */
        /* Entries that the GAP_Inquiry_Data member points to (if non-zero). */
        internal struct GAP_Inquiry_Event_Data
        {
            internal readonly ushort Number_Devices;
            internal readonly IntPtr/*"GAP_Inquiry_Data_t*"*/ GAP_Inquiry_Data;

            internal GAP_Inquiry_Event_Data(ushort numDevices)
            {
                Number_Devices = numDevices;
                GAP_Inquiry_Data = IntPtr.Zero;
            }
        }

        //#define GAP_INQUIRY_EVENT_DATA_SIZE             (sizeof(GAP_Inquiry_Event_Data_t))

        /* The following type declaration defines an individual result of an */
        /* Inquiry Process that was started via the GAP_Perform_Inquiry()    */
        /* function.  This event data is generated for each Inquiry Result as*/
        /* it is received.                                                   */
        internal struct GAP_Inquiry_Entry_Event_Data
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] BD_ADDR;
            internal readonly byte Page_Scan_Repetition_Mode;
            internal readonly byte Page_Scan_Period_Mode;
            internal readonly byte Page_Scan_Mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.Class_of_Device_SIZE)]
            internal readonly byte[] /*Class_of_Device_t*/ Class_of_Device;
            internal readonly ushort Clock_Offset;

            internal GAP_Inquiry_Entry_Event_Data(byte[] addr, byte[] cod)
            {
                BD_ADDR = addr;
                Class_of_Device = cod;
                Page_Scan_Mode = Page_Scan_Repetition_Mode = Page_Scan_Period_Mode = Page_Scan_Mode = 0;
                Clock_Offset = Clock_Offset = 0;
            }
        }

        //#define GAP_INQUIRY_ENTRY_EVENT_DATA_SIZE       (sizeof(GAP_Inquiry_Entry_Event_Data_t))

        /* The following type declaration defines GAP Encryption Status      */
        /* Information that is used with the GAP Encryption Change Result    */
        /* Event.                                                            */
        internal struct GAP_Encryption_Mode_Event_Data
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] Remote_Device;
            internal readonly byte Encryption_Change_Status;
            internal readonly StackConsts.GAP_Encryption_Mode Encryption_Mode;
        }

        //#define GAP_ENCRYPTION_MODE_EVENT_DATA_SIZE     (sizeof(GAP_Encryption_Mode_Event_Data_t))

        /* The following type declaration defines GAP Authentication         */
        /* Information that can be set and/or returned.  The first member    */
        /* of this structure specifies which Data Member should be used.     */
        /* * NOTE * For GAP Authentication Types that are rejections, the    */
        /*          Authentication_Data_Length member is set to zero and     */
        /*          All Data Members can be ignored (since non are valid).   */
        internal struct GAP_Authentication_Information
        {
            const int PinCodeLength = 16;
            //
            readonly StackConsts.GAP_Authentication_Type_t _GAP_Authentication_Type;
            internal readonly Byte _Authentication_Data_Length;
            //union
            //{
            //   PIN_Code_t PIN_Code;
            //   Link_Key_t Link_Key;
            //} Authentication_Data;
            // In the Bluetopia SDK for M3 the struct has support for SSP...
            // And the union has a DWORD in it which causes alignment!
            // Detect perhaps with HCI_Read_Simple_Pairing_Mode or GAP_IO_Capabilities_Request_Reply
            const int PadOnNonSspPlatforms = 0;
            const int PadOnSspPlatforms = 3;
            const int MaxPad = PadOnSspPlatforms;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = PinCodeLength + MaxPad)]
            readonly byte[] _pinCodeOrLinkKeyWithPad;

            internal GAP_Authentication_Information(
                    StackConsts.GAP_Authentication_Type_t type)
            {
                _GAP_Authentication_Type = type;
                _pinCodeOrLinkKeyWithPad = null;
                _Authentication_Data_Length = 0;
            }

            internal GAP_Authentication_Information(
                    StackConsts.GAP_Authentication_Type_t type,
                    byte[] content,
                    NativeMethods.ApiVersion apiVers)
                : this(type)
            {
                if (content.Length > PinCodeLength)
                    throw new ArgumentException("PIN too long (sixteen UTF-8 bytes max).");
                _pinCodeOrLinkKeyWithPad = new byte[PinCodeLength + MaxPad];
                int pad = PadOnSspPlatforms;
                if (apiVers == NativeMethods.ApiVersion.PreSsp) {
                    pad = PadOnNonSspPlatforms;
                }
                Array.Copy(content, 0, _pinCodeOrLinkKeyWithPad, pad, content.Length);
                _Authentication_Data_Length = checked((byte)content.Length);
            }

            //--
            internal static byte[] PinToByteArray(string pin)
            {
                var content = Encoding.UTF8.GetBytes(pin);
                if (content.Length > PinCodeLength) {
                    throw new ArgumentException("PIN too long (at " + content.Length + " UTF-8 bytes).");
                } else {
                    return content;
                }
            }

            //--
            public string DebugToString()
            {
                var str = "len: " + _Authentication_Data_Length
                    + " [" + BitConverter.ToString(_pinCodeOrLinkKeyWithPad) + "]withPad";
                return str;
            }
        }//struct

        //#define GAP_AUTHENTICATION_INFORMATION_SIZE     (sizeof(GAP_Authentication_Information_t))

        /* The following type declaration specifies the information that can */
        /* be returned in a GAP_Authentication_Callback.  This information   */
        /* is passed to the Callback when a GAP_Authentication Callback is   */
        /* issued.  The first member of this structure specifies which Data  */
        /* Member is valid.  Currently the following members are valid for   */
        /* the following values of the GAP_Authentication_Event_Type member: */
        /*                                                                   */
        /*    atLinkKeyRequest       - No Further Data.                      */
        /*    atPINCodeRequest       - No Further Data.                      */
        /*    atAuthenticationStatus - Authentication_Status is valid.       */
        /*    atLinkKeyCreation      - Link_Key is valid.                    */
        internal struct GAP_Authentication_Event_Data__Status
        {
            internal readonly StackConsts.GAP_Authentication_Event_Type _GAP_Authentication_Event_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] _Remote_Device;
            //union
            //{
            //   Byte_t     Authentication_Status;
            //   Link_Key_t Link_Key;
            //} Authentication_Event_Data;
            // In the Bluetopia SDK for M3 the struct has support for SSP...
            // And the union has a DWORD in it which causes alignment!
            const int PadOnNonSspPlatforms = 0;
            const int PadOnSspPlatforms = 2;
            const int MaxPad = PadOnSspPlatforms;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16 + MaxPad)]
            readonly byte[] _unionWithPadAuthStatusAndLinkKey;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dummy")]
            internal GAP_Authentication_Event_Data__Status(Version dummy)
            {
                _GAP_Authentication_Event_Type = 0;
                _Remote_Device = null;
                _unionWithPadAuthStatusAndLinkKey = null;
            }

            public StackConsts.HCI_ERROR_CODE GetAuthenticationStatus(NativeMethods.ApiVersion apiVersion)
            {
                int pad = PadOnSspPlatforms;
                if (apiVersion == NativeMethods.ApiVersion.PreSsp) {
                    pad = PadOnNonSspPlatforms;
                }
                byte b = _unionWithPadAuthStatusAndLinkKey[pad];
                var s = (StackConsts.HCI_ERROR_CODE)b;
                return s;
            }

        }

        internal struct GAP_Authentication_Event_Data__LinkKey
        {
            internal readonly StackConsts.GAP_Authentication_Event_Type _GAP_Authentication_Event_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] _Remote_Device;
            //union
            //{
            //   Byte_t     Authentication_Status;
            //   Link_Key_t Link_Key;
            //} Authentication_Event_Data;
            // In the Bluetopia SDK for M3 the struct has support for SSP...
            // And the union has a DWORD in it which causes alignment!
            //union
            //{
            //   Byte_t              Authentication_Status;
            //   GAP_Link_Key_Info_t Key_Info;
            //   DWord_t             Numeric_Value;
            //   Keypress_t          Keypress_Type;
            //} Authentication_Event_Data;     
            // with
            //struct _tagGAP_Link_Key_Info_t
            //{
            //   Link_Key_t  Link_Key;
            //   Byte_t      Key_Type;
            //} GAP_Link_Key_Info_t; 
            const int PadOnNonSspPlatforms = 0;
            const int PadOnSspPlatforms = 2;
            const int MaxPad = PadOnSspPlatforms;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPad + 16 + 1)]
            readonly byte[] _LinkKey_AndType_WithPad;

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "dummy")]
            private GAP_Authentication_Event_Data__LinkKey(Version dummy)
            {
                _GAP_Authentication_Event_Type = StackConsts.GAP_Authentication_Event_Type.LinkKeyCreation;
                _Remote_Device = null;
                _LinkKey_AndType_WithPad = null;
            }

            internal byte[] GetLinkKey(NativeMethods.ApiVersion apiVersion)
            {
                int pad = PadOnSspPlatforms;
                if (apiVersion == NativeMethods.ApiVersion.PreSsp) {
                    pad = PadOnNonSspPlatforms;
                }
                var buf = new byte[16];
                Array.Copy(_LinkKey_AndType_WithPad, pad, buf, 0, buf.Length);
                if (apiVersion >= NativeMethods.ApiVersion.Ssp) {
                    byte keyType0 = _LinkKey_AndType_WithPad[pad + 16];
                    var keyType = (HCI_LINK_KEY_TYPE)keyType0;
                }
                return buf;
            }

            /// <summary>
            /// The following Constants represent the defined Bluetooth HCI Link Key Types (Version 1.1).
            /// </summary>
            enum HCI_LINK_KEY_TYPE
            {
                COMBINATION_KEY = 0x00,
                LOCAL_UNIT_KEY = 0x01,
                REMOTE_UNIT_KEY = 0x02,
                DEBUG_COMBINATION_KEY = 0x03,
                UNAUTHENTICATED_COMBINATION_KEY = 0x04,
                AUTHENTICATED_COMBINATION_KEY = 0x05,
                CHANGED_COMBINATION_KEY = 0x06,
            }
        }

        //#define GAP_AUTHENTICATION_EVENT_DATA_SIZE      (sizeof(GAP_Authentication_Event_Data_t))

        /* The following structure represents the GAP Remote Name Response   */
        /* Event Data that is returned from the                              */
        /* GAP_Query_Remote_Device_Name() function.  The Remote_Name         */
        /* member will point to a NULL terminated string that represents     */
        /* the User Friendly Bluetooth Name of the Remote Device associated  */
        /* with the specified BD_ADDR.                                       */
        internal struct GAP_Remote_Name_Event_Data
        {
            internal readonly StackConsts.HCI_ERROR_CODE/*byte*/ _Remote_Name_Status;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] _Remote_Device;
            internal readonly IntPtr/*"char*"*/ _Remote_Name;
        }

        //#define GAP_REMOTE_NAME_EVENT_DATA_SIZE         (sizeof(GAP_Remote_Name_Event_Data_t))

        /* The following structure represents the container structure that   */
        /* holds all GAP Event Data Data.                                    */
        internal struct GAP_Event_Data
        {
            internal readonly StackConsts.GAP_Event_Type Event_Data_Type;
            internal readonly ushort Event_Data_Size;
            //union
            //{
            //   GAP_Inquiry_Event_Data         *GAP_Inquiry_Event_Data;
            //   GAP_Encryption_Mode_Event_Data *GAP_Encryption_Mode_Event_Data;
            //   GAP_Authentication_Event_Data  *GAP_Authentication_Event_Data;
            //   GAP_Remote_Name_Event_Data     *GAP_Remote_Name_Event_Data;
            //   GAP_Inquiry_Entry_Event_Data   *GAP_Inquiry_Entry_Event_Data;
            //} Event_Data;
            internal readonly IntPtr pData;

            internal GAP_Event_Data(StackConsts.GAP_Event_Type type, IntPtr pData)
            {
                this.Event_Data_Type = type;
                this.pData = pData;
                this.Event_Data_Size = 0;
            }
        };

        //#define GAP_EVENT_DATA_SIZE                     (sizeof(GAP_Event_Data_t))

        #endregion

        #region SPPAPI.h
        /* No pack */

        internal struct SPP_Open_Port_Indication_Data
        {
            internal readonly uint SerialPortID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = StackConsts.BD_ADDR_SIZE)]
            internal readonly byte[] BD_ADDR;
        }

        internal struct SPP_Open_Port_Confirmation_Data
        {
            internal readonly uint SerialPortID;
            internal readonly StackConsts.SPP_OPEN_PORT_STATUS PortOpenStatus;

            internal SPP_Open_Port_Confirmation_Data(
                uint SerialPortID, StackConsts.SPP_OPEN_PORT_STATUS PortOpenStatus)
            {
                this.SerialPortID = SerialPortID;
                this.PortOpenStatus = PortOpenStatus;
            }
        }

        internal struct SPP_Close_Port_Indication_Data
        {
            internal readonly uint SerialPortID;

            internal SPP_Close_Port_Indication_Data(
                uint SerialPortID)
            {
                this.SerialPortID = SerialPortID;
            }
        }

        // SPP_Port_Status_Indication_Data_t

        internal struct SPP_Data_Indication_Data
        {
            internal readonly uint SerialPortID;
            internal readonly ushort DataLength;

            internal SPP_Data_Indication_Data(
                uint SerialPortID, ushort DataLength)
            {
                this.SerialPortID = SerialPortID;
                this.DataLength = DataLength;
            }
        }

        internal struct SPP_Transmit_Buffer_Empty_Indication_Data
        {
            internal readonly uint SerialPortID;

            internal SPP_Transmit_Buffer_Empty_Indication_Data(
                uint SerialPortID)
            {
                this.SerialPortID = SerialPortID;
            }
        }

        // SPP_Line_Status_Indication_Data_t
        // SPP_Send_Port_Information_Confirmation_Data_t
        // SPP_Query_Port_Information_Indication_Data_t
        // SPP_Query_Port_Information_Confirmation_Data_t
        // SPP_Open_Port_Request_Indication_Data_t

        internal struct SPP_Event_Data
        {
            internal readonly StackConsts.SPP_Event_Type Event_Data_Type;
            internal readonly ushort Event_Data_Size;
            //union
            //{
            //   SPP_Open_Port_Indication_Data_t                *SPP_Open_Port_Indication_Data;
            //   SPP_Open_Port_Confirmation_Data_t              *SPP_Open_Port_Confirmation_Data;
            //   SPP_Close_Port_Indication_Data_t               *SPP_Close_Port_Indication_Data;
            //   SPP_Port_Status_Indication_Data_t              *SPP_Port_Status_Indication_Data;
            //   SPP_Data_Indication_Data_t                     *SPP_Data_Indication_Data;
            //   SPP_Transmit_Buffer_Empty_Indication_Data_t    *SPP_Transmit_Buffer_Empty_Indication_Data;
            //   SPP_Line_Status_Indication_Data_t              *SPP_Line_Status_Indication_Data;
            //   SPP_Send_Port_Information_Indication_Data_t    *SPP_Send_Port_Information_Indication_Data;
            //   SPP_Send_Port_Information_Confirmation_Data_t  *SPP_Send_Port_Information_Confirmation_Data;
            //   SPP_Query_Port_Information_Indication_Data_t   *SPP_Query_Port_Information_Indication_Data;
            //   SPP_Query_Port_Information_Confirmation_Data_t *SPP_Query_Port_Information_Confirmation_Data;
            //   SPP_Open_Port_Request_Indication_Data_t        *SPP_Open_Port_Request_Indication_Data;
            //} Event_Data;
            internal readonly IntPtr pEventData;

            internal SPP_Event_Data(StackConsts.SPP_Event_Type Event_Data_Type,
                IntPtr pEventData)
            {
                this.Event_Data_Type = Event_Data_Type;
                this.pEventData = pEventData;
                Event_Data_Size = Event_Data_Size = 0;
            }
        }

        internal struct SPP_SDP_Service_Record : IDisposable
        {
            internal readonly uint _NumberServiceClassUUID;
            // NETCF P/Invoke doesn't support marshalling an array of structs
            // so we have to marshal it ourselves.
            // (We got NotSupportedException and Interop log told us why).
            internal readonly IntPtr _SDPUUIDEntries;
            /// <summary>
            /// Any Protocol Information that is specified (if any) will be
            /// added in the Protocol Attribute after the default SPP Protocol
            /// List (L2CAP and RFCOMM).
            /// </summary>
            readonly IntPtr fake_ProtocolList;

            //----
            internal SPP_SDP_Service_Record(SDP_UUID_Entry[] SDPUUIDEntries)
            {
                _NumberServiceClassUUID = checked((uint)SDPUUIDEntries.Length);
                // NETCF P/Invoke doesn't support marshalling an array of structs
                // so we have to marshal it ourselves.
                var so = Marshal.SizeOf(typeof(SDP_UUID_Entry));
                var pUuids = Marshal.AllocHGlobal(so * SDPUUIDEntries.Length);
                // TODO SPP_SDP_Service_Record: Delete memory if failure in initialisation...
                var pDst = pUuids;
                for (int i = 0; i < SDPUUIDEntries.Length; ++i) {
                    Marshal.StructureToPtr(SDPUUIDEntries[i], pDst, false);
                    pDst = Pointers.Add(pDst, so);
                }
#if DEBUG
                var pEnd = Pointers.Add(pUuids, (so * SDPUUIDEntries.Length));
                Debug.Assert(pDst == pEnd);
#endif
                _SDPUUIDEntries = pUuids;
                fake_ProtocolList = IntPtr.Zero;
            }

            //--
            public void Dispose()
            {
                if (_SDPUUIDEntries != IntPtr.Zero) {
                    Marshal.FreeHGlobal(_SDPUUIDEntries);
                }
            }
        }
        #endregion

        #region SDPAPI.H
        /* No pack */

        /// <summary>
        /// The following data type represents a special SDP Data Type.  This
        /// structure ONLY holds the UUID Data Types.  This structure is
        /// provided so that API calls that only deal with UUID's can use
        /// this data type instead of the more generic SDP_Data_Element_Type_t
        /// Data Type.  This will aid in code readability and also aid in
        /// making the code that only processes UUID's more simple.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal struct SDP_UUID_Entry
        {
            internal readonly StackConsts.SDP_Data_Element_Type _SDP_Data_Element_Type;
            //union
            //{
            //   UUID_16_t  UUID_16;
            //   UUID_32_t  UUID_32;
            //   UUID_128_t UUID_128;
            //} UUID_Value;
            readonly Guid _uuid128;

            public SDP_UUID_Entry(Guid uuid128)
            {
                Guid networkOrder = Sockets.BluetoothListener.HostToNetworkOrder(uuid128);
                this._uuid128 = networkOrder;
                this._SDP_Data_Element_Type = StackConsts.SDP_Data_Element_Type.UUID_128;
            }
        };

        internal struct SDP_UUID_Entry16
        {
            internal readonly StackConsts.SDP_Data_Element_Type _SDP_Data_Element_Type;
            readonly byte[] _uuid16;

            public SDP_UUID_Entry16(UInt16 uuid16)
            {
                var net = System.Net.IPAddress.HostToNetworkOrder(
                    unchecked((Int16)uuid16));
                _uuid16 = BitConverter.GetBytes(net);
                this._SDP_Data_Element_Type = StackConsts.SDP_Data_Element_Type.UUID_16;
            }
        };

        //#define SDP_UUID_ENTRY_SIZE                             (sizeof(SDP_UUID_Entry_t))

        internal struct SDP_UUID_Entry_Bytes
        {
            internal readonly StackConsts.SDP_Data_Element_Type _SDP_Data_Element_Type;
            //union
            //{
            //   UUID_16_t  UUID_16;
            //   UUID_32_t  UUID_32;
            //   UUID_128_t UUID_128;
            //} UUID_Value;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            readonly byte[] _array;

            //----
            private SDP_UUID_Entry_Bytes(StackConsts.SDP_Data_Element_Type type)
            {
                _SDP_Data_Element_Type = type;
                _array = new byte[16];
            }

            public SDP_UUID_Entry_Bytes(Guid uuid128)
                : this(StackConsts.SDP_Data_Element_Type.UUID_128)
            {
                Guid networkOrder = Sockets.BluetoothListener.HostToNetworkOrder(uuid128);
                var arr = networkOrder.ToByteArray();
                arr.CopyTo(_array, 0);
            }

            public SDP_UUID_Entry_Bytes(UInt32 uuid32)
                : this(StackConsts.SDP_Data_Element_Type.UUID_32)
            {
                var net = System.Net.IPAddress.HostToNetworkOrder(
                    unchecked((Int32)uuid32));
                var arr = BitConverter.GetBytes(net);
                arr.CopyTo(_array, 0);
            }

            public SDP_UUID_Entry_Bytes(UInt16 uuid16)
                : this(StackConsts.SDP_Data_Element_Type.UUID_16)
            {
                var net = System.Net.IPAddress.HostToNetworkOrder(
                    unchecked((Int16)uuid16));
                var arr = BitConverter.GetBytes(net);
                arr.CopyTo(_array, 0);
            }

            //----
            internal static SDP_UUID_Entry_Bytes Create(ServiceElement serviceElement)
            {
                switch (serviceElement.ElementType) {
                    case ElementType.Uuid128:
                        return new SDP_UUID_Entry_Bytes((Guid)serviceElement.Value);
                    case ElementType.Uuid32:
                        var u32 = checked((UInt32)serviceElement.Value);
                        return new SDP_UUID_Entry_Bytes(u32);
                    case ElementType.Uuid16:
                        var u16 = checked((UInt16)serviceElement.Value);
                        return new SDP_UUID_Entry_Bytes(u16);
                    default:
                        throw new ArgumentException("Not a UUID type: " + serviceElement.ElementType + ".");
                }
            }

        }

#if false
        [StructLayout(LayoutKind.Sequential)]
        internal class SDP_UUID_Entry__Class
        {
            internal readonly StackConsts.SDP_Data_Element_Type _SDP_Data_Element_Type;
            //union
            //{
            //   UUID_16_t  UUID_16;
            //   UUID_32_t  UUID_32;
            //   UUID_128_t UUID_128;
            //} UUID_Value;

            public SDP_UUID_Entry__Class(StackConsts.SDP_Data_Element_Type type)
            {
                this._SDP_Data_Element_Type = StackConsts.SDP_Data_Element_Type.UUID_128;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SDP_UUID_Entry__Class128 : SDP_UUID_Entry__Class
        {
            readonly Guid _uuid128;

            public SDP_UUID_Entry__Class128(Guid uuid128)
                : base(StackConsts.SDP_Data_Element_Type.UUID_128)
            {
                Guid networkOrder = Sockets.BluetoothListener.HostToNetworkOrder(uuid128);
                this._uuid128 = networkOrder;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SDP_UUID_Entry__Class16 : SDP_UUID_Entry__Class
        {
            readonly UInt16 _uuid16;

            public SDP_UUID_Entry__Class16(UInt16 uuid16)
                : base(StackConsts.SDP_Data_Element_Type.UUID_16)
            {
                Int16 networkOrder = IPAddress.HostToNetworkOrder(
                    unchecked((Int16)uuid16));
                this._uuid16 = unchecked((UInt16)networkOrder);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SDP_UUID_Entry__Class32 : SDP_UUID_Entry__Class
        {
            readonly UInt32 _uuid32;

            public SDP_UUID_Entry__Class32(UInt32 uuid32)
                : base(StackConsts.SDP_Data_Element_Type.UUID_32)
            {
                Int32 networkOrder = IPAddress.HostToNetworkOrder(
                    unchecked((Int32)uuid32));
                this._uuid32 = unchecked((UInt32)networkOrder);
            }
        }
#endif

        /// <summary>
        /// The following data type represents a special SDP Data Type.  This
        /// structure ONLY holds the Attribute ID Information.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This
        /// structure is provided so that API calls that only deal with
        /// Attribute ID Lists can use this data type instead of the more
        /// generic SDP_Data_Element_Type_t Data Type.  This will aid in code
        /// readability and also aid in making the code that only processes
        /// Attribute ID Lists more simple.
        /// </para>
        /// <para>
        /// The BOOLEAN Flag specifies whether or not the Attribute ID List
        /// specifies a Range or a single Attribute ID.  If this flag is
        /// TRUE, then the End_Attribute_ID is valid and is used.  The
        /// End_Attribute_ID value MUST be greater than the Start_Attribute_ID
        /// value of the entry is considered invalid (if the Attribute_Range
        /// flag is TRUE).  If the Attribute_Range member is FALSE, then the
        /// Start_Attribute_ID member is the only member that is used.
        /// </para>
        /// </remarks>
        internal struct SDP_Attribute_ID_List_Entry
        {
            [MarshalAs(UnmanagedType.U1)]
            readonly Boolean Attribute_Range;
            readonly ushort Start_Attribute_ID;
            readonly ushort End_Attribute_ID;

            internal SDP_Attribute_ID_List_Entry(bool isRange, ushort lower, ushort upper)
            {
                this.Attribute_Range = isRange;
                this.Start_Attribute_ID = lower;
                this.End_Attribute_ID = upper;
                Debug.Assert(6 == Marshal.SizeOf(this), "No pack(1) on SDPAPI.h, so unexpected SizeOf(SDP_Attribute_ID_List_Entry_t): " + Marshal.SizeOf(this));
            }

            internal static SDP_Attribute_ID_List_Entry CreateRange(ushort lower, ushort upper)
            {
                return new SDP_Attribute_ID_List_Entry(true, lower, upper);
            }

            internal static SDP_Attribute_ID_List_Entry CreateItem(ServiceAttributeId serviceAttributeId)
            {
                return new SDP_Attribute_ID_List_Entry(false, (ushort)serviceAttributeId, 0);
            }
        }

        //#define SDP_ATTRIBUTE_ID_LIST_ENTRY_SIZE                (sizeof(SDP_Attribute_ID_List_Entry_t))

        /// <summary>
        /// The following Data Structure represents a structure that will hold
        /// an individual SDP Data Element.  The SDP_Data_Element_Type field
        /// holds the SDP Data Element Type, the Length field holds the number
        /// of Bytes that the Actual Data Element Value occupies (this value
        /// represents the buffer size that the pointer member of the union
        /// points to), and finally, the union defines the actual Data Type
        /// value.  It should be noted that the Text Field and the URL
        /// members are pointers to data because the data itself is variable
        /// in length.
        /// * NOTE * The following structure also supports the SDP Data Types
        ///          of Sequences and Alternatives.  This is treated as any
        ///          of the other Data Element Types, the
        ///          SDP_Data_Element_Length field denotes the Number of the
        ///          Data Elements that the SDP_Data_Element_Sequence OR the
        ///          SDP_Data_Element_Alternative Member points to.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal struct SDP_Data_Element
        {
            internal readonly StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type;
            internal readonly uint SDP_Data_Element_Length;
            internal byte FakeAtUnionPosition;
            internal byte FakeAtUnionPosition1;
            internal byte FakeAtUnionPosition2;
            internal byte FakeAtUnionPosition3;
            internal byte FakeAtUnionPosition4;
            internal byte FakeAtUnionPosition5;
            internal byte FakeAtUnionPosition6;
            internal byte FakeAtUnionPosition7;
            internal byte FakeAtUnionPosition8;
            internal byte FakeAtUnionPosition9;
            internal byte FakeAtUnionPositionA;
            internal byte FakeAtUnionPositionB;
            internal byte FakeAtUnionPositionC;
            internal byte FakeAtUnionPositionD;
            internal byte FakeAtUnionPositionE;
            internal byte FakeAtUnionPositionF;
            //TODO union
            //{
            //   Byte_t                         UnsignedInteger1Byte;
            //   Word_t                         UnsignedInteger2Bytes;
            //   DWord_t                        UnsignedInteger4Bytes;
            //   Byte_t                         UnsignedInteger8Bytes[8];
            //   Byte_t                         UnsignedInteger16Bytes[16];
            //   SByte_t                        SignedInteger1Byte;
            //   SWord_t                        SignedInteger2Bytes;
            //   SDWord_t                       SignedInteger4Bytes;
            //   Byte_t                         SignedInteger8Bytes[8];
            //   Byte_t                         SignedInteger16Bytes[16];
            //   Byte_t                         Boolean;
            //   UUID_16_t                      UUID_16;
            //   UUID_32_t                      UUID_32;
            //   UUID_128_t                     UUID_128;
            //   Byte_t                        *TextString;
            //   Byte_t                        *URL;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Sequence;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Alternative;
            //} SDP_Data_Element;

            /* For unit-test. */
            internal SDP_Data_Element(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length)
            {
                this.SDP_Data_Element_Type = SDP_Data_Element_Type;
                this.SDP_Data_Element_Length = SDP_Data_Element_Length;
                this.FakeAtUnionPosition = 0;
                this.FakeAtUnionPosition1 = this.FakeAtUnionPosition2 = this.FakeAtUnionPosition3 = 0;
                this.FakeAtUnionPosition4 = this.FakeAtUnionPosition5 = this.FakeAtUnionPosition6 = this.FakeAtUnionPosition7 = 0;
                this.FakeAtUnionPosition8 = this.FakeAtUnionPosition9 = this.FakeAtUnionPositionA = this.FakeAtUnionPositionB = 0;
                this.FakeAtUnionPositionC = this.FakeAtUnionPositionD = this.FakeAtUnionPositionE = this.FakeAtUnionPositionF = 0;
            }
        }

        internal struct SDP_Data_Element__Struct : IEquatable<SDP_Data_Element__Struct>
        {
            const int UnionSize = 16;
            //
            internal readonly StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type;
            internal readonly uint SDP_Data_Element_Length;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = UnionSize)]
            internal readonly byte[] _content;
            //union
            //{
            //   Byte_t                         UnsignedInteger1Byte;
            //   Word_t                         UnsignedInteger2Bytes;
            //   DWord_t                        UnsignedInteger4Bytes;
            //   Byte_t                         UnsignedInteger8Bytes[8];
            //   Byte_t                         UnsignedInteger16Bytes[16];
            //   SByte_t                        SignedInteger1Byte;
            //   SWord_t                        SignedInteger2Bytes;
            //   SDWord_t                       SignedInteger4Bytes;
            //   Byte_t                         SignedInteger8Bytes[8];
            //   Byte_t                         SignedInteger16Bytes[16];
            //   Byte_t                         Boolean;
            //   UUID_16_t                      UUID_16;
            //   UUID_32_t                      UUID_32;
            //   UUID_128_t                     UUID_128;
            //   Byte_t                        *TextString;
            //   Byte_t                        *URL;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Sequence;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Alternative;
            //} SDP_Data_Element;

            /* For unit-test. */
            internal SDP_Data_Element__Struct(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint length, byte[] content)
            {
                this.SDP_Data_Element_Type = SDP_Data_Element_Type;
                this.SDP_Data_Element_Length = checked((uint)content.Length);
                this.SDP_Data_Element_Length = length;
                this._content = new byte[UnionSize];
                content.CopyTo(_content, 0);
            }

            public override bool Equals(object other)
            {
                // (Can't use 'as' with struct)
                if (!(other is SDP_Data_Element__Struct))
                    return false;
                return Equals((SDP_Data_Element__Struct)other);
            }

            public bool Equals(SDP_Data_Element__Struct other)
            {
                if (this.SDP_Data_Element_Type != other.SDP_Data_Element_Type)
                    return false;
                if (this.SDP_Data_Element_Length != other.SDP_Data_Element_Length)
                    return false;
                if ((this._content == null) && (other._content == null)) {
                    return true;
                }
                if ((this._content == null) || (other._content == null)) {
                    return false;
                }
                for (int i = 0; i < this.SDP_Data_Element_Length; ++i) {
                    if (this._content[i] != other._content[i])
                        return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                if (_content != null) {
                    return _content.GetHashCode();
                } else {
                    // Only when no constructor called -- default struct creation
                    Debug.Assert(this.SDP_Data_Element_Length == 0);
                    Debug.Assert(this.SDP_Data_Element_Type == 0);
                    return -1;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal abstract class SDP_Data_Element__Class
        {
            const int UnionSize = 16;
            //
            internal readonly StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type;
            internal readonly uint SDP_Data_Element_Length;

            //----
            protected SDP_Data_Element__Class(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length)
            {
                this.SDP_Data_Element_Type = SDP_Data_Element_Type;
                this.SDP_Data_Element_Length = SDP_Data_Element_Length;
#if DEBUG && !NETCF
                var sizeOf = Marshal.SizeOf(this);
                Debug.Assert(sizeOf == ExpectedSize, "NOT sizeOf == ExpectedSize, is: " + sizeOf + " and " + ExpectedSize);
#endif
            }

            protected SDP_Data_Element__Class(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                int SDP_Data_Element_Length)
                : this(SDP_Data_Element_Type, (uint)SDP_Data_Element_Length)
            {
            }

            //----
            protected virtual int ExpectedSize { get { return 2 * 4; } }

        }

        [StructLayout(LayoutKind.Sequential)]
        internal abstract class SDP_Data_Element__Class_ByteArray : SDP_Data_Element__Class,
            IEquatable<SDP_Data_Element__Class_ByteArray>
        {
            protected abstract byte[] ElementValue { get; }
            //{
            //   Byte_t                         UnsignedInteger1Byte;
            //   Word_t                         UnsignedInteger2Bytes;
            //   DWord_t                        UnsignedInteger4Bytes;
            //   Byte_t                         UnsignedInteger8Bytes[8];
            //   Byte_t                         UnsignedInteger16Bytes[16];
            //   SByte_t                        SignedInteger1Byte;
            //   SWord_t                        SignedInteger2Bytes;
            //   SDWord_t                       SignedInteger4Bytes;
            //   Byte_t                         SignedInteger8Bytes[8];
            //   Byte_t                         SignedInteger16Bytes[16];
            //   Byte_t                         Boolean;
            //   UUID_16_t                      UUID_16;
            //   UUID_32_t                      UUID_32;
            //   UUID_128_t                     UUID_128;
            //   Byte_t                        *TextString;
            //   Byte_t                        *URL;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Sequence;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Alternative;
            //} SDP_Data_Element;

            internal SDP_Data_Element__Class_ByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length)
                : base(SDP_Data_Element_Type, SDP_Data_Element_Length)
            {
            }

            internal SDP_Data_Element__Class_ByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                int SDP_Data_Element_Length)
                : this(SDP_Data_Element_Type, (uint)SDP_Data_Element_Length)
            {
            }

            //----
            //protected virtual override int ExpectedSize { get; }

            //----
            public override bool Equals(object obj)
            {
                return Equals(obj as SDP_Data_Element__Class_ByteArray);
            }

            public override int GetHashCode() // ShutUpCompiler
            {
                if (ElementValue == null)
                    return -1;
                return ElementValue.GetHashCode();
            }

            #region IEquatable<SDP_Data_Element__Class_Array> Members
            public virtual bool Equals(SDP_Data_Element__Class_ByteArray other)
            {
                if (other == null) {
                    return false;
                }
                if (this.SDP_Data_Element_Type != other.SDP_Data_Element_Type) {
                    Debug.Fail("Different SDP_Data_Element_Type");
                    return false;
                }
                if (this.SDP_Data_Element_Length != other.SDP_Data_Element_Length) {
                    Debug.Fail("Different SDP_Data_Element_Length");
                    return false;
                }
                if ((this.ElementValue == null) && (other.ElementValue == null)) {
                    return true; // Both are null.
                }
                if ((this.ElementValue == null) || (other.ElementValue == null)) {
                    Debug.Fail("One array is null");
                    return false; // One alone is null.
                }
                if (this.ElementValue.Length != other.ElementValue.Length) {
                    Debug.Fail("Arrays are different length.");
                    return false;
                }
                for (int i = 0; i < this.ElementValue.Length; ++i) {
                    if (this.ElementValue[i] != other.ElementValue[i]) {
                        Debug.Fail("Array content different at index: " + i
                            + ", value: 0x" + this.ElementValue[i].ToString("X2")
                            + " <> 0x" + other.ElementValue[i].ToString("X2") + ".");
                        return false;
                    }
                }
                //
                return true;
            }
            #endregion
        }

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SDP_Data_Element__Class_NonInlineByteArray : SDP_Data_Element__Class_ByteArray
        {
            internal byte[] _elementValue;

            //----
            internal SDP_Data_Element__Class_NonInlineByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length,
                byte[] elementValue)
                : base(SDP_Data_Element_Type, SDP_Data_Element_Length)
            {
                Debug.Assert(SDP_Data_Element_Length == elementValue.Length, "param len!=arrLen");
                this._elementValue = elementValue;
            }

            internal SDP_Data_Element__Class_NonInlineByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                int SDP_Data_Element_Length,
                byte[] elementValue)
                : this(SDP_Data_Element_Type, (uint)SDP_Data_Element_Length, elementValue)
            {
            }

            //----
            protected override int ExpectedSize
            {
                get
                {
                    return base.ExpectedSize
                        + IntPtr.Size;
                }
            }

            protected override byte[] ElementValue { get { return _elementValue; } }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SDP_Data_Element__Class_InlineByteArray : SDP_Data_Element__Class_ByteArray
        {
            const int UnionSize = 16;
            //
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = UnionSize)]
            internal byte[] _elementValue;

            //----
            internal SDP_Data_Element__Class_InlineByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length,
                byte[] elementValue)
                : base(SDP_Data_Element_Type, SDP_Data_Element_Length)
            {
                Debug.Assert(SDP_Data_Element_Length == elementValue.Length, "param len!=arrLen");
                _elementValue = new byte[UnionSize];
                elementValue.CopyTo(this._elementValue, 0);
            }

            internal SDP_Data_Element__Class_InlineByteArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                int SDP_Data_Element_Length,
                byte[] elementValue)
                : this(SDP_Data_Element_Type, (uint)SDP_Data_Element_Length, elementValue)
            {
            }

            //----
            protected override int ExpectedSize
            {
                get
                {
                    return base.ExpectedSize
                        + UnionSize;
                }
            }

            protected override byte[] ElementValue { get { return _elementValue; } }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SDP_Data_Element__Class_ElementArray : SDP_Data_Element__Class,
            IEquatable<SDP_Data_Element__Class_ElementArray>
        {
            internal SDP_Data_Element__Class[] elementArray;
            //{
            //   Byte_t                         UnsignedInteger1Byte;
            //   Word_t                         UnsignedInteger2Bytes;
            //   DWord_t                        UnsignedInteger4Bytes;
            //   Byte_t                         UnsignedInteger8Bytes[8];
            //   Byte_t                         UnsignedInteger16Bytes[16];
            //   SByte_t                        SignedInteger1Byte;
            //   SWord_t                        SignedInteger2Bytes;
            //   SDWord_t                       SignedInteger4Bytes;
            //   Byte_t                         SignedInteger8Bytes[8];
            //   Byte_t                         SignedInteger16Bytes[16];
            //   Byte_t                         Boolean;
            //   UUID_16_t                      UUID_16;
            //   UUID_32_t                      UUID_32;
            //   UUID_128_t                     UUID_128;
            //   Byte_t                        *TextString;
            //   Byte_t                        *URL;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Sequence;
            //   struct _tagSDP_Data_Element_t *SDP_Data_Element_Alternative;
            //} SDP_Data_Element;

            internal SDP_Data_Element__Class_ElementArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                uint SDP_Data_Element_Length,
                params SDP_Data_Element__Class[] elementArray)
                : base(SDP_Data_Element_Type, SDP_Data_Element_Length)
            {
                Debug.Assert(SDP_Data_Element_Length == elementArray.Length, "param len!=arrLen");
                this.elementArray = elementArray;
            }

            internal SDP_Data_Element__Class_ElementArray(StackConsts.SDP_Data_Element_Type SDP_Data_Element_Type,
                int SDP_Data_Element_Length,
                params SDP_Data_Element__Class[] elementArray)
                : this(SDP_Data_Element_Type, (uint)SDP_Data_Element_Length, elementArray)
            {
            }

            //----
            protected override int ExpectedSize
            {
                get
                {
                    return base.ExpectedSize
                        + IntPtr.Size;
                }
            }

            //----
            public override bool Equals(object obj)
            {
                return Equals(obj as SDP_Data_Element__Class_ElementArray);
            }

            public override int GetHashCode() // ShutUpCompiler
            {
                if (elementArray == null)
                    return -1;
                return elementArray.GetHashCode();
            }

            #region IEquatable<SDP_Data_Element__Class_Array> Members
            public bool Equals(SDP_Data_Element__Class_ElementArray other)
            {
                if (this.SDP_Data_Element_Type != other.SDP_Data_Element_Type) {
                    Debug.Fail("Different ElementArray SDP_Data_Element_Type");
                    return false;
                }
                if (this.SDP_Data_Element_Length != other.SDP_Data_Element_Length) {
                    Debug.Fail("Different ElementArray SDP_Data_Element_Length");
                    return false;
                }
                if ((this.elementArray == null) && (other.elementArray == null)) {
                    return true; // Both are null.
                }
                if ((this.elementArray == null) || (other.elementArray == null)) {
                    Debug.Fail("One ElementArray is null");
                    return false; // One alone is null.
                }
                if (this.elementArray.Length != other.elementArray.Length) {
                    Debug.Fail("ElementArray arrays are different length.");
                    return false;
                }
                for (int i = 0; i < this.elementArray.Length; ++i) {
                    var eqE = this.elementArray[i].Equals(other.elementArray[i]);
#if DEBUG_DEBUG_DEBUG
                    var eqSE = Object.Equals(this.elementArray[i], other.elementArray[i]);
                    var cur = this.elementArray[i];
                    var curT = cur.GetType();
                    //var mm = cur.GetType().GetMethod("Equals", 0, null,
                    //    new Type[] { typeof(object) }, null);
                    var eq0 = curT.InvokeMember("Equals", System.Reflection.BindingFlags.InvokeMethod, null,
                        cur,
                        new object[] { other.elementArray[i] });
                    var eq = (bool)eq0;
                    Debug.Assert(eq == eqSE, "NOT eq == eqSE");
                    Debug.Assert(eq == eqE, "(INFO NOT eq == eqE)");
                    if (!eq) {
                        Debug.Fail("ElementArray array content different at index: " + i);
                        return false;
                    }
#endif
                    if (!eqE) {
                        Debug.Fail("Array content different at ElementArray index: " + i);
                        return false;
                    }
                }
                //
                return true;
            }

            #endregion
        }

        //#define SDP_DATA_ELEMENT_SIZE                           (sizeof(SDP_Data_Element_t))

        /// <summary>
        /// The following Data Structure represents the SDP Error Response          
        /// Information that is returned by a Remote SDP Server when an
        /// invalid request has been received.  The Error_Info field is an
        /// optional field that may or may be present depending upon the
        /// Error Code value.  If there is NO Error Information, then the
        /// Error_Info_Length member will be set to zero and the Error_Info
        /// member pointer will be NULL.  If there is Error Information, then
        /// the Error_Info member will be a non-NULL pointer to the Error
        /// Information, and the Error_Info_Length member will contain the
        /// Length of the Data (in Bytes) that the Error_Info Pointer points
        /// to.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        struct SDP_Error_Response_Data
        {
            internal readonly ushort Error_Code;
            internal readonly ushort Error_Info_Length;
            internal readonly byte[] Error_Info;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ShutUpCompiler")]
            private SDP_Error_Response_Data(Version ShutUpCompiler)
            {
                Error_Code = 0;
                Error_Info_Length = 0;
                Error_Info = null;
            }
        }

        //#define SDP_ERROR_RESPONSE_DATA_SIZE                    (sizeof(SDP_Error_Response_Data_t))

        ///// <summary>
        ///// The following Data Structure represents the SDP Service Search
        ///// Response Data that is returned by the Remote SDP Server when a
        ///// SDP Service Search Request is submitted.  If there are Service
        ///// Records then the Total_Service_Record_Count member will contain
        ///// the Number Service Record Handles that the Service_Record_List
        ///// member points to.  If there are NO Service Record Handles, then
        ///// the Total_Service_Record_Count member will be zero, and the
        ///// Service_Record_List member will set to NULL.
        ///// -
        ///// <remarks>
        ///// <para>
        ///// </para>
        ///// </remarks>
        //struct SDP_Service_Search_Response_Data_t
        //{
        //    internal readonly Word_t Total_Service_Record_Count;
        //    //TO-DO internal readonly DWord_t* Service_Record_List;
        //}

        //#define SDP_SERVICE_SEARCH_RESPONSE_DATA_SIZE           (sizeof(SDP_Service_Search_Response_Data_t))

        /// <summary>
        /// The following Data Structure represents a Single SDP Service
        /// Attribute.  This Attribute consists of an Attribute ID and the
        /// Attribute Data which is a SDP Data Element (Note the SDP Data
        /// Element could contain a list of SDP Data Elements).
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal struct SDP_Service_Attribute_Value_Data
        {
            internal readonly ushort Attribute_ID;
            internal readonly IntPtr/*"SDP_Data_Element_t*"*/ pSDP_Data_Element;

            /* For unit-test. */
            internal SDP_Service_Attribute_Value_Data(ushort Attribute_ID,
                IntPtr/*"SDP_Data_Element_t*"*/ pSDP_Data_Element)
            {
                this.Attribute_ID = Attribute_ID;
                this.pSDP_Data_Element = pSDP_Data_Element;
            }
        }

        //#define SDP_SERVICE_ATTRIBUTE_VALUE_DATA_SIZE           (sizeof(SDP_Service_Attribute_Value_Data_t))

        /// <summary>
        /// The following Data Structure represents an Attribute List that
        /// the Remote SDP Server returns when a SDP Service Attribute Request
        /// is processed.  If there are Attributes, then the
        /// Number_Attribute_Values member will contain a non-zero value, and
        /// the SDP_Service_Attribute_Value_Data will point to an array of
        /// Service Attributes (Attribute ID/Attribute Data).  If the
        /// Number_Attribute_Values member is zero, then there are NO
        /// Attributes present in the list, and the
        /// SDP_Service_Attribute_Value_Data member will be NULL.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal struct SDP_Service_Attribute_Response_Data
        {
            internal readonly ushort Number_Attribute_Values;
            internal readonly IntPtr/*"SDP_Service_Attribute_Value_Data_t*"*/ pSDP_Service_Attribute_Value_Data;

            /* For unit-test. */
            internal SDP_Service_Attribute_Response_Data(ushort Number_Attribute_Values,
                IntPtr/*"SDP_Service_Attribute_Value_Data_t*"*/ pSDP_Service_Attribute_Value_Data)
            {
                this.Number_Attribute_Values = Number_Attribute_Values;
                this.pSDP_Service_Attribute_Value_Data = pSDP_Service_Attribute_Value_Data;
            }
        }

        internal struct SDP_Service_Attribute_Response_Data_ArrayOfChildren
        {
            internal readonly ushort Number_Attribute_Values;
            internal readonly SDP_Service_Attribute_Value_Data[] aSDP_Service_Attribute_Value_Data;

            // /* For unit-test. */
            //internal SDP_Service_Attribute_Response_Data_t_ArrayOfChildren(Word_t Number_Attribute_Values,
            //    IntPtr/*"SDP_Service_Attribute_Value_Data_t*"*/ pSDP_Service_Attribute_Value_Data)
            //{
            //    this.Number_Attribute_Values = Number_Attribute_Values;
            //    this.pSDP_Service_Attribute_Value_Data = pSDP_Service_Attribute_Value_Data;
            //}
        }

        //#define SDP_SERVICE_ATTRIBUTE_RESPONSE_DATA_SIZE        (sizeof(SDP_Service_Attribute_Response_Data_t))

        /// <summary>
        /// The following Data Structure represents a Service Record Attribute
        /// List that the Remote SDP Server returns when a SDP Service Search
        /// Attribute Request is processed.  If there were Service Records
        /// that matched the requested Search Pattern, then the
        /// Number_Service_Records member will contain a non-zero value, and
        /// the SDP_Service_Attribute_Response member will point to an array
        /// of SDP_Service_Attribute_Response_t data structures.  Each element
        /// of this array will specify the Attribute(s) for an individual
        /// Service Record .  If the Number_Service_Records member is zero,
        /// then there are NO Service Records (and thus NO Attributes)
        /// present in the list, and the SDP_Service_Attribute_Response_Data
        /// member will be NULL.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
        internal struct SDP_Service_Search_Attribute_Response_Data
        {
            internal readonly ushort Number_Service_Records;
            internal readonly IntPtr/*"SDP_Service_Attribute_Response_Data_t*"*/ pSDP_Service_Attribute_Response_Data;

            /* For unit-test. */
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "For unit-test.")]
            internal SDP_Service_Search_Attribute_Response_Data(ushort Number_Service_Records,
                IntPtr/*"SDP_Service_Attribute_Response_Data_t*"*/ pSDP_Service_Attribute_Response_Data)
            {
                this.Number_Service_Records = Number_Service_Records;
                this.pSDP_Service_Attribute_Response_Data = pSDP_Service_Attribute_Response_Data;
            }
        }

        //#define SDP_SERVICE_SEARCH_ATTRIBUTE_RESPONSE_DATA_SIZE (sizeof(SDP_Service_Search_Attribute_Response_Data_t))

        /// <summary>
        /// The following Data Structure represents the Data that is
        /// returned in the SDP Response Callback.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>This Data Structure
        /// is a container for ALL possible Data that could be returned in
        /// a SDP Response Callback.  Note that the type of Data returned
        /// depends on the SDP Request that the callback was installed for.
        /// Also note that for the rdTimeout and rdConnectionError
        /// SDP_Response_Data_Types there is NO further Data available for
        /// these Errors.
        /// </para>
        /// </remarks>
        internal struct SDP_Response_Data
        {
            internal readonly StackConsts.SDP_Response_Data_Type SDP_Response_Data_Type;
            //union
            //{
            //   SDP_Error_Response_Data_t                    SDP_Error_Response_Data;
            //   SDP_Service_Search_Response_Data_t           SDP_Service_Search_Response_Data;
            //   SDP_Service_Attribute_Response_Data_t        SDP_Service_Attribute_Response_Data;
            //   SDP_Service_Search_Attribute_Response_Data_t SDP_Service_Search_Attribute_Response_Data;
            //} SDP_Response_Data;
        }

        //#define SDP_RESPONSE_DATA_SIZE                          (sizeof(SDP_Response_Data_t))

        //====


        //--
        /* The following Data Structure represents a Service Record Attribute*/
        /* List that the Remote SDP Server returns when a SDP Service Search */
        /* Attribute Request is processed.  If there were Service Records    */
        /* that matched the requested Search Pattern, then the               */
        /* Number_Service_Records member will contain a non-zero value, and  */
        /* the SDP_Service_Attribute_Response member will point to an array  */
        /* of SDP_Service_Attribute_Response_t data structures.  Each element*/
        /* of this array will specify the Attribute(s) for an individual     */
        /* Service Record .  If the Number_Service_Records member is zero,   */
        /* then there are NO Service Records (and thus NO Attributes)        */
        /* present in the list, and the SDP_Service_Attribute_Response_Data  */
        /* member will be NULL.                                              */
        internal struct SDP_Response_Data__SDP_Service_Search_Attribute_Response_Data
        {
            internal readonly StackConsts.SDP_Response_Data_Type SDP_Response_Data_Type;
            /**/
            internal readonly ushort Number_Service_Records;
            internal readonly IntPtr/*"SDP_Service_Attribute_Response_Data_t*"*/ pSDP_Service_Attribute_Response_Data;

            /* For unit-test. */
            internal SDP_Response_Data__SDP_Service_Search_Attribute_Response_Data(
                StackConsts.SDP_Response_Data_Type SDP_Response_Data_Type,
                ushort Number_Service_Records,
                IntPtr/*"SDP_Service_Attribute_Response_Data_t*"*/ pSDP_Service_Attribute_Response_Data)
            {
                this.SDP_Response_Data_Type = SDP_Response_Data_Type;
                this.Number_Service_Records = Number_Service_Records;
                this.pSDP_Service_Attribute_Response_Data = pSDP_Service_Attribute_Response_Data;
            }
        }

        internal struct SDP_Response_Data__SDP_Error_Response_Data
        {
            internal readonly StackConsts.SDP_Response_Data_Type SDP_Response_Data_Type;
            //--
            internal readonly ushort Error_Code;
            internal readonly ushort Error_Info_Length;
            internal readonly byte[] Error_Info;
        }

        #endregion

    }
}

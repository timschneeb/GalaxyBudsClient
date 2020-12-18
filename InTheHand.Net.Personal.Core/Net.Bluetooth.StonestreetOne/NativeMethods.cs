// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.NativeMethods
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

//using Byte_t = System.Byte;
//using Word_t = System.UInt16;   /* Generic 16 bit Container.  */
//
using BD_ADDR_BY_VALUE = System.Int64;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    static class NativeMethods
    {
        const string LibraryName = "SS1BTPS.dll";
        const string SymbolUtilityLibraryName = "SS1BTSYM.dll";
        const string SS1PWMGR = "SS1PWMGR.dll";
        const string CustomerDll = "CustomerDLL.dll";
#if NETCF
        const string TestPinvokeLibraryName = "TestPinvokeTarget.dll";
#else
        const string TestPinvokeLibraryName = "TPTWin32.dll";
#endif

        #region BSCAPI.h
        /*[DllImport(LibraryName)]
        internal static extern int BSC_Initialize(byte[] HCI_DriverInformation,
            StackConsts.BSC_INITIALIZE_FLAGs Flags);*/

        [DllImport(LibraryName)]
        internal static extern int BSC_Initialize([In] ref Structs.HCI_DriverInformation__HCI_COMMDriverInformation HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags);
        [DllImport(LibraryName)]
        internal static extern int BSC_Initialize([In] byte[] HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags);

        [DllImport(LibraryName)]
        internal static extern void BSC_Shutdown(uint BluetoothStackID);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError BSC_RegisterDebugCallback(
            uint BluetoothStackID, BSC_Debug_Callback_t BSC_DebugCallback,
            uint CallbackParameter);

        //
        [DllImport(LibraryName)]
        internal static extern BluetopiaError BSC_Read_RSSI(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR, out sbyte RSSI);
        #endregion

        #region HCIAPI.h
        // Function 'HCI_Version_Supported' is cheaper, it does not access the controller.
        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Read_Local_Version_Information(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult,
            out HciVersion HCI_VersionResult, out ushort HCI_RevisionResult,
            out LmpVersion LMP_VersionResult, out Manufacturer Manufacturer_NameResult, out ushort LMP_SubversionResult);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Read_Local_Supported_Features(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult, byte[] LMP_FeaturesResult);


        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Register_Event_Callback(uint BluetoothStackID,
            HCI_Event_Callback_t HCI_EventCallback, uint CallbackParameter);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        // TODO: [Obsolete("GAP_Query_Remote_Device_Name_Ex")]
        internal static extern BluetopiaError HCI_Remote_Name_Request(
            uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            byte Page_Scan_Repetition_Mode, byte Page_Scan_Mode, ushort Clock_Offset,
            out StackConsts.HCI_ERROR_CODE StatusResult);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Remote_Name_Request_Cancel(
            uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR, out byte StatusResult,
            /*"BD_ADDR_t *"*/ [Out] byte[] BD_ADDRResult);


        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Create_Connection(uint BluetoothStackID,
            BD_ADDR_BY_VALUE BD_ADDR, ushort Packet_Type,
            byte Page_Scan_Repetition_Mode, byte Page_Scan_Mode, ushort Clock_Offset,
            byte Allow_Role_Switch,
            out StackConsts.HCI_ERROR_CODE StatusResult);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Add_SCO_Connection(uint BluetoothStackID,
            ushort Connection_Handle, /*ushort*/StackConsts.HCI_PACKET_SCO_TYPE__u16 Packet_Type,
            out StackConsts.HCI_ERROR_CODE StatusResult);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError HCI_Send_Raw_Command(uint bluetoothStackID,
            byte command_OGF, ushort command_OCF, byte command_Length, byte[] command_Data,
            out byte statusResult, ref byte lengthResult, byte[] bufferResult,
            [MarshalAs(UnmanagedType.Bool)] bool WaitForResponse);
        #endregion

        #region GAPAPI.h
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Set_Discoverability_Mode(uint BluetoothStackID, StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode, uint Max_Discoverable_Time);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Discoverability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode,
            out uint Max_Discoverable_Time);
        //
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Set_Connectability_Mode(uint BluetoothStackID, StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Connectability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode);
        //
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Set_Pairability_Mode(uint BluetoothStackID, StackConsts.GAP_Pairability_Mode GAP_Pairability_Mode);
        //--
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Authenticate_Remote_Device(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Register_Remote_Authentication(uint BluetoothStackID,
            GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Authentication_Response(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            ref Structs.GAP_Authentication_Information GAP_Authentication_Information);
        //[DllImport("TestPinvokeTarget")]//, EntryPoint = "My2GAP_Authentication_Response")]
        //internal static extern BluetopiaError My2GAP_Authentication_Response(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
        //    ref Structs.GAP_Authentication_Information GAP_Authentication_Information);
        //--
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Perform_Inquiry(uint BluetoothStackID, StackConsts.GAP_Inquiry_Type GAP_Inquiry_Type,
            uint MinimumPeriodLength, uint MaximumPeriodLength,
            uint InquiryLength, uint MaximumResponses,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        //
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Remote_Device_Name(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Remote_Device_Name_Ex(uint BluetoothStackID,
            BD_ADDR_BY_VALUE BD_ADDR, byte Page_Scan_Repetition_Mode, ushort Clock_Offset,
            GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Cancel_Query_Remote_Device_Name(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR);
        // Bonding
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Initiate_Bonding(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            StackConsts.GAP_Bonding_Type GAP_Bonding_Type, GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter);
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_End_Bonding(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR);
        //--
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Local_BD_ADDR(uint BluetoothStackID, [Out] /*"BD_ADDR_t *"*/byte[] BD_ADDR);
        //
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Set_Class_Of_Device(uint BluetoothStackID,
            /*'struct Class_of_Device_t'*/uint Class_of_Device);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Class_Of_Device(uint BluetoothStackID, out uint/*"Class_of_Device_t*"*/ Class_of_Device);
        //Local Name
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Set_Local_Device_Name(uint BluetoothStackID, byte[] Name);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Local_Device_Name(uint BluetoothStackID, int NameBufferLength, byte[] NameBuffer);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError GAP_Query_Local_Out_Of_Band_Data(uint BluetoothStackID,
            IntPtr/*"Simple_Pairing_Hash_t *"*/SimplePairingHash, IntPtr/*"Simple_Pairing_Randomizer_t *"*/SimplePairingRandomizer);
        #endregion

        #region SPPAPI.h
        [DllImport(LibraryName)]
        internal static extern int SPP_Open_Remote_Port(uint BluetoothStackID,
            BD_ADDR_BY_VALUE BD_ADDR, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError SPP_Close_Port(uint BluetoothStackID, uint SerialPortID);
        [DllImport(LibraryName)]
        internal static extern int SPP_Open_Server_Port(uint BluetoothStackID, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter);
        [DllImport(LibraryName)]
        internal static extern BluetopiaError SPP_Close_Server_Port(uint BluetoothStackID, uint SerialPortID);
        [DllImport(LibraryName)]
        internal static extern int SPP_Data_Write(uint BluetoothStackID, uint SerialPortID, ushort DataLength, byte[] DataBuffer);
        [DllImport(LibraryName)]
        internal static extern int SPP_Data_Read(uint BluetoothStackID, uint SerialPortID, ushort DataBufferSize, byte[] DataBuffer);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError SPP_Register_SDP_Record(uint BluetoothStackID, uint SerialPortID,
            ref Structs.SPP_SDP_Service_Record SDPServiceRecord,
            byte[] ServiceNameUtf8, out uint SDPServiceRecordHandle);
#if DEBUG
        [DllImport(TestPinvokeLibraryName)]
        internal static extern BluetopiaError My2_SPP_Register_SDP_Record(uint BluetoothStackID, uint SerialPortID,
            ref Structs.SPP_SDP_Service_Record SDPServiceRecord,
            byte[] ServiceNameUtf8, out uint SDPServiceRecordHandle);
#endif
        #endregion

        #region SDPAPI.h
        [DllImport(LibraryName)]
        internal static extern int SDP_Service_Search_Attribute_Request(uint BluetoothStackID,
            BD_ADDR_BY_VALUE BD_ADDR,
            uint NumberServiceUUID, [In] Structs.SDP_UUID_Entry[] SDP_UUID_Entry,
            uint NumberAttributeListElements, [In] Structs.SDP_Attribute_ID_List_Entry[] AttributeIDList,
            NativeMethods.SDP_Response_Callback SDP_Response_Callback, uint CallbackParameter);

        [DllImport(LibraryName)]
        internal static extern int SDP_Create_Service_Record(uint BluetoothStackID,
            uint NumberServiceClassUUID, [In] Structs.SDP_UUID_Entry_Bytes[] SDP_UUID_Entry);
#if DEBUG
        [DllImport(TestPinvokeLibraryName)]
        internal static extern int My2_SDP_Create_Service_Record(uint BluetoothStackID,
            uint NumberServiceClassUUID, [In] Structs.SDP_UUID_Entry[] SDP_UUID_Entry);
#endif

        [DllImport(LibraryName)]
        internal static extern BluetopiaError SDP_Delete_Service_Record(uint BluetoothStackID, UInt32 Service_Record_Handle);

        [DllImport(LibraryName)]
        internal static extern BluetopiaError SDP_Add_Attribute(uint BluetoothStackID, UInt32 Service_Record_Handle,
            ushort Attribute_ID, [In] ref Structs.SDP_Data_Element__Struct SDP_Data_Element);
#if DEBUG
        [DllImport(TestPinvokeLibraryName)]
        internal static extern BluetopiaError My2_SDP_Add_Attribute(uint BluetoothStackID, UInt32 Service_Record_Handle,
            ushort Attribute_ID, [In] ref Structs.SDP_Data_Element__Struct SDP_Data_Element);
#endif
        #endregion

        #region SYMBUAPI.h
        /*See BSC_Read_RSSI above
        [DllImport(SymbolUtilityLibraryName)]
        internal static extern BluetopiaError SYMB_Read_RSSI(uint BluetoothStackID, Int64 BD_ADDR, out sbyte RSSI);*/

        /// <summary>
        /// The following Bit-Mask constants represent the Power Events that
        /// are reported by the SYMB_Get_Power_Event() function.
        /// </summary>
        /// -
        /// <remarks>
        /// When a Power
        /// Event occurs the SYMB_POWER_INFORMATION_EVENT event shall be set
        /// and a call to SYMB_Get_Power_Event() shall return one (or more) of
        /// these items and may take the appropriate action.
        /// </remarks>
        enum SYMB_DEVICE_POWER : uint
        {
            SYMB_DEVICE_POWER_UP_EVENT = 0x00000001,
            SYMB_FLIGHT_MODE_POWER_UP_REQUEST_EVENT = 0x00000002,
            SYMB_FLIGHT_MODE_POWER_DOWN_REQUEST_EVENT = 0x00000004,
        }
        /// <summary>
        /// The following function is provided to allow a means to get the
        /// last power event that occurred.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>* NOTE * The "SS1PWMGR.dll" driver must be installed for this
        /// feature to function.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="event_Mask">
        /// The only parameter to this
        /// function is a pointer to an event mask that shall be used to
        /// returned that last power event that occurred.
        /// </param>
        /// -
        /// <returns>
        /// This function returns zero if successful or a negative return value
        /// if an error occurred.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(SymbolUtilityLibraryName)]
        /*internal*/
        static extern BluetopiaError SYMB_Get_Power_Event(ref SYMB_DEVICE_POWER event_Mask);
        #endregion

        #region SS1PWMGR.dll
        // http://support.symbol.com/support/search.do?cmd=displayKC&docType=kc&externalId=10284&sliceId=SAL_Public&dialogID=32604907&stateId=0%200%2032602996
        // "OEMBthGetMode returns two states ON/OFF. If the value is returned
        // as 0, Bluetooth is OFF. If it returns 1, Bluetooth radio is ON."
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(SS1PWMGR)]
        internal static extern int OEMGetBthPowerState([MarshalAs(UnmanagedType.Bool)] out bool pdwMode);
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [DllImport(SS1PWMGR)]
        internal static extern int OEMSetBthPowerState([MarshalAs(UnmanagedType.Bool)] bool dwMode);
        #endregion

        #region Customer.dll
        // Calling this does NOT close BTExplorer.exe
        //[DllImport(CustomerDll)]
        //internal static extern void BTPCloseAPI();
        #endregion

        //=====================
        // Delegates
        //=====================
        #region BSCAPI.h Delegates
        internal delegate void BSC_Debug_Callback_t(uint BluetoothStackID, Boolean PacketSent,
            IntPtr/*"HCI_Packet_t*"*/ HCIPacket, uint CallbackParameter);
        #endregion

        #region HCIAPI.h Delegates
        internal delegate void HCI_Event_Callback_t(uint BluetoothStackID,
            ref Structs.HCI_Event_Data_t HCI_Event_Data, uint CallbackParameter);
        #endregion

        #region GAPAPI.h Delegates
        internal delegate void GAP_Event_Callback(uint BluetoothStackID,
            [In] ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter);
        #endregion

        #region SPPAPI.h Delegates
        internal delegate void SPP_Event_Callback(uint BluetoothStackID,
            [In] ref Structs.SPP_Event_Data SPP_Event_Data, uint CallbackParameter);
        #endregion

        #region SDPAPI.h Delegates
        internal delegate void SDP_Response_Callback(uint BluetoothStackID, uint SDPRequestID,
            IntPtr/*"SDP_Response_Data_t *"*/ SDP_Response_Data, uint CallbackParameter);
        #endregion

        internal enum ApiVersion
        {
            Unknown,
            PreSsp,
            /// <summary>
            /// 'Simple Secure Pairing' version.
            /// Where SSO added (int32) field to unions in the auth structs and thus changed their alignment!
            /// </summary>
            Ssp, // 
        }
    }
}

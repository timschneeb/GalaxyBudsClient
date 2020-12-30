// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaRealApi
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using BD_ADDR_BY_VALUE = System.Int64;
//
using System;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaRealApi : IBluetopiaApi
    {

        #region BSCAPI.h

        int IBluetopiaApi.BSC_Initialize(ref Structs.HCI_DriverInformation__HCI_COMMDriverInformation HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags)
        {
            //TestStru6();
            //
            return NativeMethods.BSC_Initialize(ref HCI_DriverInformation, Flags);
        }

        int IBluetopiaApi.BSC_Initialize(byte[] HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags)
        {
            //TestStru6();
            //
            return NativeMethods.BSC_Initialize(HCI_DriverInformation, Flags);
        }

        void IBluetopiaApi.BSC_Shutdown(uint BluetoothStackID)
        {
            NativeMethods.BSC_Shutdown(BluetoothStackID);
        }

        BluetopiaError IBluetopiaApi.BSC_Read_RSSI(uint BluetoothStackID, Int64 BD_ADDR, out sbyte RSSI)
        {
            return NativeMethods.BSC_Read_RSSI(BluetoothStackID, BD_ADDR, out RSSI);
        }

        #endregion

        #region HCIAPI.h
        BluetopiaError IBluetopiaApi.HCI_Read_Local_Version_Information(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult,
            out HciVersion HCI_VersionResult, out ushort HCI_RevisionResult,
            out LmpVersion LMP_VersionResult, out Manufacturer Manufacturer_NameResult, out ushort LMP_SubversionResult)
        {
            return NativeMethods.HCI_Read_Local_Version_Information(BluetoothStackID,
                out StatusResult, out HCI_VersionResult, out HCI_RevisionResult,
                out LMP_VersionResult, out Manufacturer_NameResult, out LMP_SubversionResult);
        }

        BluetopiaError IBluetopiaApi.HCI_Read_Local_Supported_Features(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult, byte[] LMP_FeaturesResult)
        {
            return NativeMethods.HCI_Read_Local_Supported_Features(BluetoothStackID,
                out StatusResult, LMP_FeaturesResult);
        }
        #endregion

        #region GAPAPI.h
        BluetopiaError IBluetopiaApi.GAP_Authenticate_Remote_Device(uint BluetoothStackID, Int64 BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.GAP_Authenticate_Remote_Device(BluetoothStackID, BD_ADDR,
                    GAP_Event_Callback, CallbackParameter);
        }

        BluetopiaError IBluetopiaApi.GAP_Initiate_Bonding(uint BluetoothStackID, Int64 BD_ADDR,
            StackConsts.GAP_Bonding_Type GAP_Bonding_Type,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.GAP_Initiate_Bonding(BluetoothStackID, BD_ADDR,
                GAP_Bonding_Type,
                GAP_Event_Callback, CallbackParameter);
        }

        BluetopiaError IBluetopiaApi.GAP_Register_Remote_Authentication(uint BluetoothStackID,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.GAP_Register_Remote_Authentication(BluetoothStackID,
                GAP_Event_Callback, CallbackParameter);
        }

        BluetopiaError IBluetopiaApi.GAP_Authentication_Response(uint BluetoothStackID, Int64 BD_ADDR,
            ref Structs.GAP_Authentication_Information GAP_Authentication_Information)
        {
            //NativeMethods.My2GAP_Authentication_Response(BluetoothStackID, BD_ADDR,
            //    ref GAP_Authentication_Information);
            return NativeMethods.GAP_Authentication_Response(BluetoothStackID, BD_ADDR,
                ref GAP_Authentication_Information);
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Local_BD_ADDR(uint BluetoothStackID, /*"BD_ADDR_t *"*/byte[] BD_ADDR)
        {
            return NativeMethods.GAP_Query_Local_BD_ADDR(BluetoothStackID, BD_ADDR);
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Local_Device_Name(uint BluetoothStackID, int NameBufferLength, byte[] NameBuffer)
        {
            return NativeMethods.GAP_Query_Local_Device_Name(BluetoothStackID, NameBufferLength, NameBuffer);
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Class_Of_Device(uint BluetoothStackID, out uint/*"Class_of_Device_t*"*/ Class_of_Device)
        {
            return NativeMethods.GAP_Query_Class_Of_Device(BluetoothStackID, out Class_of_Device);
        }
        //
        BluetopiaError IBluetopiaApi.GAP_Query_Connectability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode)
        {
            return NativeMethods.GAP_Query_Connectability_Mode(BluetoothStackID,
                out GAP_Connectability_Mode);
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Connectability_Mode(uint BluetoothStackID, StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode)
        {
            return NativeMethods.GAP_Set_Connectability_Mode(BluetoothStackID, GAP_Connectability_Mode);
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Discoverability_Mode(uint BluetoothStackID,
            out StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode,
            out uint Max_Discoverable_Time)
        {
            return NativeMethods.GAP_Query_Discoverability_Mode(BluetoothStackID,
                out GAP_Discoverability_Mode, out Max_Discoverable_Time);
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Discoverability_Mode(uint BluetoothStackID, StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode,
            uint Max_Discoverable_Time)
        {
            return NativeMethods.GAP_Set_Discoverability_Mode(BluetoothStackID, GAP_Discoverability_Mode, Max_Discoverable_Time);
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Pairability_Mode(uint BluetoothStackID, StackConsts.GAP_Pairability_Mode GAP_Pairability_Mode)
        {
            return NativeMethods.GAP_Set_Pairability_Mode(BluetoothStackID, GAP_Pairability_Mode);
        }

        //
        BluetopiaError IBluetopiaApi.GAP_Query_Remote_Device_Name(uint BluetoothStackID, Int64 BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.GAP_Query_Remote_Device_Name(BluetoothStackID, BD_ADDR,
                GAP_Event_Callback, CallbackParameter);
        }
        //
        BluetopiaError IBluetopiaApi.GAP_Perform_Inquiry(uint BluetoothStackID, StackConsts.GAP_Inquiry_Type GAP_Inquiry_Type,
            uint MinimumPeriodLength, uint MaximumPeriodLength,
            uint InquiryLength, uint MaximumResponses,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.GAP_Perform_Inquiry(BluetoothStackID, GAP_Inquiry_Type,
                MinimumPeriodLength, MaximumPeriodLength,
                InquiryLength, MaximumResponses,
                GAP_Event_Callback, CallbackParameter);
        }

        //
        BluetopiaError IBluetopiaApi.GAP_Query_Local_Out_Of_Band_Data(uint BluetoothStackID,
            IntPtr/*"Simple_Pairing_Hash_t *"*/SimplePairingHash, IntPtr/*"Simple_Pairing_Randomizer_t *"*/SimplePairingRandomizer)
        {
            return NativeMethods.GAP_Query_Local_Out_Of_Band_Data(BluetoothStackID,
                SimplePairingHash, SimplePairingRandomizer);
        }
        #endregion

        #region SPPAPI.h
        int IBluetopiaApi.SPP_Open_Remote_Port(uint BluetoothStackID, Int64 BD_ADDR, uint ServerPort, NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.SPP_Open_Remote_Port(BluetoothStackID, BD_ADDR, ServerPort,
                SPP_Event_Callback, CallbackParameter);
        }

        BluetopiaError IBluetopiaApi.SPP_Close_Port(uint BluetoothStackID, uint SerialPortID)
        {
            return NativeMethods.SPP_Close_Port(BluetoothStackID, SerialPortID);
        }

        int IBluetopiaApi.SPP_Open_Server_Port(uint BluetoothStackID, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter)
        {
            return NativeMethods.SPP_Open_Server_Port(BluetoothStackID, ServerPort,
                SPP_Event_Callback, CallbackParameter);
        }

        BluetopiaError IBluetopiaApi.SPP_Close_Server_Port(uint BluetoothStackID, uint SerialPortID)
        {
            return NativeMethods.SPP_Close_Server_Port(BluetoothStackID, SerialPortID);
        }

        BluetopiaError IBluetopiaApi.SPP_Register_SDP_Record(uint BluetoothStackID, uint SerialPortID,
            ref Structs.SPP_SDP_Service_Record SDPServiceRecord,
            byte[] ServiceNameUtf8, out uint SDPServiceRecordHandle)
        {
            return NativeMethods.SPP_Register_SDP_Record(BluetoothStackID, SerialPortID,
                ref SDPServiceRecord,
                ServiceNameUtf8, out SDPServiceRecordHandle);
        }

        int IBluetopiaApi.SPP_Data_Write(uint BluetoothStackID, uint SerialPortID, ushort DataLength, byte[] DataBuffer)
        {
            return NativeMethods.SPP_Data_Write(BluetoothStackID, SerialPortID, DataLength, DataBuffer);
        }

        int IBluetopiaApi.SPP_Data_Read(uint BluetoothStackID, uint SerialPortID, ushort DataBufferSize, byte[] DataBuffer)
        {
            return NativeMethods.SPP_Data_Read(BluetoothStackID, SerialPortID, DataBufferSize, DataBuffer);
        }
        #endregion

        #region SDPAPI.h
        [Obsolete("TODO")]
        int IBluetopiaApi.SDP_Service_Search_Attribute_Request(uint BluetoothStackID, Int64 BD_ADDR,
            uint NumberServiceUUID, Structs.SDP_UUID_Entry[] SDP_UUID_Entry,
            uint NumberAttributeListElements, Structs.SDP_Attribute_ID_List_Entry[] AttributeIDList,
            NativeMethods.SDP_Response_Callback SDP_Response_Callback, uint CallbackParameter)
        {
            return NativeMethods.SDP_Service_Search_Attribute_Request(BluetoothStackID,
                BD_ADDR,
                NumberServiceUUID, SDP_UUID_Entry,
                NumberAttributeListElements, AttributeIDList,
                SDP_Response_Callback, CallbackParameter);
        }

#if false
        private static Structs.BdAddrStruFields6 MakeBdAddrStruct6f(byte[] BD_ADDR)
        {
            var stru = new Structs.BdAddrStruFields6();
            stru.b0 = BD_ADDR[0];
            stru.b1 = BD_ADDR[1];
            stru.b2 = BD_ADDR[2];
            stru.b3 = BD_ADDR[3];
            stru.b4 = BD_ADDR[4];
            stru.b5 = BD_ADDR[5];
            return stru;
        }

        private static Structs.BdAddrStruArr6 MakeBdAddrStruct6a(byte[] BD_ADDR)
        {
            var stru = new Structs.BdAddrStruArr6();
            stru.addr = new byte[6];
            BD_ADDR.CopyTo(stru.addr, 0);
            return stru;
        }
#endif

        int IBluetopiaApi.SDP_Create_Service_Record(uint BluetoothStackID, uint NumberServiceClassUUID, Structs.SDP_UUID_Entry_Bytes[] SDP_UUID_Entry)
        {
            //NativeMethods.My2_SDP_Create_Service_Record(BluetoothStackID, NumberServiceClassUUID, SDP_UUID_Entry);
            return NativeMethods.SDP_Create_Service_Record(BluetoothStackID, NumberServiceClassUUID, SDP_UUID_Entry);
        }

        BluetopiaError IBluetopiaApi.SDP_Delete_Service_Record(uint BluetoothStackID, uint Service_Record_Handle)
        {
            return NativeMethods.SDP_Delete_Service_Record(BluetoothStackID, Service_Record_Handle);
        }

        BluetopiaError IBluetopiaApi.SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, ref Structs.SDP_Data_Element__Struct SDP_Data_Element)
        {
#if DEBUG
            //NativeMethods.My2_SDP_Add_Attribute(BluetoothStackID, Service_Record_Handle,
            //    Attribute_ID, ref SDP_Data_Element);
#endif
            return NativeMethods.SDP_Add_Attribute(BluetoothStackID, Service_Record_Handle,
                Attribute_ID, ref SDP_Data_Element);
        }
        BluetopiaError IBluetopiaApi.SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, Structs.SDP_Data_Element__Class SDP_Data_Element)
        {
            throw new NotSupportedException();
        }
        #endregion

#if false
        //==========================
        public const string TestPinvokeTarget_Dll = "TestPinvokeTarget";

        static class NativeMethods_Test
        {
            [System.Runtime.InteropServices.DllImport(InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRealApi.TestPinvokeTarget_Dll)]
            internal static extern int GAP_Authenticate_Remote_Device(uint BluetoothStackID,
                byte[] BD_ADDR,
                /*GAP_Event_Callback*/IntPtr GAP_Event_Callback,
                uint CallbackParameter);
            [System.Runtime.InteropServices.DllImport(InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRealApi.TestPinvokeTarget_Dll)]
            internal static extern int GAP_Authenticate_Remote_Device(uint BluetoothStackID,
                StonestreetOne.Structs.BdAddrStruFields6 BD_ADDR,
                /*GAP_Event_Callback*/IntPtr GAP_Event_Callback,
                uint CallbackParameter);
            [System.Runtime.InteropServices.DllImport(InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRealApi.TestPinvokeTarget_Dll)]
            internal static extern int GAP_Authenticate_Remote_Device(uint BluetoothStackID,
                StonestreetOne.Structs.BdAddrStruArr6 BD_ADDR,
                /*GAP_Event_Callback*/IntPtr GAP_Event_Callback,
                uint CallbackParameter);
            [System.Runtime.InteropServices.DllImport(InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRealApi.TestPinvokeTarget_Dll)]
            internal static extern int GAP_Authenticate_Remote_Device(uint BluetoothStackID,
                Int64 BD_ADDR,
                /*GAP_Event_Callback*/IntPtr GAP_Event_Callback,
                uint CallbackParameter);
        }

        private static void TestStru6()
        {
            byte[] addrBytes = { 0xa4, 0x4c, 0x24, 0x98, 0x80, 0x00 };
            Structs.BdAddrStruFields6 stru6f = MakeBdAddrStruct6f(addrBytes);
            Structs.BdAddrStruArr6 stru6a = MakeBdAddrStruct6a(addrBytes);
            var arr8 = new byte[8]; addrBytes.CopyTo(arr8, 0);
            Int64 addrInt64 = BitConverter.ToInt64(arr8, 0);
            int ret;
            //
            IntPtr PFn = (IntPtr)0x31234567;
            try {
                ret = NativeMethods_Test.GAP_Authenticate_Remote_Device(
                    0x12345678, addrBytes, PFn, 0x41234567);
            } catch (Exception ex) {
            }
            try {
                ret = NativeMethods_Test.GAP_Authenticate_Remote_Device(
                    0x12345678, stru6f, PFn, 0x41234567);
            } catch (Exception ex) {
            }
            try {
                ret = NativeMethods_Test.GAP_Authenticate_Remote_Device(
                    0x12345678, stru6a, PFn, 0x41234567);
            } catch (Exception ex) {
            }
            try {
                ret = NativeMethods_Test.GAP_Authenticate_Remote_Device(
                    0x12345678, addrInt64, PFn, 0x41234567);
            } catch (Exception ex) {
            }
            //
            var hLib = Widcomm.WidcommBtIf.NativeMethods.LoadLibraryEx(
                TestPinvokeTarget_Dll, IntPtr.Zero, 0);
            int gle = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        }
#endif
    }
}

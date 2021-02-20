// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaFakeApi
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using BD_ADDR_BY_VALUE = System.Int64;
//
using System;
using System.Text;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaFakeApi : IBluetopiaApi
    {
        int IBluetopiaApi.BSC_Initialize(ref Structs.HCI_DriverInformation__HCI_COMMDriverInformation HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags)
        {
            return 99;
        }
        int IBluetopiaApi.BSC_Initialize(byte[] HCI_DriverInformation,
            /*unsigned long*/StackConsts.BSC_INITIALIZE_FLAGs Flags)
        {
            return 99;
        }

        void IBluetopiaApi.BSC_Shutdown(uint BluetoothStackID)
        {
        }

        BluetopiaError IBluetopiaApi.BSC_Read_RSSI(uint BluetoothStackID, Int64 BD_ADDR, out sbyte RSSI)
        {
            RSSI = sbyte.MinValue;
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        //--
        #region HCIAPI.h
        BluetopiaError IBluetopiaApi.HCI_Read_Local_Version_Information(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult,
            out HciVersion HCI_VersionResult, out ushort HCI_RevisionResult,
            out LmpVersion LMP_VersionResult, out Manufacturer Manufacturer_NameResult, out ushort LMP_SubversionResult)
        {
            StatusResult = 0;
            HCI_VersionResult = HciVersion.Unknown;
            LMP_VersionResult = LmpVersion.Unknown;
            HCI_RevisionResult = LMP_SubversionResult = 0xFFFF;
            Manufacturer_NameResult = Manufacturer.Unknown;
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }
        BluetopiaError IBluetopiaApi.HCI_Read_Local_Supported_Features(uint BluetoothStackID,
            out StackConsts.HCI_ERROR_CODE StatusResult, byte[] LMP_FeaturesResult)
        {
            StatusResult = 0;
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }
        #endregion

        #region Radio
        BluetopiaError IBluetopiaApi.GAP_Query_Local_BD_ADDR(uint BluetoothStackID, byte[] BD_ADDR)
        {
            byte[] addrBytes = { 0x00, 0x22, 0x33, 0x44, 0x55, 0x66 };
            addrBytes.CopyTo(BD_ADDR, 0);
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Local_Device_Name(uint BluetoothStackID, int NameBufferLength, byte[] NameBuffer)
        {
            Encoding.UTF8.GetBytes("AlanIsCool").CopyTo(NameBuffer, 0);
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Class_Of_Device(uint BluetoothStackID, out uint Class_of_Device)
        {
            Class_of_Device = 0x010203;
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Connectability_Mode(uint BluetoothStackID, out StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode)
        {
            GAP_Connectability_Mode = StackConsts.GAP_Connectability_Mode.ConnectableMode;
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Connectability_Mode(uint BluetoothStackID, StackConsts.GAP_Connectability_Mode GAP_Connectability_Mode)
        {
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Query_Discoverability_Mode(uint BluetoothStackID, out StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode, out uint Max_Discoverable_Time)
        {
            GAP_Discoverability_Mode = StackConsts.GAP_Discoverability_Mode.NonDiscoverableMode;
            Max_Discoverable_Time = 0;
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Discoverability_Mode(uint BluetoothStackID, StackConsts.GAP_Discoverability_Mode GAP_Discoverability_Mode,
            uint Max_Discoverable_Time)
        {
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Set_Pairability_Mode(uint BluetoothStackID, StackConsts.GAP_Pairability_Mode GAP_Pairability_Mode)
        {
            return BluetopiaError.OK;
        }
        #endregion

        #region Authentication
        BluetopiaError IBluetopiaApi.GAP_Query_Remote_Device_Name(uint BluetoothStackID, Int64 BD_ADDR, NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        BluetopiaError IBluetopiaApi.GAP_Authenticate_Remote_Device(uint BluetoothStackID, Int64 BD_ADDR,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        BluetopiaError IBluetopiaApi.GAP_Register_Remote_Authentication(uint BluetoothStackID,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.GAP_Initiate_Bonding(uint BluetoothStackID, BD_ADDR_BY_VALUE BD_ADDR,
            StackConsts.GAP_Bonding_Type GAP_Bonding_Type,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        BluetopiaError IBluetopiaApi.GAP_Authentication_Response(uint BluetoothStackID, Int64 BD_ADDR,
            ref Structs.GAP_Authentication_Information GAP_Authentication_Information)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }
        #endregion

        #region DeviceDiscovery
        BluetopiaError IBluetopiaApi.GAP_Perform_Inquiry(uint BluetoothStackID,
            StackConsts.GAP_Inquiry_Type GAP_Inquiry_Type,
            uint MinimumPeriodLength, uint MaximumPeriodLength,
            uint InquiryLength, uint MaximumResponses,
            NativeMethods.GAP_Event_Callback GAP_Event_Callback, uint CallbackParameter)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }
        #endregion

        #region RfcommComms
        int IBluetopiaApi.SPP_Open_Remote_Port(uint BluetoothStackID, Int64 BD_ADDR, uint ServerPort, NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter)
        {
            return (int)BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        BluetopiaError IBluetopiaApi.SPP_Close_Port(uint BluetoothStackID, uint SerialPortID)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        int IBluetopiaApi.SPP_Open_Server_Port(uint BluetoothStackID, uint ServerPort,
            NativeMethods.SPP_Event_Callback SPP_Event_Callback, uint CallbackParameter)
        {
#if DEBUG
            return 0x12345;
#else
            return (int)BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
#endif
        }

        BluetopiaError IBluetopiaApi.SPP_Register_SDP_Record(uint BluetoothStackID, uint SerialPortID,
            ref Structs.SPP_SDP_Service_Record SDPServiceRecord,
            byte[] ServiceNameUtf8, out uint SDPServiceRecordHandle)
        {
#if true
            SDPServiceRecordHandle = 0;
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
#else
            NativeMethods.My2_SPP_Register_SDP_Record(BluetoothStackID,
                SerialPortID, ref SDPServiceRecord, ServiceNameUtf8, out SDPServiceRecordHandle);
            return BluetopiaError.OK;
#endif
        }

        BluetopiaError IBluetopiaApi.SPP_Close_Server_Port(uint BluetoothStackID, uint SerialPortID)
        {
            return BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        int IBluetopiaApi.SPP_Data_Write(uint BluetoothStackID, uint SerialPortID, ushort DataLength, byte[] DataBuffer)
        {
            return (int)BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        int IBluetopiaApi.SPP_Data_Read(uint BluetoothStackID, uint SerialPortID, ushort DataBufferSize, byte[] DataBuffer)
        {
            return (int)BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        int IBluetopiaApi.SDP_Service_Search_Attribute_Request(uint BluetoothStackID, Int64 BD_ADDR, uint NumberServiceUUID, Structs.SDP_UUID_Entry[] SDP_UUID_Entry, uint NumberAttributeListElements, Structs.SDP_Attribute_ID_List_Entry[] AttributeIDList, NativeMethods.SDP_Response_Callback SDP_Response_Callback, uint CallbackParameter)
        {
            return (int)BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
        }

        #endregion

        #region IBluetopiaApi Members


        int IBluetopiaApi.SDP_Create_Service_Record(uint BluetoothStackID, uint NumberServiceClassUUID, Structs.SDP_UUID_Entry_Bytes[] SDP_UUID_Entry)
        {
#if DEBUG
            //NativeMethods.My2_SDP_Create_Service_Record(BluetoothStackID, NumberServiceClassUUID, SDP_UUID_Entry);
            return (int)0x123456;
#else
            return (int)BluetopiaError.NOT_IMPLEMENTED;
#endif
        }

        BluetopiaError IBluetopiaApi.SDP_Delete_Service_Record(uint BluetoothStackID, uint Service_Record_Handle)
        {
            return BluetopiaError.OK;
        }

        BluetopiaError IBluetopiaApi.SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, ref Structs.SDP_Data_Element__Struct SDP_Data_Element)
        {
#if DEBUG
            //NativeMethods.My2_SDP_Add_Attribute(BluetoothStackID, Service_Record_Handle,
            //    Attribute_ID, ref SDP_Data_Element);
#endif
            return BluetopiaError.OK;
        }
        BluetopiaError IBluetopiaApi.SDP_Add_Attribute(uint BluetoothStackID, uint Service_Record_Handle,
            ushort Attribute_ID, Structs.SDP_Data_Element__Class SDP_Data_Element)
        {
            throw new NotSupportedException();
        }

        //
        BluetopiaError IBluetopiaApi.GAP_Query_Local_Out_Of_Band_Data(uint BluetoothStackID,
            IntPtr/*"Simple_Pairing_Hash_t *"*/SimplePairingHash, IntPtr/*"Simple_Pairing_Randomizer_t *"*/SimplePairingRandomizer)
        {
            // MMEx is what NETCF throws; Win32 would throw EntryPointNotFoundException
            throw new MissingMethodException();
        }
        #endregion
    }
}

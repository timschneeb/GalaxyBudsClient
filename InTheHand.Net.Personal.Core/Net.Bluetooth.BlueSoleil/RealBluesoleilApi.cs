// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth.BlueSoleil;
//
using BtSdkUUIDStru = System.Guid;
using BTDEVHDL = System.UInt32;
using BTSVCHDL = System.UInt32;
using BTCONNHDL = System.UInt32;
using BTSHCHDL = System.UInt32;
using BTSDKHANDLE = System.UInt32;
//
using BTUINT32 = System.UInt32;
using BTUINT8 = System.Byte;
using BTINT32 = System.Int32;
using BTINT16 = System.Int16;
using UINT = System.UInt32;
using ULONG = System.UInt32;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    class RealBluesoleilApi : IBluesoleilApi
    {
        /* ---- Initialization and Termination */
        BtSdkError IBluesoleilApi.Btsdk_Init()
        {
            return NativeMethods.Btsdk_Init();
        }

        bool IBluesoleilApi.Btsdk_IsSDKInitialized()
        {
            return NativeMethods.Btsdk_IsSDKInitialized();
        }

        bool IBluesoleilApi.Btsdk_IsServerConnected()
        {
            return NativeMethods.Btsdk_IsServerConnected();
        }

        BtSdkError IBluesoleilApi.Btsdk_Done()
        {
            return NativeMethods.Btsdk_Done();
        }

        BtSdkError IBluesoleilApi.Btsdk_RegisterGetStatusInfoCB4ThirdParty(ref NativeMethods.Func_ReceiveBluetoothStatusInfo statusCBK)
        {
            return NativeMethods.Btsdk_RegisterGetStatusInfoCB4ThirdParty(statusCBK);
        }

        BtSdkError IBluesoleilApi.Btsdk_RegisterCallback4ThirdParty(ref Structs.BtSdkCallbackStru call_back)
        {
            return NativeMethods.Btsdk_RegisterCallback4ThirdParty(ref call_back);
        }

        BtSdkError IBluesoleilApi.Btsdk_SetStatusInfoFlag(UInt16 usMsgType)
        {
            return NativeMethods.Btsdk_SetStatusInfoFlag(usMsgType);
        }

        /* ---- Memory Management */
        void IBluesoleilApi.Btsdk_FreeMemory(IntPtr memblock)
        {
            NativeMethods.Btsdk_FreeMemory(memblock);
        }

        /* ---- Local Bluetooth Device Initialization */
        BtSdkError IBluesoleilApi.Btsdk_StartBluetooth()
        {
            return NativeMethods.Btsdk_StartBluetooth();
        }

        BtSdkError IBluesoleilApi.Btsdk_StopBluetooth()
        {
            return NativeMethods.Btsdk_StopBluetooth();
        }

        /* ---- Local Device Mode */
        bool IBluesoleilApi.Btsdk_IsBluetoothReady()
        {
            return NativeMethods.Btsdk_IsBluetoothReady();
        }

        bool IBluesoleilApi.Btsdk_IsBluetoothHardwareExisted()
        {
            return NativeMethods.Btsdk_IsBluetoothHardwareExisted();
        }

        /* ---- Local Device Mode */
        BtSdkError IBluesoleilApi.Btsdk_SetDiscoveryMode(StackConsts.DiscoveryMode mode)
        {
            return NativeMethods.Btsdk_SetDiscoveryMode(mode);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetDiscoveryMode(out StackConsts.DiscoveryMode mode)
        {
            return NativeMethods.Btsdk_GetDiscoveryMode(out mode);
        }

        /* ---- Local Device Information */
        BtSdkError IBluesoleilApi.Btsdk_GetLocalDeviceAddress(byte[] bd_addr)
        {
            return NativeMethods.Btsdk_GetLocalDeviceAddress(bd_addr);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetLocalName(byte[] name, ref ushort len)
        {
            return NativeMethods.Btsdk_GetLocalName(name, ref len);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetLocalDeviceClass(out uint device_class)
        {
            return NativeMethods.Btsdk_GetLocalDeviceClass(out device_class);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetLocalLMPInfo(ref Structs.BtSdkLocalLMPInfoStru lmp_info)
        {
            return NativeMethods.Btsdk_GetLocalLMPInfo(ref lmp_info);
        }

        /* ---- Remote Device Discovery */
        BtSdkError IBluesoleilApi.Btsdk_StartDeviceDiscovery(uint device_class, ushort max_num, ushort max_seconds)
        {
            return NativeMethods.Btsdk_StartDeviceDiscovery(device_class, max_num, max_seconds);
        }

        BtSdkError IBluesoleilApi.Btsdk_StopDeviceDiscovery()
        {
            return NativeMethods.Btsdk_StopDeviceDiscovery();
        }

        BtSdkError IBluesoleilApi.Btsdk_UpdateRemoteDeviceName(uint dev_hdl, byte[] name, ref ushort plen)
        {
            return NativeMethods.Btsdk_UpdateRemoteDeviceName(dev_hdl, name, ref plen);
        }

        /* ---- Device Pairing */
        BtSdkError IBluesoleilApi.Btsdk_IsDevicePaired(uint dev_hdl, out bool pis_paired)
        {
            return NativeMethods.Btsdk_IsDevicePaired(dev_hdl, out pis_paired);
        }

        BtSdkError IBluesoleilApi.Btsdk_PairDevice(uint dev_hdl)
        {
            return NativeMethods.Btsdk_PairDevice(dev_hdl);
        }

        BtSdkError IBluesoleilApi.Btsdk_PinCodeReply(uint dev_hdl, byte[] pin_code, UInt16 size)
        {
            return NativeMethods.Btsdk_PinCodeReply(dev_hdl, pin_code, size);
        }

        /* ---- Link Management */
        bool IBluesoleilApi.Btsdk_IsDeviceConnected(uint dev_hdl)
        {
            return NativeMethods.Btsdk_IsDeviceConnected(dev_hdl);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteRSSI(uint device_handle, out sbyte prssi)
        {
            return NativeMethods.Btsdk_GetRemoteRSSI(device_handle, out prssi);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteLinkQuality(uint device_handle, out ushort plink_quality)
        {
            return NativeMethods.Btsdk_GetRemoteLinkQuality(device_handle, out plink_quality);
        }

        /* ---- Remote Device Database Management */
        uint IBluesoleilApi.Btsdk_GetRemoteDeviceHandle(byte[] bd_addr)
        {
            return NativeMethods.Btsdk_GetRemoteDeviceHandle(bd_addr);
        }

        uint IBluesoleilApi.Btsdk_AddRemoteDevice(byte[] bd_addr)
        {
            return NativeMethods.Btsdk_AddRemoteDevice(bd_addr);
        }

        BtSdkError IBluesoleilApi.Btsdk_DeleteRemoteDeviceByHandle(uint dev_hdl)
        {
            return NativeMethods.Btsdk_DeleteRemoteDeviceByHandle(dev_hdl);
        }

        int IBluesoleilApi.Btsdk_GetStoredDevicesByClass(uint dev_class, uint[] pdev_hdl, int max_dev_num)
        {
            return NativeMethods.Btsdk_GetStoredDevicesByClass(dev_class, pdev_hdl, max_dev_num);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteDeviceAddress(uint dev_hdl, byte[] bd_addr)
        {
            return NativeMethods.Btsdk_GetRemoteDeviceAddress(dev_hdl, bd_addr);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteDeviceName(uint dev_hdl, byte[] name, ref ushort plen)
        {
            return NativeMethods.Btsdk_GetRemoteDeviceName(dev_hdl, name, ref  plen);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteDeviceClass(uint dev_hdl, out uint pdevice_class)
        {
            return NativeMethods.Btsdk_GetRemoteDeviceClass(dev_hdl, out pdevice_class);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteDeviceProperty(BTDEVHDL dev_hdl, out Structs.BtSdkRemoteDevicePropertyStru rmt_dev_prop)
        {
            return NativeMethods.Btsdk_GetRemoteDeviceProperty(dev_hdl, out rmt_dev_prop);
        }

        /* ---- Service Discovery */
        BtSdkError IBluesoleilApi.Btsdk_BrowseRemoteServicesEx(uint dev_hdl, Structs.BtSdkSDPSearchPatternStru[] psch_ptn, int ptn_num, uint[] svc_hdl, ref int svc_count)
        {
            return NativeMethods.Btsdk_BrowseRemoteServicesEx(dev_hdl, psch_ptn, ptn_num, svc_hdl, ref svc_count);
        }

        BtSdkError IBluesoleilApi.Btsdk_GetRemoteServiceAttributes(BTSVCHDL svc_hdl, ref Structs.BtSdkRemoteServiceAttrStru attribute)
        {
            return NativeMethods.Btsdk_GetRemoteServiceAttributes(svc_hdl, ref attribute);
        }

        /* ---- Connection Establishment */
        BtSdkError IBluesoleilApi.Btsdk_ConnectEx(BTDEVHDL dev_hdl, UInt16 service_class, UInt32 lParam, out BTCONNHDL conn_hdl)
        {
            return NativeMethods.Btsdk_ConnectEx(dev_hdl, service_class, lParam, out conn_hdl);
        }

        BtSdkError IBluesoleilApi.Btsdk_ConnectEx(BTDEVHDL dev_hdl, UInt16 service_class, ref Structs.BtSdkSPPConnParamStru lParam, out BTCONNHDL conn_hdl)
        {
            return NativeMethods.Btsdk_ConnectEx(dev_hdl, service_class, ref lParam, out conn_hdl);
        }

        /* ---- Connection Database Management */
        //FUNC_EXPORT BTINT32 Btsdk_GetConnectionProperty(BTCONNHDL conn_hdl, PBtSdkConnectionPropertyStru conn_prop);
        BTSDKHANDLE IBluesoleilApi.Btsdk_StartEnumConnection()
        {
            return NativeMethods.Btsdk_StartEnumConnection();
        }

        BTCONNHDL IBluesoleilApi.Btsdk_EnumConnection(BTSDKHANDLE enum_handle, ref Structs.BtSdkConnectionPropertyStru conn_prop)
        {
            return NativeMethods.Btsdk_EnumConnection(enum_handle, ref conn_prop);
        }

        BtSdkError IBluesoleilApi.Btsdk_EndEnumConnection(BTSDKHANDLE enum_handle)
        {
            return NativeMethods.Btsdk_EndEnumConnection(enum_handle);
        }

        /* ---- Connection Release */
        BtSdkError IBluesoleilApi.Btsdk_Disconnect(uint handle)
        {
            return NativeMethods.Btsdk_Disconnect(handle);
        }

        /* 
            Serial Port Profile 
        */
        BtSdkError IBluesoleilApi.Btsdk_InitCommObj(byte com_idx, UInt16 svc_class)
        {
            return NativeMethods.Btsdk_InitCommObj(com_idx, svc_class);
        }

        BtSdkError IBluesoleilApi.Btsdk_DeinitCommObj(BTUINT8 com_idx)
        {
            return NativeMethods.Btsdk_DeinitCommObj(com_idx);
        }

        BTINT16 IBluesoleilApi.Btsdk_GetClientPort(BTCONNHDL conn_hdl)
        {
            return NativeMethods.Btsdk_GetClientPort(conn_hdl);
        }

        //byte Btsdk_GetAvailableExtSPPCOMPort([MarshalAs(UnmanagedType.U1)] bool bIsForLocalSPPService);

        BtSdkError IBluesoleilApi.Btsdk_SearchAppExtSPPService(BTDEVHDL dev_hdl, ref Structs.BtSdkAppExtSPPAttrStru psvc)
        {
            return NativeMethods.Btsdk_SearchAppExtSPPService(dev_hdl, ref psvc);
        }

        BtSdkError IBluesoleilApi.Btsdk_ConnectAppExtSPPService(uint dev_hdl, ref Structs.BtSdkAppExtSPPAttrStru psvc, out uint conn_hdl)
        {
            return NativeMethods.Btsdk_ConnectAppExtSPPService(dev_hdl, ref psvc, out conn_hdl);
        }
    
        //
        BTUINT32 IBluesoleilApi.Btsdk_CommNumToSerialNum(int comportNum)
        {
            return NativeMethods.Btsdk_CommNumToSerialNum(comportNum);
        }

        void IBluesoleilApi.Btsdk_PlugOutVComm(UINT serialNum, StackConsts.COMM_SET flag)
        {
            NativeMethods.Btsdk_PlugOutVComm(serialNum, flag);
        }

        bool IBluesoleilApi.Btsdk_PlugInVComm(UINT serialNum, out UInt32 comportNumber,
            UInt32 usageType, StackConsts.COMM_SET flag, UInt32 dwTimeout)
        {
            return NativeMethods.Btsdk_PlugInVComm(serialNum, out comportNumber,
               usageType, flag, dwTimeout);
        }

        UInt32 IBluesoleilApi.Btsdk_GetASerialNum()
        {
            return NativeMethods.Btsdk_GetASerialNum();
        }

    }
}

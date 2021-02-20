// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
//
using BtSdkUUIDStru = System.Guid;
using BTDEVHDL = System.UInt32;
using BTSVCHDL = System.UInt32;
using BTCONNHDL = System.UInt32;
using BTSHCHDL = System.UInt32;
using BTSDKHANDLE = System.UInt32;
//
using BTINT8 = System.SByte;
using BTINT16 = System.Int16;
using BTINT32 = System.Int32;
using BTUINT8 = System.Byte;
using BTUINT16 = System.UInt16;
using BTUINT32 = System.UInt32;
//
using USHORT = System.UInt16;
using UINT = System.UInt32;
using ULONG = System.UInt32;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    /// <exclude/>
    interface IBluesoleilApi
    {
        //void Btsdk_FreeMemory(IntPtr memblock);

        /* ---- Initialization and Termination */
        //typedef void Func_ReceiveBluetoothStatusInfo(ULONG usMsgType, ULONG pulData, ULONG param, BTUINT8 *arg);
        BtSdkError Btsdk_Init();
        BtSdkError Btsdk_Done();
        /// <summary>
        /// The Btsdk_IsSDKInitialized function indicates whether a successful
        /// call to Btsdk_Init is made.
        /// </summary>
        /// <returns>
        /// </returns>
        bool Btsdk_IsSDKInitialized();
        /// <summary>
        /// The Btsdk_IsServerConnected function checks whether client
        /// application can call BlueSoleil Server APIs.
        /// </summary>
        /// <returns>
        /// When this fuction returns
        /// <see langword="true"/>, client application can call APIs normally, versa versit.
        /// </returns>
        bool Btsdk_IsServerConnected();
        BtSdkError Btsdk_RegisterGetStatusInfoCB4ThirdParty(ref NativeMethods.Func_ReceiveBluetoothStatusInfo statusCBK);
        BtSdkError Btsdk_RegisterCallback4ThirdParty(ref Structs.BtSdkCallbackStru call_back);
        //[DllImport(DllName)]
        //BtSdkError Btsdk_RegisterCallbackEx(ref BtSdkCallbackStru call_back, UInt32 priority);
        /// <summary>
        /// The Btsdk_SetStatusInfoFlag function is used to set the status
        /// changing callback types which the user wants to receive.
        /// </summary>
        /// -
        /// <remarks>
        /// usMsgType can be one of the following value or their combination:
        /// <list type="definition">
        /// <item><term>BTSDK_NTSERVICE_STATUS_FLAG</term>
        /// <description>The status change of BlueSoleil server
        /// event or OS message event.</description>
        /// </item>
        /// <item><term>BTSDK_BLUETOOTH_STATUS_FLAG</term>
        /// <description>Message event of the change of Bluetooth</description>
        /// status.
        /// </item>
        /// <item><term>BTSDK_REFRESH_STATUS_FLAG</term>
        /// <description>Refresh event.</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// -
        /// <param name="usMsgType">See remarks.</param>
        /// -
        /// <returns>BTSDK_OK for success, other for error code.</returns>
        BtSdkError Btsdk_SetStatusInfoFlag(USHORT usMsgType);


        /* ---- Memory Management */
        //void *Btsdk_MallocMemory(BTUINT32 size);
        void Btsdk_FreeMemory(IntPtr memblock);

        /* ---- Local Bluetooth Device Initialization */
        BtSdkError Btsdk_StartBluetooth();
        BtSdkError Btsdk_StopBluetooth();
        /// <summary>
        /// The Btsdk_IsBluetoothReady function checks whether the local
        /// Bluetooth device is working.
        /// </summary>
        /// <returns>Boolean</returns>
        bool Btsdk_IsBluetoothReady();
        bool Btsdk_IsBluetoothHardwareExisted();

        /* ---- Local Device Mode */
        BtSdkError Btsdk_SetDiscoveryMode(StackConsts.DiscoveryMode mode);
        BtSdkError Btsdk_GetDiscoveryMode(out StackConsts.DiscoveryMode mode);

        /* ---- Local Device Information */
        BtSdkError Btsdk_GetLocalDeviceAddress(byte[] bd_addr);
        //BTINT32 Btsdk_SetLocalName(BTUINT8* name, BTUINT16 len);
        BtSdkError Btsdk_GetLocalName(byte[] name, ref UInt16 len);
        //BTINT32 Btsdk_SetLocalDeviceClass(BTUINT32 device_class);
        BtSdkError Btsdk_GetLocalDeviceClass(out UInt32 device_class);
        BtSdkError Btsdk_GetLocalLMPInfo(ref Structs.BtSdkLocalLMPInfoStru lmp_info);
        //BTINT32 Btsdk_SetFixedPinCode(BTUINT8 *pin_code, BTUINT16 size);
        //BTINT32 Btsdk_GetFixedPinCode(BTUINT8 *pin_code, BTUINT16 *psize);

        /* ---- Local Device Application Extension */
        //BTINT32 Btsdk_VendorCommand(BTUINT32 ev_flag, PBtSdkVendorCmdStru in_cmd, PBtSdkEventParamStru out_ev);
        //BTUINT32 Btsdk_EnumAVDriver();
        //void Btsdk_DeEnumAVDriver();
        //BTINT32 Btsdk_ActivateEx(const BTINT8 *pszSN, BTINT32 iSnlen);

        /* ---- Remote Device Discovery */
        BtSdkError Btsdk_StartDeviceDiscovery(UInt32 device_class, UInt16 max_num, UInt16 max_seconds);
        BtSdkError Btsdk_StopDeviceDiscovery();

        /// <summary>
        /// Gets the current user-friendly name of the specified remote device.
        /// </summary>
        /// -
        /// <remarks>
        /// Before calling Btsdk_UpdateRemoteDeviceName, the device database must be initialized by a
        /// previous successful call to Btsdk_StartBluetooth.
        /// The user-friendly device name is a UTF-8 character string. The device name acquired by this
        /// command is stored automatically in the device database.
        /// </remarks>
        BtSdkError Btsdk_UpdateRemoteDeviceName(BTDEVHDL dev_hdl, byte[] name, ref UInt16 plen);

        //FUNC_EXPORT BTINT32 Btsdk_CancelUpdateRemoteDeviceName(BTDEVHDL dev_hdl);

        /* ---- Device Pairing */
        BtSdkError Btsdk_IsDevicePaired(BTDEVHDL dev_hdl, out bool pis_paired);

        BtSdkError Btsdk_PairDevice(BTDEVHDL dev_hdl);
        //FUNC_EXPORT BTINT32 Btsdk_UnPairDevice(BTDEVHDL dev_hdl);
        BtSdkError Btsdk_PinCodeReply(BTDEVHDL dev_hdl, byte[] pin_code, UInt16 size);
        //FUNC_EXPORT BTUINT32 Btsdk_AuthorizationResponse(BTSVCHDL svc_hdl, BTDEVHDL dev_hdl, BTUINT16 author_result);

        /* ---- Callback Prototype */
        //typedef BTUINT8  (Btsdk_UserHandle_Pin_Req_Ind_Func)(BTDEVHDL dev_hdl);
        //typedef BTUINT8  (Btsdk_UserHandle_Authorization_Req_Ind_Func)(BTSVCHDL svc_hdl, BTDEVHDL dev_hdl);
        //typedef void  (Btsdk_Link_Key_Notif_Ind_Func)(BTDEVHDL dev_hdl, BTUINT8 *link_key);
        //typedef void  (Btsdk_Authentication_Fail_Ind_Func)(BTDEVHDL dev_hdl);

        /* ---- Link Management */
        bool Btsdk_IsDeviceConnected(BTDEVHDL dev_hdl);
        //FUNC_EXPORT BTINT32 Btsdk_GetRemoteDeviceRole(BTDEVHDL dev_hdl, BTUINT16 *prole);
        //FUNC_EXPORT BTINT32 Btsdk_GetRemoteLMPInfo(BTDEVHDL dev_hdl, BtSdkRemoteLMPInfoStru *info);

        /// <summary>
        /// "gets the RSSI value of the specified remote device."
        /// </summary>
        /// -
        /// <remarks>
        /// <para>
        /// </para>
        /// <para>"a connection between local device and the specified
        /// remote device must be created first."
        /// </para>
        /// </remarks>
        /// -
        /// <param name="device_handle">hDev
        /// </param>
        /// <param name="prssi">"Range: -128 to 127 (dB)."
        /// </param>
        BtSdkError Btsdk_GetRemoteRSSI(BTDEVHDL device_handle, out SByte prssi);

        /// <summary>
        /// "gets the current link quality value of the connection between local
        /// device and the specified remote device."
        /// </summary>
        /// -
        /// <remarks>"The higher the value, the better the link quality is."
        /// </remarks>
        /// -
        /// <returns>"Range: 0 to 0xFF."
        /// </returns>
        BtSdkError Btsdk_GetRemoteLinkQuality(BTDEVHDL device_handle, out UInt16 plink_quality);

        //FUNC_EXPORT BTINT32 Btsdk_GetSupervisionTimeout(BTDEVHDL dev_hdl, BTUINT16 *ptimeout);
        //FUNC_EXPORT BTINT32 Btsdk_SetSupervisionTimeout(BTDEVHDL dev_hdl, BTUINT16 timeout);
        //FUNC_EXPORT BTINT32 Btsdk_ChangeConnectionPacketType(BTDEVHDL dev_hdl, BTUINT16 packet_type);

        /* ---- Remote Device Database Management */
        BTDEVHDL Btsdk_GetRemoteDeviceHandle(byte[] bd_addr);
        BTDEVHDL Btsdk_AddRemoteDevice(byte[] bd_addr);
        BtSdkError Btsdk_DeleteRemoteDeviceByHandle(BTDEVHDL dev_hdl);
        //FUNC_EXPORT BTINT32 Btsdk_DeleteUnpairedDevicesByClass(BTUINT32 device_class);
        Int32 Btsdk_GetStoredDevicesByClass(UInt32 dev_class, BTDEVHDL[] pdev_hdl, Int32 max_dev_num);
        //FUNC_EXPORT BTUINT32 Btsdk_GetInquiredDevices(BTDEVHDL *pdev_hdl, BTUINT32 max_dev_num);
        //FUNC_EXPORT BTUINT32 Btsdk_GetPairedDevices(BTDEVHDL *pdev_hdl, BTUINT32 max_dev_num);
        //FUNC_EXPORT BTSDKHANDLE Btsdk_StartEnumRemoteDevice(BTUINT32 flag, BTUINT32 dev_class);
        //FUNC_EXPORT BTDEVHDL Btsdk_EnumRemoteDevice(BTSDKHANDLE enum_handle, PBtSdkRemoteDevicePropertyStru rmt_dev_prop);
        //FUNC_EXPORT BTINT32 Btsdk_EndEnumRemoteDevice(BTSDKHANDLE enum_handle);

        BtSdkError Btsdk_GetRemoteDeviceAddress(BTDEVHDL dev_hdl, byte[] bd_addr);

        /// <summary>
        /// "Gets the user-friendly name of the specified remote device from the device database."
        /// </summary>
        /// -
        /// <remarks>
        /// "Before calling Btsdk_GetRemoteDeviceName, the device database must be initialized by a
        /// previous successful call to Btsdk_Init.
        /// The user-friendly device name is a UTF-8 character string. The Btsdk_GetRemoteDeviceNamefunction returns =BTSDK_OPERATION_FAILURE immediately if the device name doesn’t
        /// exist in the database. In this case, the application shall call Btsdk_UpdateRemoteDeviceName
        /// to acquire the name information directly from the remote device.
        /// BlueSoleil will automatically update the device name when the local device connects to the
        /// specified remote device.
        /// </remarks>
        BtSdkError Btsdk_GetRemoteDeviceName(BTDEVHDL dev_hdl, byte[] name, ref UInt16 plen);

        BtSdkError Btsdk_GetRemoteDeviceClass(BTDEVHDL dev_hdl, out UInt32 pdevice_class);

        BtSdkError Btsdk_GetRemoteDeviceProperty(BTDEVHDL dev_hdl, out Structs.BtSdkRemoteDevicePropertyStru rmt_dev_prop);
        //FUNC_EXPORT BTINT32 Btsdk_RemoteDeviceFlowStatistic(BTDEVHDL dev_hdl, BTUINT32* rx_bytes, BTUINT32* tx_bytes);

        /* ---- Service Discovery */
        BtSdkError Btsdk_BrowseRemoteServicesEx(BTDEVHDL dev_hdl,
            Structs.BtSdkSDPSearchPatternStru[] psch_ptn, Int32 ptn_num,
            BTSVCHDL[] svc_hdl, ref Int32 svc_count);
        //BTINT32 Btsdk_BrowseRemoteServices(BTDEVHDL dev_hdl, BTSVCHDL *svc_hdl, BTUINT32 *svc_count);
        //BtSdkError Btsdk_RefreshRemoteServiceAttributes(BTSVCHDL svc_hdl,
        //    ref Structs.BtSdkRemoteServiceAttrStru attribute);
        //BTINT32 Btsdk_GetRemoteServicesEx(BTDEVHDL dev_hdl, PBtSdkSDPSearchPatternStru psch_ptn, BTUINT32 ptn_num, BTSVCHDL *svc_hdl, BTUINT32 *svc_count);
        //BTINT32 Btsdk_GetRemoteServices(BTDEVHDL dev_hdl, BTSVCHDL *svc_hdl, BTUINT32 *svc_count);
        BtSdkError Btsdk_GetRemoteServiceAttributes(BTSVCHDL svc_hdl, ref Structs.BtSdkRemoteServiceAttrStru attribute);
        //BTSDKHANDLE Btsdk_StartEnumRemoteService(BTDEVHDL dev_hdl);
        //BTSVCHDL Btsdk_EnumRemoteService(BTSDKHANDLE enum_handle, PBtSdkRemoteServiceAttrStru attribute);
        //BTINT32 Btsdk_EndEnumRemoteService(BTSDKHANDLE enum_handle);

        /* ---- Connection Management Application Extension */
        //BTINT32 Btsdk_SetRemoteServiceParam(BTSVCHDL svc_hdl, BTUINT32 app_param);
        //BTINT32 Btsdk_GetRemoteServiceParam(BTSVCHDL svc_hdl, BTUINT32 *papp_param);

        /* ---- Connection Establishment */
        //FUNC_EXPORT BTINT32 Btsdk_Connect(BTSVCHDL svc_hdl, BTUINT32 lParam, BTCONNHDL *conn_hdl);
        BtSdkError Btsdk_ConnectEx(BTDEVHDL dev_hdl, UInt16 service_class, UInt32 lParam, out BTCONNHDL conn_hdl);
        BtSdkError Btsdk_ConnectEx(BTDEVHDL dev_hdl, UInt16 service_class, ref Structs.BtSdkSPPConnParamStru lParam, out BTCONNHDL conn_hdl);

        /* ---- Connection Database Management */
        //FUNC_EXPORT BTINT32 Btsdk_GetConnectionProperty(BTCONNHDL conn_hdl, PBtSdkConnectionPropertyStru conn_prop);
        BTSDKHANDLE Btsdk_StartEnumConnection();
        BTCONNHDL Btsdk_EnumConnection(BTSDKHANDLE enum_handle, ref Structs.BtSdkConnectionPropertyStru conn_prop);
        BtSdkError Btsdk_EndEnumConnection(BTSDKHANDLE enum_handle);

        /* ---- Connection Release */
        BtSdkError Btsdk_Disconnect(BTCONNHDL handle);

        /* ---- BlueSoleil Extend APIs */
        //BTUINT32 Btsdk_VDIInstallDev(BTINT8 *HardwareID,  BTINT8 *COMName);
        //BTUINT32 Btsdk_VDIDelModem( BTINT8 *COMName);
        //BTUINT32 Btsdk_GetActivationInformation(BTINT8 *SerialNumber, BTINT8 *ActivateInformation, BTUINT32 ActiveInformationLen);
        //BTUINT32 Btsdk_EnterUnlockCode(BTINT8 *UnlockCode);

        /* 
            Serial Port Profile 
        */
        BtSdkError Btsdk_InitCommObj(byte com_idx, UInt16 svc_class);
        BtSdkError Btsdk_DeinitCommObj(BTUINT8 com_idx);
        BTINT16 Btsdk_GetClientPort(BTCONNHDL conn_hdl);
        //byte Btsdk_GetAvailableExtSPPCOMPort([MarshalAs(UnmanagedType.U1)] bool bIsForLocalSPPService);
        BtSdkError Btsdk_SearchAppExtSPPService(BTDEVHDL dev_hdl, ref Structs.BtSdkAppExtSPPAttrStru psvc);
        BtSdkError Btsdk_ConnectAppExtSPPService(BTDEVHDL dev_hdl,
            ref Structs.BtSdkAppExtSPPAttrStru psvc, out BTCONNHDL conn_hdl);

        BTUINT32 Btsdk_CommNumToSerialNum(int comportNum);
        void Btsdk_PlugOutVComm(UINT serialNum, StackConsts.COMM_SET flag);
        bool Btsdk_PlugInVComm(UInt32 serialNum, out UInt32 comportNumber,
            UInt32 usageType, StackConsts.COMM_SET flag, UInt32 dwTimeout);
        UInt32 Btsdk_GetASerialNum();

    }
}

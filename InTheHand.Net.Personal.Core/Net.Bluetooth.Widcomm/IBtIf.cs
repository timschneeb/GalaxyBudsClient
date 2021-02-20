// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2009 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2009 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal interface IBtIf
    {
        /// <summary>
        /// To get to HandleDeviceResponded, HandleInquiryCompleted etc
        /// </summary>
        void SetParent(WidcommBtInterface parent);
        void Create();
        void Destroy(bool disposing);

        //----
        bool StartInquiry();
        void StopInquiry();

        //----
        /// <summary>
        /// &#x201C;This function requests a service discovery for a specific device.&#x201D;
        /// </summary>
        /// -
        /// <remarks>
        /// <para>&#x201C;When the discovery is complete the derived function OnDiscoveryComplete() is called.&#x201D;
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address"></param>
        /// <param name="serviceGuid"></param>
        /// <returns>&#x201C;TRUE, if discovery has started; FALSE, if discovery has not started.&#x201D;</returns>
        bool StartDiscovery(BluetoothAddress address, Guid serviceGuid);

        /// <summary>
        /// &#x201C;When multiple discovery operations are in progress, the application 
        /// must call GetLastDiscoveryResult() from within the OnDiscoveryComplete() 
        /// to determine which remote devices reported services.&#x201D;
        /// </summary>
        /// <param name="address"></param>
        /// <param name="p_num_recs"></param>
        /// <returns>&#x201C;DISCOVERY_RESULT_SUCCESS, if the discovery operation was successful.&#x201D;</returns>
        DISCOVERY_RESULT GetLastDiscoveryResult(out BluetoothAddress address, out UInt16 p_num_recs);

        /// <summary>
        /// &#x201C;This function is called when discovery is complete to retrieve the records 
        /// received from the remote device.&#x201D;
        /// </summary>
        /// -
        /// <remarks>
        /// <para>&#x201C;Discovery results for a device are not removed until the device fails to respond to an inquiry.&#x201D;
        /// </para>
        /// </remarks>
        /// -
        /// <param name="address"></param>
        /// <param name="maxRecords"></param>
        /// <param name="args"></param>
        /// <returns>The discovery records read, which may have recordCount equals zero.</returns>
        ISdpDiscoveryRecordsBuffer ReadDiscoveryRecords(BluetoothAddress address, int maxRecords, ServiceDiscoveryParams args);

        //----------
        REM_DEV_INFO_RETURN_CODE GetRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb);
        REM_DEV_INFO_RETURN_CODE GetNextRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb);

        //----
        bool GetLocalDeviceVersionInfo(ref DEV_VER_INFO devVerInfo);
        bool GetLocalDeviceInfoBdAddr(byte[] bdAddr);
        bool GetLocalDeviceName(byte[] bdName);
        void IsStackUpAndRadioReady(out bool stackServerUp, out bool deviceReady);
        void IsDeviceConnectableDiscoverable(out bool conno, out bool disco);
        void SetDeviceConnectableDiscoverable(bool connectable, bool pairedOnly, bool discoverable);
        //
        int GetRssi(byte[] bd_addr);
        bool BondQuery(byte[] bd_addr);
        BOND_RETURN_CODE Bond(BluetoothAddress address, string passphrase);
        bool UnBond(BluetoothAddress address);
        //
        WBtRc GetExtendedError();
        //
        SDK_RETURN_CODE IsRemoteDevicePresent(byte[] bd_addr);
        bool IsRemoteDeviceConnected(byte[] bd_addr);
    }

}

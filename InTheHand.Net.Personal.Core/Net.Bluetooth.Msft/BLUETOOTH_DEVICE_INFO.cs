// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BLUETOOTH_DEVICE_INFO
// 
// Copyright (c) 2003-2008 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Runtime.InteropServices;
#if WinXP
using InTheHand.Win32;
using System.Text;
using System.Diagnostics;
#endif

namespace InTheHand.Net.Bluetooth.Msft
{

#if WinXP
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
#endif
    [CLSCompliant(false)]
    public struct BLUETOOTH_DEVICE_INFO
    {
        public int dwSize;
        public long Address;
        public uint ulClassofDevice;
#if WinXP
        [MarshalAs(UnmanagedType.Bool)]
#endif
        public bool fConnected;
#if WinXP
        [MarshalAs(UnmanagedType.Bool)]
#endif
        public bool fRemembered;
#if WinXP
        [MarshalAs(UnmanagedType.Bool)]
#endif
        public bool fAuthenticated;
#if WinXP
        public SYSTEMTIME stLastSeen;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
        public SYSTEMTIME stLastUsed;  
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=248)]
#endif
        public string szName;

        public BLUETOOTH_DEVICE_INFO(long address)
        {
            dwSize = 560;
            this.Address = address;
            ulClassofDevice = 0;
            fConnected = false;
            fRemembered = false;
            fAuthenticated = false;
#if WinXP
            stLastSeen = new SYSTEMTIME();
            stLastUsed = new SYSTEMTIME();

            // The size is much smaller on CE (no times and string not inline) it
            // appears to ignore the bad dwSize value.  So don't check this on CF.
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO)) == dwSize, "BLUETOOTH_DEVICE_INFO SizeOf == dwSize");
#endif
            szName = "";
        }

        public BLUETOOTH_DEVICE_INFO(BluetoothAddress address)
        {
            if (address == null) {
                throw new ArgumentNullException("address");
            }
            dwSize = 560;
            this.Address = address.ToInt64();
            ulClassofDevice = 0;
            fConnected = false;
            fRemembered = false;
            fAuthenticated = false;
#if WinXP
            stLastSeen = new SYSTEMTIME();
            stLastUsed = new SYSTEMTIME();

            // The size is much smaller on CE (no times and string not inline) it
            // appears to ignore the bad dwSize value.  So don't check this on CF.
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO)) == dwSize, "BLUETOOTH_DEVICE_INFO SizeOf == dwSize");
#endif
            szName = "";
        }

#if WinXP
        public DateTime LastSeen
        {
            get
            {
                return stLastSeen.ToDateTime(DateTimeKind.Utc);
            }
        }
        public DateTime LastUsed
        {
            get
            {
                return stLastUsed.ToDateTime(DateTimeKind.Utc);
            }
        }
#endif

        //--------
#if !NETCF
        public static BLUETOOTH_DEVICE_INFO Create(BTH_DEVICE_INFO deviceInfo)
        {
            Debug.Assert(0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Address),
                "BTH_DEVICE_INFO Address field flagged as empty!: " + deviceInfo.address.ToString("X12"));
            BLUETOOTH_DEVICE_INFO bdi0 = new BLUETOOTH_DEVICE_INFO(deviceInfo.address);
            //
            if (0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Cod)) {
                bdi0.ulClassofDevice = deviceInfo.classOfDevice;
            }
            byte[] nameUtf8 = deviceInfo.name;
            if (0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Name)) {
                int end = Array.IndexOf<byte>(nameUtf8, 0);
                if (end != -1) {
                    string name = Encoding.UTF8.GetString(nameUtf8, 0, end);
                    bdi0.szName = name;
                }
            }
            bdi0.fAuthenticated = 0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Paired);
            bdi0.fConnected = 0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Connected);
            bdi0.fRemembered = 0 != (deviceInfo.flags & BluetoothDeviceInfoProperties.Personal);
            //
            return bdi0;
        }
#endif

    }
}

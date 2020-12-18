// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BTH_DEVICE_INFO
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    //
    // The BTH_DEVICE_INFO structure stores information about a Bluetooth device.
    //
    [StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BTH_DEVICE_INFO
    {
        //
        // Combination BDIF_Xxx flags
        //
        public BluetoothDeviceInfoProperties flags;
        //
        // Address of remote device.
        //
        public long address;
        //
        // Class Of Device.
        //
        public uint classOfDevice;
        //
        // name of the device (As UTF8 String)
        //
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = NativeMethods.BTH_MAX_NAME_SIZE)]
        public byte[] name;
    }
}

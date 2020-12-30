// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if FAKE_ANDROID_BTH_API
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InTheHand.Net.Bluetooth.Droid
{
    public class AndroidMockValues
    {
        // -- Adapter --
        public string Radio_Address { get; set; }
        public string Radio_Name { get; set; }
        public Android.Bluetooth.State Radio_State { get; set; }
        public Android.Bluetooth.ScanMode Radio_ScanMode { get; set; }
        // Func GetRemoteDevice
        // -- Device1 --
        public string Device1_Address { get; set; }
        public string Device1_Name { get; set; }
        public Android.Bluetooth.Bond Device1_BondState { get; set; }
        // Func BluetoothClass
        // -- Class --
        public Android.Bluetooth.DeviceClass Device1_Class_DeviceClass { get; set; }
        public Android.Bluetooth.ServiceClass Device1_Class_ServiceClass { get; set; }
    }

}
#endif

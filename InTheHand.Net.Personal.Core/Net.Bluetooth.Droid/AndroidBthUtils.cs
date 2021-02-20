// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{
    static class AndroidBthUtils
    {
        //--
        #region Device Address
        [DebuggerStepThrough]
        internal static BluetoothAddress ToBluetoothAddress(string address_)
        {
            return BluetoothAddress.Parse(address_);
        }

        internal static string FromBluetoothAddress(BluetoothAddress address)
        {
            var str = address.ToString("C");
            Debug.Assert(str == str.ToUpperInvariant());
            Debug.Assert(BluetoothAdapter.CheckBluetoothAddress(str), "Failed CheckBluetoothAddress: '" + str + "'");
            return str;
        }
        #endregion

        #region Class of Device
        internal static ClassOfDevice ConvertCoDs(BluetoothClass codAndroid)
        {
            DeviceClass dc = (DeviceClass)(int)codAndroid.DeviceClass;
            ServiceClass sc = FindSetServiceBits(codAndroid);
            return new ClassOfDevice(dc, sc);
        }

        private static ServiceClass FindSetServiceBits(BluetoothClass codAndroid)
        {
            Assert(ServiceClass.Information, Android.Bluetooth.ServiceClass.Information);
            Assert(ServiceClass.LimitedDiscoverableMode, Android.Bluetooth.ServiceClass.LimitedDiscoverability);
#if false
            // For playing don't use FindSetServiceBits
            return ServiceClass.None;
#else
            ServiceClass sum = ServiceClass.None;
            for (var ian = (int)Android.Bluetooth.ServiceClass.LimitedDiscoverability;
                 ian <= (int)Android.Bluetooth.ServiceClass.Information;
                 ian <<= 1) {
                var san = (Android.Bluetooth.ServiceClass)ian;
                if (codAndroid.HasService(san)) {
                    var i = ConvertServiceClassAndroidTo32feet(ian);
                    var s = (ServiceClass)i;
                    sum |= s;
                }
            }//for
            return sum;
#endif
        }

        private static int ConvertServiceClassAndroidTo32feet(int iAndroid)
        {
            /*
             * 32feet
             *   LimitedDiscoverableMode = 0x0001,
             * Android 
             *   LimitedDiscoverability = 8192, = 0x2000
             * Shift = 3*4 + 1 = 13
             */
            const int ServiceClass_Offset = 13;
            return iAndroid >> ServiceClass_Offset;
        }

        private static void Assert(ServiceClass serviceClassWe, Android.Bluetooth.ServiceClass serviceClassAndroid)
        {
            var iwe = (int)serviceClassWe;
            var ian = (int)serviceClassAndroid;
            Debug.Assert(iwe == ConvertServiceClassAndroidTo32feet(ian),
                string.Format(CultureInfo.InvariantCulture,
                    "Not equal {0}=0x{1:X} and {2}=0x{3:X}",
                    serviceClassWe, iwe, serviceClassAndroid, ian));
        }
        #endregion

    }
}
#endif

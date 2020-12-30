// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.BluetoothDeviceInfo
// 
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Msft
{
    public static class MsftPlaying
    {
        public static int AuthDeviceExFakeOob(BluetoothAddress device, byte[] oobData)
        {
            var bdi = new BLUETOOTH_DEVICE_INFO(device.ToInt64());
            //
            IntPtr hwndParent = IntPtr.Zero;
            IntPtr hRadio = IntPtr.Zero;
            BluetoothAuthenticationRequirements authReq
                = BluetoothAuthenticationRequirements.MITMProtectionNotRequired;
            var result = NativeMethods.BluetoothAuthenticateDeviceEx(hwndParent,
                hRadio, ref bdi, oobData, authReq);
            if (result != 0) {
                //determine error cause from "result"...
                // ERROR_INVALID_PARAMETER      87
                // WAIT_TIMEOUT                258
                // ERROR_NOT_AUTHENTICATED    1244
                // E_FAIL               0x80004005

                //<--return false;
            }
            //<--return true;
            return result;
        }

        //=========================

        public static void AllIoctls()
        {
            var f = BluetoothFactory.GetTheFactoryOfTypeOrDefault<SocketsBluetoothFactory>();
            var r = f.DoGetPrimaryRadio();
            var handle = r.Handle;
            //
            const uint IOCTL_BTH__BASE = 0x00410000;
            const int Shift = 2;
            //const uint IOCTL_BTH_GET_LOCAL_INFO = 0x00410000;
            for (uint i = 0; ; ++i) {
                uint offs = i << Shift;
                if (offs == 256 << Shift) {
                    break;
                }
                uint ctl = IOCTL_BTH__BASE | offs;
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "i: {0,2}, ctl: 0x{1:X}.", i, ctl));
                TryAnIoctl(ctl, handle);
            }
        }

        static void TryAnIoctl(uint ctl, IntPtr handle)
        {
            var bufIn = new byte[300];
            var buf = new byte[300];
            int bytesReturned;
            var success = NativeMethods.DeviceIoControl(handle,
                ctl,
                bufIn, bufIn.Length,
                buf, buf.Length, out bytesReturned, IntPtr.Zero);
            if (!success) {
                int gle = Marshal.GetLastWin32Error();
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "failure: {0} = 0x{0:X}.", gle));
            } else {
                const int MaxPrintLen = 32;
                var tmpOut = new byte[Math.Min(MaxPrintLen, bytesReturned)];
                Array.Copy(buf, 0, tmpOut, 0, tmpOut.Length);
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "success: outLen: {0}, >>{1}{2}<<", bytesReturned,
                    BitConverter.ToString(tmpOut), bytesReturned > MaxPrintLen ? "..." : ""));
            }
        }

    }
}
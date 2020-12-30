// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    static partial class WidcommUtils
    {
        //
        internal static readonly byte[] SetSecurityLevel_Client_ServiceName
            = { (byte)'f', (byte)'a', (byte)'k', (byte)'e', (byte)'3', (byte)'2', (byte)'f', 0 };

        //--------------------------------------------------------------
        internal static BTM_SEC ToBTM_SEC(bool isServer, bool authenticate, bool encrypt)
        {
            BTM_SEC ret = BTM_SEC.NONE;
            // Note for authenticate=true we set AUTHENTICATE, and not also AUTHORIZE.
            if (!isServer) {
                if (encrypt) {
                    ret |= BTM_SEC.OUT_ENCRYPT;
                    // "BTM_SEC_OUT_AUTHENTICATE has to be set"
                    ret |= BTM_SEC.OUT_AUTHENTICATE;
                }
                if (authenticate) {
                    ret |= BTM_SEC.OUT_AUTHENTICATE;
                }
            } else {
                if (encrypt) {
                    ret |= BTM_SEC.IN_ENCRYPT;
                    // "BTM_SEC_IN_AUTHENTICATE has to be set.)"
                    ret |= BTM_SEC.IN_AUTHENTICATE;
                }
                if (authenticate) {
                    ret |= BTM_SEC.IN_AUTHENTICATE;
                }
            }
            Utils.MiscUtils.Trace_WriteLine("{0}; {1},{2}-> {3}", isServer, authenticate, encrypt, ret);
            return ret;
        }

        //--------------------------------------------------------------
        internal static BluetoothAddress ToBluetoothAddress(byte[] address_)
        {
            byte[] arr = (byte[])address_.Clone();
            Array.Reverse(arr);
            return new BluetoothAddress(arr);
        }

        internal static byte[] FromBluetoothAddress(BluetoothAddress address)
        {
            byte[] arr = address.ToByteArray();
            byte[] arr6 = new byte[6];
            Array.Copy(arr, arr6, arr6.Length);
            Array.Reverse(arr6);
            return arr6;
        }

        internal static string BdNameToString(byte[] deviceName)
        {
#if DEBUG
            if (deviceName.Length == WidcommStructs.BD_NAME_LEN) {
                ;
            } else {
                ;
            }
#endif
            //
            int len = Array.IndexOf<byte>(deviceName, 0);
            if (len == -1)
                len = deviceName.Length;
            return BdNameToString_(deviceName, len);
        }

        internal static string BdNameToString_(byte[] deviceName, int deviceNameLength)
        {
            if (deviceName != null && deviceName.Length != 0) {
                if (deviceNameLength < 0 || deviceNameLength > deviceName.Length)
                    throw new ArgumentOutOfRangeException("deviceNameLength");
                return Encoding.UTF8.GetString(deviceName, 0, deviceNameLength);
            }
            return null;
        }

        internal static ClassOfDevice ToClassOfDevice(byte[] devClass)
        {
            int tmp2 = ToInt32FromBigEndianUInt24(devClass);
            uint x = unchecked((UInt32)tmp2);
            return new ClassOfDevice(x);
        }

        private static int ToInt32FromBigEndianUInt24(byte[] devClass)
        {
            byte[] tmp = new byte[4]; // 32-bits
            devClass.CopyTo(tmp, 1);
            int tmp2 = BitConverter.ToInt32(tmp, 0);
            tmp2 = System.Net.IPAddress.NetworkToHostOrder(tmp2);
            return tmp2;
        }

        //--------------------------------------------------------------
        internal static string GetTime4Log()
        {
            return DateTime.Now.TimeOfDay.ToString();
        }

        //--------------------------------------------------------------
        internal static byte[] GetByteArray(IntPtr p, int count)
        {
            byte[] arr = new byte[count];
            for (int i = 0; i < count; ++i)
                arr[i] = System.Runtime.InteropServices.Marshal.ReadByte(p, i);
            return arr;
        }

        internal static byte[] GetByteArrayNullTerminated(IntPtr p, int maxCount)
        {
            int count = 0;
            for (int i = 0; i < maxCount; ++i) {
                if (0 == System.Runtime.InteropServices.Marshal.ReadByte(p, i)) {
                    count = i;
                    break;
                }
            }
            if (count == maxCount)
                throw new ArgumentException("String too long or not there at all?!");
            byte[] arr = new byte[count];
            for (int i = 0; i < count; ++i)
                arr[i] = System.Runtime.InteropServices.Marshal.ReadByte(p, i);
            return arr;
        }

        internal static void GetBluetoothCallbackValues(IntPtr bdAddr, IntPtr devClass, IntPtr deviceName, out byte[] bdAddrArr, out byte[] devClassArr, out byte[] deviceNameArr)
        {
            bdAddrArr = WidcommUtils.GetByteArray(bdAddr, 6);
            devClassArr = WidcommUtils.GetByteArray(devClass, 3);
            const int DoWeNeedToAddOneForTheNullTerminator_CheckFor248InTheBtSpec = 1;
            deviceNameArr = WidcommUtils.GetByteArrayNullTerminated(deviceName, 248 + DoWeNeedToAddOneForTheNullTerminator_CheckFor248InTheBtSpec);
        }

    }
}

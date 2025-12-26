// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    static class BluesoleilUtils
    {
        //--
        [DebuggerNonUserCode]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "descr")]
        internal static void CheckAndThrow(BtSdkError ret, string descr)
        {
            if (ret == BtSdkError.OK)
                return;
            SocketError? sockErr = null;
            switch (ret) {
                // TODO ! Match BlueSoleil error codes to SocketExceptions.
                // Note need different errors for GetServiceRecords, handle here or in GSR itself?
                case BtSdkError.PAGE_TIMEOUT:
                    sockErr = SocketError.TimedOut;
                    break;
                case BtSdkError.NO_SERVICE:
                    sockErr = SocketError.ConnectionRefused;
                    break;
                case BtSdkError.SDK_UNINIT:
                    sockErr = SocketError.NotInitialized;
                    break;
            }
            if (sockErr == null)
                sockErr = SocketError.NotSocket;
            throw new BlueSoleilSocketException(ret, sockErr.Value);
        }

        internal static Exception ErrorConnectIsNonRfcomm()
        {
            return new BlueSoleilSocketException(0, SocketError.MessageSize);
        }

        //----
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ret")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "descr")]
        internal static void Assert(BtSdkError ret, string descr)
        {
            Debug.Assert(ret == BtSdkError.OK,
                descr + ": " + BtSdkErrorToString(ret));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used in Debug at least.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "ret")]
        public static string BtSdkErrorToString(BtSdkError ret)
        {
            var txt = $"{ret}=0x{(int)ret:X4}";
            return txt;
        }


        //--
        internal static BluetoothAddress ToBluetoothAddress(byte[] address_)
        {
            byte[] arr = (byte[])address_.Clone();
            return new BluetoothAddress(arr);
        }

        internal static byte[] FromBluetoothAddress(BluetoothAddress address)
        {
            byte[] arr = address.ToByteArray();
            byte[] arr6 = new byte[6];
            Array.Copy(arr, arr6, arr6.Length);
            return arr6;
        }

        //--
        internal static string FromNameString(byte[] arr, UInt16? bufferLen)
        {
            int idx = Array.IndexOf<byte>(arr, 0);
            int len = idx != -1 ? idx : arr.Length;
            Debug.Assert(!bufferLen.HasValue || bufferLen.Value <= arr.Length,
                "bufferLen :" + bufferLen + ", arr.Length: " + arr.Length);
            if (bufferLen.HasValue && bufferLen.Value < len) {
                len = bufferLen.Value;
            }
            Debug.Assert(len <= arr.Length, "len: " + len + ", arr.Length: " + arr.Length);
            var txt = Encoding.UTF8.GetString(arr, 0, len);
            return txt;
        }

        internal static string FromNameString(byte[] arr)
        {
            return FromNameString(arr, null);
        }

        internal static Manufacturer FromManufName(ushort mn)
        {
            Manufacturer mf = (Manufacturer)mn;
            return mf;
        }


        internal static string FixedLengthArrayToStringUtf8(byte[] arr)
        {
            int len = Array.IndexOf<byte>(arr, 0);
            if (len == -1) len = arr.Length;
            string name = Encoding.UTF8.GetString(arr, 0, len);
            return name;
        }

    }
}

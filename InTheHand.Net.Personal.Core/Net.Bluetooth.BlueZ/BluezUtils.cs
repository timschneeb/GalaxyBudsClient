// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezUtils
// 
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.Sockets;
#if FX3_5
using System.Linq;
#endif
#if BlueZ
namespace InTheHand.Net.Bluetooth.BlueZ
{
    static class BluezUtils
    {
        internal static bool IsSuccess(BluezError ret)
        {
            return (ret >= 0);
        }

        [DebuggerNonUserCode]
        internal static void Assert(BluezError ret, string descr)
        {
            Debug.Assert(ret >= 0, "Bluez error: "
                + ret + "=(" + ((int)ret).ToString()
                + "), at: " + descr);
        }

        [DebuggerNonUserCode]
        internal static void CheckAndThrow(BluezError ret, string descr)
        {
            if (ret >= 0)
                return;
            Throw(ret, descr);
        }

        [DebuggerNonUserCode]
        internal static void Throw(BluezError ret0, string descr)
        {
            int ret = Marshal.GetLastWin32Error(); // Does work on Mono/Linux.
            int? sockErr = null;
            switch (ret) {
                // TODO ! Match BlueZ/Linux error codes to SocketExceptions.
                // Note need different errors for GetServiceRecords, handle here or in GSR itself?
                case 112:
                    sockErr = (int)SocketError.HostDown;
                    break;
            }
            if (sockErr == null)
                sockErr = SocketError_NotSocket;
            string stackTrace = "<disabled>";
            //-stackTrace = new StackTrace().ToString();
            var msg = "BluezUtils.Throw: '" + descr + "', ret: " + ret0 + " at: " + stackTrace;
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
            throw new SocketException(sockErr.Value);
        }

        const int SocketError_NotSocket = 10038;


        //--
        internal static BluetoothAddress ToBluetoothAddress(byte[] address_)
        {
            return BluetoothAddress.CreateFromLittleEndian(address_);
        }

        internal static byte[] FromBluetoothAddress(BluetoothAddress address)
        {
            var arrB = address.ToByteArrayLittleEndian();
            Debug.Assert(arrB.Length == 6, "BAD, arrB.Length is: " + arrB.Length);
            return arrB;
        }

        //--
        internal static string FromNameString(byte[] arr, int? bufferLen)
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

        internal static byte[] ToNameString(string name)
        {
            byte[] rawX = Encoding.UTF8.GetBytes(name);
            if (rawX.Length > 248) {
                throw new ArgumentException("Name is too long, must be 248 chars or less (actually 248 bytes in UTF8 encoding).");
            }
            var nullTd = new byte[rawX.Length + 1];
            rawX.CopyTo(nullTd, 0);
            if (nullTd.Length > 248 + 1) {
                throw new ArgumentException("Name is too long, must be 248 chars or less (actually 248 bytes in UTF8 encoding).");
            }
            return nullTd;
        }

        //----
        internal static ClassOfDevice ToClassOfDevice(byte[] p)
        {
            uint codI = (uint)p[0] + 256 * ((uint)p[1] + 256 * (uint)p[2]);
            var cod = new ClassOfDevice(codI);
            return cod;
        }

        //--------
        internal static IntPtr malloc(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        internal static void free(IntPtr p)
        {
            Marshal.FreeHGlobal(p);
        }

        internal static void close(int sock)
        {
            // TODO ! Mono.Unix.Native.Syscall.close(sock);
        }

        //----
        internal static IntPtr sdp_list_append<T>(IntPtr list, T val, List<IntPtr> listAllocs)
            where T : struct
        {
            if (val is IntPtr) throw new RankException("1");
            if (typeof(T) == typeof(IntPtr)) throw new RankException("2");
            IntPtr pVal = Malloc<T>(ref val, listAllocs);
            IntPtr pList = NativeMethods.sdp_list_append(list, pVal);
            if (pList == IntPtr.Zero)
                throw new InvalidOperationException("sdp_list_append(STRU)");
            //Console.WriteLine("sdp_list_append: in p: {0}, pList: {1}", pVal, pList);
            return pList;
        }

        internal static IntPtr Malloc<T>(ref T val, List<IntPtr> listAllocs) where T : struct
        {
            int size = Marshal.SizeOf(val);
            IntPtr p = BluezUtils.malloc(size);
            //Console.WriteLine("Malloc size: {0}, p: {1}", size, p);
            if (p == IntPtr.Zero)
                throw new InvalidOperationException("malloc");
            listAllocs.Add(p);
            Marshal.StructureToPtr(val, p, false);
            var data = Marshal.ReadInt64(p);
            //Console.WriteLine("Malloc data as u64: {0:X}", data);
            return p;
        }

        //----
        [Conditional("DEBUG")]
        internal static void DebugKeyExists<TValue>(IDictionary<string, TValue> dict,
            string key)
        {
            Debug.Assert(dict.ContainsKey(key), "Key '" + key + "' not exists");
            if (!dict.ContainsKey(key)) // Stupid Mono no Assert.
                Debug.WriteLine("Key '" + key + "' not exists.");
        }

        internal static void DumpKeys<TValue>(IDictionary<string, TValue> deviceDict)
        {
            var arr = new List<string>(deviceDict.Keys).ToArray();
            var s = string.Join(", ", arr);
            Debug.WriteLine("keys: " + s); ;
        }

        internal static void DumpKeys<TValue>(IDictionary<NDesk.DBus.ObjectPath, TValue> deviceDict)
        {
            var arr = new List<string>(
                deviceDict.Keys.Select(x => x.ToString())
                ).ToArray();
            var s = string.Join(", ", arr);
            Debug.WriteLine("keys: " + s); ;
        }


        internal static string FromBluetoothAddressToDbus(BluetoothAddress addr)
        {
            return addr.ToString("C");
        }

    }
}
#endif
// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.MicrosoftSdpService
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2003-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth;
using System.Runtime.InteropServices;

namespace InTheHand.Net.Bluetooth.Msft
{
    /// <exclude/>
    public static class MicrosoftSdpService
    {
        #region Service
        /// <exclude/>
        /// <summary>
        /// Remove a SDP record as added by <see cref="SetService"/>.
        /// </summary>
        /// <param name="handle">The handle.
        /// </param>
        /// <param name="sdpRecord">The raw record, presumably not actually used by the stack.
        /// </param>
        public static void RemoveService(IntPtr handle, byte[] sdpRecord)
        {
            BTHNS_SETBLOB blob = new BTHNS_SETBLOB(sdpRecord);
            blob.Handle = handle;

            WSAQUERYSET qs = new WSAQUERYSET();
            qs.dwSize = WqsOffset.StructLength_60;
            qs.dwNameSpace = WqsOffset.NsBth_16;
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(qs) == qs.dwSize, "WSAQUERYSET SizeOf == dwSize");

            GCHandle hBlob = GCHandle.Alloc(blob.ToByteArray(), GCHandleType.Pinned);

            BLOB b = new BLOB(blob.Length, GCHandleHelper.AddrOfPinnedObject(hBlob));

            GCHandle hb = GCHandle.Alloc(b, GCHandleType.Pinned);

            qs.lpBlob = hb.AddrOfPinnedObject();

            try {
                int hresult = NativeMethods.WSASetService(ref qs, WSAESETSERVICEOP.RNRSERVICE_DELETE, 0);
                SocketBluetoothClient.ThrowSocketExceptionForHR(hresult);
            } finally {
                hb.Free();
                hBlob.Free();

                //release blob and associated GCHandles
                blob.Dispose();
            }
        }

        /// <exclude/>
        /// <summary>
        /// Add a SDP record.
        /// </summary>
        /// -
        /// <param name="sdpRecord">An array of <see cref="T:System.Byte"/>
        /// containing the complete SDP record.
        /// </param>
        /// <param name="cod">A <see cref="T:InTheHand.Net.Bluetooth.ServiceClass"/>
        /// containing any bits to set in the devices Class of Device value.
        /// </param>
        /// -
        /// <returns>A handle representing the record, pass to 
        /// <see cref="RemoveService"/> to remote the record.
        /// </returns>
        public static IntPtr SetService(byte[] sdpRecord, ServiceClass cod)
        {
            BTHNS_SETBLOB blob = new BTHNS_SETBLOB(sdpRecord);
            //added for XP - adds class of device bits
#if WinXP
            blob.CodService = (uint)cod;
#endif

            WSAQUERYSET qs = new WSAQUERYSET();
            qs.dwSize = WqsOffset.StructLength_60;
            qs.dwNameSpace = WqsOffset.NsBth_16;
            System.Diagnostics.Debug.Assert(Marshal.SizeOf(qs) == qs.dwSize, "WSAQUERYSET SizeOf == dwSize");
            GCHandle hBlob = GCHandle.Alloc(blob.ToByteArray(), GCHandleType.Pinned);

            BLOB b = new BLOB(blob.Length, GCHandleHelper.AddrOfPinnedObject(hBlob));

            GCHandle hb = GCHandle.Alloc(b, GCHandleType.Pinned);

            qs.lpBlob = hb.AddrOfPinnedObject();

            try {
                int hresult = NativeMethods.WSASetService(ref qs, WSAESETSERVICEOP.RNRSERVICE_REGISTER, 0);
                SocketBluetoothClient.ThrowSocketExceptionForHR(hresult);
            } finally {
                hb.Free();
                hBlob.Free();
            }

            IntPtr handle = blob.Handle;
            blob.Dispose();

            return handle;
        }
        #endregion
    }
}

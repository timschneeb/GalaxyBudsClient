// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezListener
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

#if BlueZ

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezListener : InTheHand.Net.Bluetooth.Msft.WindowsBluetoothListener
    {
        //readonly BluezFactory _fcty;
        NativeMethods.SdpSessionSafeHandle _sdpSession;

        //----
        internal BluezListener(BluezFactory fcty)
            : base(fcty)
        {
            Debug.Assert(fcty != null, "ArgNull");
            //_fcty = fcty;
        }

        protected override AddressFamily BluetoothAddressFamily
        {
            get { return AddressFamily32.BluetoothOnLinuxBlueZ; }
        }

        protected override ISocketOptionHelper CreateSocketOptionHelper(Socket socket)
        {
            return new BluezSocketOptionHelper(socket);
        }

        //----
        protected override BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP_)
        {
            Console.WriteLine("Calling BluezRfcommEndPoint.CreateBindEndPoint");
            var bindEP = BluezRfcommEndPoint.CreateBindEndPoint(serverEP_);
            // Win32 uses -1 for 'auto assign' but BlueZ uses 0.
            if (serverEP_.Port == -1) {
                serverEP_.Port = 0;
            } else if (serverEP_.Port > BluetoothEndPoint.MaxScn) {
                // BlueZ doesn't complain in this case!  Do't know what it does...
                throw new SocketException((int)SocketError.AddressNotAvailable);
            }
            Debug.Assert(bindEP.Address == serverEP_.Address, "Address DIFF");
            Debug.Assert(bindEP.Port == serverEP_.Port, "Port DIFF");
            return bindEP;
        }

        protected override void SetService(byte[] sdpRecord, ServiceClass cod)
        {
            int used;
            IntPtr rec = NativeMethods.sdp_extract_pdu(sdpRecord, sdpRecord.Length, out used);
            if (used != sdpRecord.Length)
                throw new InvalidOperationException(
                    $"sdp_extract_pdu had {sdpRecord.Length} but only used {used}");
            _sdpSession = AddRecord(rec);
            //???NativeMethods.sdp_free_record(pRec);
            Console.WriteLine("Done SetService");
        }

        protected override void RemoveService()
        {
            Console.WriteLine("Gonna _sdpSession.Close()...");
            _sdpSession.Close();
        }

        internal static NativeMethods.SdpSessionSafeHandle AddRecord(IntPtr rec)
        {
            //Console.Write("Hit return to AddRecord> ");
            //Console.ReadLine();
            //
            BluezError ret;
            var session = NativeMethods.sdp_connect(
                StackConsts.BDADDR_ANY, StackConsts.BDADDR_LOCAL,
                StackConsts.SdpConnectFlags.SDP_RETRY_IF_BUSY);
            //
            ret = NativeMethods.sdp_record_register(session, rec, 0);
            BluezUtils.CheckAndThrow(ret, "sdp_record_register");

            return session;
        }

    }
}
#endif
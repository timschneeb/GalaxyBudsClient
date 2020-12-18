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
using System.Net;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezL2CapListener : InTheHand.Net.Bluetooth.Msft.WindowsBluetoothListener
    {
        readonly BluezFactory _fcty;
        NativeMethods.SdpSessionSafeHandle _sdpSession;

        //----
        internal BluezL2CapListener(BluezFactory fcty)
            : base(fcty)
        {
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
        }

        protected override AddressFamily BluetoothAddressFamily
        {
            get { return AddressFamily32.BluetoothOnLinuxBlueZ; }
        }

        protected override Socket CreateSocket()
        {
            const ProtocolType BTPROTO_L2CAP = 0;
            return new Socket(BluetoothAddressFamily, SocketType.Seqpacket, BTPROTO_L2CAP);
        }

        protected override ISocketOptionHelper CreateSocketOptionHelper(Socket socket)
        {
            return new BluezSocketOptionHelper(socket);
        }

        protected override InTheHand.Net.Bluetooth.Factory.IBluetoothClient MakeIBluetoothClient(Socket s)
        {
            return new BluezL2CapClient(_fcty, s);
        }

        //----
        protected override BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP)
        {
            var l2capEp = BluezL2capEndPoint.CreateBindEndPoint(serverEP);
            return l2capEp;
        }

        protected override void SetService(byte[] sdpRecord, ServiceClass cod)
        {
            // TODO Change base Listener to add an L2CAP record!!!!!!!!!!!!!
            int used;
            IntPtr rec = NativeMethods.sdp_extract_pdu(sdpRecord, sdpRecord.Length, out used);
            if (used != sdpRecord.Length)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "sdp_extract_pdu had {0} but only used {1}", sdpRecord.Length, used));
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
// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezClient
// 
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

#if BlueZ

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Bluetooth.Msft;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezClient : SocketBluetoothClient, IUsesBluetoothConnectorImplementsServiceLookup
    {
#if HACK_FAKE_SOCKET_TO_ALLOW_DISCOVERDEVICES_TEST
        protected override Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified);
        }
#endif

        readonly BluezFactory _fcty;
        BluezSdpQuery _sdpQuery;
        SocketConnectProcess _conn;

        internal BluezClient(BluezFactory fcty)
            : base(fcty)
        {
            _fcty = fcty;
        }

        internal BluezClient(BluezFactory fcty, BluetoothEndPoint localEP)
            : base(fcty, localEP)
        {
            _fcty = fcty;
            //TODO _radio = _fcty.GetAdapterWithAddress(localEP.DeviceAddress);
        }

        internal BluezClient(BluezFactory fcty, System.Net.Sockets.Socket acceptedSocket)
            : base(fcty, acceptedSocket)
        {
            _fcty = fcty;
        }

        protected override AddressFamily BluetoothAddressFamily
        {
            get { return AddressFamily32.BluetoothOnLinuxBlueZ; }
        }

        protected override ISocketOptionHelper CreateSocketOptionHelper(Socket socket)
        {
            return new BluezSocketOptionHelper(socket);
        }

        protected override BluetoothEndPoint PrepareConnectEndPoint(BluetoothEndPoint serverEP)
        {
            throw new NotImplementedException();
        }

        protected override BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP)
        {
            Console.WriteLine("Calling BluezRfcommEndPoint.CreateBindEndPoint");
            return BluezRfcommEndPoint.CreateBindEndPoint(serverEP);
        }

        //----
        public override IBluetoothDeviceInfo[] DiscoverDevices(
            int maxDevices, bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            Console.WriteLine("DiscoverDevices");
            BluezDbus bus = null;
            Action<IBluetoothDeviceInfo> removeDlgt = null;
            if (liveDiscoHandler != null || liveDiscoState != null) {
                Console.WriteLine("Gonna AddInquiryEvents 2");
                bus = _fcty.BluezDbus;
                Action<IBluetoothDeviceInfo> dlgt = delegate(IBluetoothDeviceInfo bdi) {
                    liveDiscoHandler(bdi, liveDiscoState);
                };
                bus.LiveDisco += dlgt;
                removeDlgt = dlgt;
            }
            try {
                return DoDiscoverDevices(
                    maxDevices, authenticated, remembered, unknown, discoverableOnly,
                    liveDiscoHandler, liveDiscoState);
            } finally {
                if (removeDlgt != null) {
                    bus.LiveDisco -= removeDlgt;
                }
            }
        }

        IBluetoothDeviceInfo[] DoDiscoverDevices(
            int maxDevices, bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            Console.WriteLine("DoDiscoverDevices");
            var discoTime = DateTime.UtcNow;
            List<IBluetoothDeviceInfo> known = new List<IBluetoothDeviceInfo>(); // hack often GC fodder
            var bus = _fcty.BluezDbus;
            known = bus.GetDeviceList_OnDefaultAdapter();
            //
            List<IBluetoothDeviceInfo> inquired = null;
            if (discoverableOnly || unknown) {
                inquired = new List<IBluetoothDeviceInfo>();
                double td = InquiryLength.TotalSeconds;
                const double Multiplier = 1.25d;
                td /= Multiplier;
                int t = (int)td;
                StackConsts.IREQ_int flags = StackConsts.IREQ_int.IREQ_CACHE_FLUSH;
                //
                var TypeofItem = typeof(Structs.inquiry_info);
                var SizeofItem = Marshal.SizeOf(TypeofItem);
                IntPtr pii = BluezUtils.malloc(maxDevices * SizeofItem);
                try {
                    Console.WriteLine("Gonna hci_inquiry num_rsp: {0}, t: {1} ({2} was {3}) ", maxDevices, t, td, InquiryLength);
                    // TO-DO LAP/IAC: var lap = InquiryAccessCode;
                    //var stackTrace = new StackTrace();
                    //var msg = "Gonna hci_inquiry at: " + stackTrace;
                    //Debug.WriteLine(msg);
                    //Console.WriteLine(msg);
                    int num = NativeMethods.hci_inquiry(_fcty.DevId, t, maxDevices, IntPtr.Zero, ref pii, flags);
                    Console.WriteLine("inquiry num=" + num);
                    //BluezUtils.CheckAndThrow((BluezError)num, "hci_inquiry");
                    BluezUtils.Assert((BluezError)num, "hci_inquiry");
                    //
                    IntPtr pCur = pii;
                    for (int i = 0; i < num; i++) {
                        var cur = (Structs.inquiry_info)Marshal.PtrToStructure(pCur, TypeofItem);
                        var bdi = BluezDeviceInfo.CreateFromInquiry(_fcty, cur);
                        inquired.Add(bdi);
                        pCur = PointerAdd(pCur, SizeofItem);
                    }//for
                } finally {
                    BluezUtils.free(pii);
                }
            }
            //
            var merged = BluetoothClient.DiscoverDevicesMerge(
                authenticated, remembered, unknown,
                known, inquired, discoverableOnly, discoTime);
            return merged.ToArray();
        }

        //--------

        public override void Connect(BluetoothEndPoint remoteEP)
        {
            EndConnect(BeginConnect(remoteEP, null, null));
        }

        public override IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            if (_conn == null) {
                _conn = new SocketConnectProcess(this, this.Client);
            }
            return _conn.BeginConnect(remoteEP, requestCallback, state);
        }

        public override void EndConnect(IAsyncResult asyncResult)
        {
            if (_conn == null) {
                throw new InvalidOperationException("BeginConnect must be called first.");
            }
            _conn.EndConnect(asyncResult);
        }


        class SocketConnectProcess : BluetoothConnector
        {
            readonly Socket m_sock;

            //----
            internal SocketConnectProcess(IUsesBluetoothConnectorImplementsServiceLookup parent, Socket sock)
                : base(parent)
            {
                m_sock = sock;
            }

            //----
            protected BluetoothEndPoint PrepareConnectEndPoint(BluetoothEndPoint serverEP)
            {
                Console.WriteLine("Calling BluezRfcommEndPoint.CreateBindEndPoint");
                return BluezRfcommEndPoint.CreateConnectEndPoint(serverEP);
            }

            //----
            protected override IAsyncResult ConnBeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
            {
                var connEP = PrepareConnectEndPoint(remoteEP);
                return m_sock.BeginConnect(connEP, requestCallback, state);
            }

            protected override void ConnEndConnect(IAsyncResult asyncResult)
            {
                m_sock.EndConnect(asyncResult);
            }
        }

        //--
        IAsyncResult IUsesBluetoothConnectorImplementsServiceLookup.BeginServiceDiscovery(
            BluetoothAddress address, Guid serviceGuid,
            AsyncCallback asyncCallback, Object state)
        {
            if (_sdpQuery == null) {
                _sdpQuery = new BluezSdpQuery(_fcty);
            }
            return _sdpQuery.BeginQuery(address, serviceGuid, true, asyncCallback, state);
        }

        List<int> IUsesBluetoothConnectorImplementsServiceLookup.EndServiceDiscovery(IAsyncResult ar)
        {
            if (_sdpQuery == null) {
                throw new InvalidOperationException("Begin not called.");
            }
            var records = _sdpQuery.EndQuery(ar);
            var ports = BluetoothConnector.ListAllRfcommPortsInRecords(records);
            return ports;
        }

        //--------------------------------------------------------------
        private static IntPtr PointerAdd(IntPtr x, int y)
        {
            var pi = x.ToInt64();
            pi += y;
            var p = new IntPtr(pi);
            return p;
        }

    }
}
#endif
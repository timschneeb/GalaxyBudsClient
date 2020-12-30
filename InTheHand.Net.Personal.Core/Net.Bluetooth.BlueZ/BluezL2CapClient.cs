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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Bluetooth.Msft;
using System.Diagnostics;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezL2CapClient : SocketBluetoothClient, IUsesBluetoothConnectorImplementsServiceLookup,
        IL2CapClient
    {
        readonly BluezFactory _fcty;
        BluezSdpQuery _sdpQuery;
        SocketConnectProcess _conn;

        internal BluezL2CapClient(BluezFactory fcty)
            : base(fcty)
        {
            _fcty = fcty;
        }

        internal BluezL2CapClient(BluezFactory fcty, BluetoothEndPoint localEP)
            : base(fcty, localEP)
        {
            _fcty = fcty;
        }

        internal BluezL2CapClient(BluezFactory fcty, System.Net.Sockets.Socket acceptedSocket)
            : base(fcty, acceptedSocket)
        {
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

        protected override BluetoothEndPoint PrepareConnectEndPoint(BluetoothEndPoint serverEP)
        {
            throw new NotImplementedException();
        }

        protected override BluetoothEndPoint PrepareBindEndPoint(BluetoothEndPoint serverEP)
        {
            Console.WriteLine("Calling BluezL2capEndPoint.CreateBindEndPoint");
            return BluezL2capEndPoint.CreateBindEndPoint(serverEP);
        }

        //----
        public override IBluetoothDeviceInfo[] DiscoverDevices(
            int maxDevices, bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState)
        {
            throw new NotImplementedException();
        }

        //--------
        // TODO BlueZL2CapClient.Mtu
        // -- bluetooth.h --
        //#define SOL_L2CAP	6
        // -- l2cap.h --
        // /* L2CAP socket options */
        //#define L2CAP_OPTIONS	0x01
        //struct l2cap_options {
        //    uint16_t	omtu;
        //    uint16_t	imtu;
        //    uint16_t	flush_to;
        //    uint8_t		mode;
        //    uint8_t		fcs;
        //    uint8_t		max_tx;
        //    uint16_t	txwin_size;
        //};

        public int GetMtu()
        {
            const SocketOptionLevel SOL_L2CAP = (SocketOptionLevel)6;
            const SocketOptionName L2CAP_OPTIONS = (SocketOptionName)0x01;
            const int l2cap_optionsLen = 2 + 2 + 2 + 1 + 1 + 1 + (1) + 2;
            //
            var buf = new byte[l2cap_optionsLen];
            Client.GetSocketOption(SOL_L2CAP, L2CAP_OPTIONS, buf);
            var omtu = BitConverter.ToUInt16(buf, 0);
            var imtu = BitConverter.ToUInt16(buf, 2);
            return omtu;
        }

        //--------
        protected override NetworkStream MakeStream(Socket sock)
        {
            return new L2CapNetworkStream(Client, true);
        }

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
                Console.WriteLine("Calling BluezL2capEndPoint.CreateConnectEndPoint");
                return BluezL2capEndPoint.CreateConnectEndPoint(serverEP);
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

    internal class L2CapNetworkStream : NetworkStream
    {
        internal L2CapNetworkStream(Socket l2capSeqPacketSocket, bool ownsSocket)
            : base(DecoratorNetworkStream.GetAConnectedSocket(), ownsSocket)
        {
            // Get around the check of SOCK_STREAM in NetworkStream constructor!
            // Pass a fake Socket(SOCK_STREAM) into the constructor and then
            // reset the internal variable to the Socket(SOCK_SEQPACKET) 
            typeof(NetworkStream).InvokeMember("socket",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.FlattenHierarchy
                | System.Reflection.BindingFlags.SetField, null,
                this, new object[] { l2capSeqPacketSocket });
        }
    }
}
#endif

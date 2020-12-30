// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{
    //#if DEBUG
    //    public
    //#endif
    class AndroidBthClient : CommonDiscoveryBluetoothClient
    {
        readonly AndroidBthFactoryBase _fcty;
        BluetoothSocket _sock;
        bool _auth, _encr;
        Stream _strm0;
        NetworkStream _strm;
#if DEBUG
        Stream _TEST_InputStream, _TEST_OutputStream;
#endif
        object _lock = new object();
        AndroidBthInquiry _inquiry;

        internal AndroidBthClient(AndroidBthFactoryBase fcty)
        {
            _fcty = fcty;
        }

        internal static IBluetoothClient CreateFromListener(AndroidBthFactoryBase fcty, BluetoothSocket sock)
        {
            var cli = new AndroidBthClient(fcty);
            cli.SetupConnection(sock);
            return cli;
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) {
                    if (_strm0 != null) {
                        _strm0.Close();
                    }
                    if (_inquiry != null) {
                        _inquiry.Dispose();
                    }
                }
            } finally {
                base.Dispose(disposing);
            }
        }

        #endregion

#if DEBUG
        internal void GetObjectsForTest(out BluetoothSocket sock, out Stream @in, out Stream @out)
        {
            sock = _sock;
            @in = _TEST_InputStream;
            @out = _TEST_OutputStream;
        }
#endif

        #region IBluetoothClient Members

        public override void Connect(BluetoothEndPoint remoteEP)
        {
            // TESTING
            if (!TestUtilities.IsUnderTestHarness()) {
                _fcty.ToJavaUuid(BluetoothService.ObexObjectPush);
                _fcty.ToJavaUuid(Guid.NewGuid());
            }
            //--
            var cloneEP = (BluetoothEndPoint)remoteEP.Clone();
            var dev = (AndroidBthDeviceInfo)_fcty.DoGetBluetoothDeviceInfoInternalOnly(remoteEP.Address);
            var sock = dev.CreateSocket(cloneEP, _auth, _encr);
            sock.Connect();
            SetupConnection(sock);
        }

        public override IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            var ar = new AsyncResultNoResult(requestCallback, state);
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try {
                    Connect(remoteEP);
                    ar.SetAsCompleted(null, AsyncResultCompletion.IsAsync);
                } catch (Exception ex) {
                    ar.SetAsCompleted(ex, AsyncResultCompletion.IsAsync);
                }
            });
            return ar;
        }

        public override void EndConnect(IAsyncResult asyncResult)
        {
            var ar = (AsyncResultNoResult)asyncResult;
            ar.EndInvoke();
        }

        #region AndroidNetworkStream

        class AndroidNetworkStream : DecoratorNetworkStream
        {
            readonly Stream _child;

            internal AndroidNetworkStream(Stream child)
                : base(child)
            {
                _child = child;
            }

            public override bool DataAvailable
            {
                get
                {
                    //bool x = child.IsDataAvailable(); // extension-method
                    throw new NotImplementedException();
                }
            }
        }

        #endregion

        void SetupConnection(BluetoothSocket sock)
        {
            _sock = sock;
            _strm0 = new AndroidBthSocketStream(_sock);
#if DEBUG
            _TEST_InputStream = _sock.InputStream;
            _TEST_OutputStream = _sock.OutputStream;
#endif
        }

        public Stream GetStream2()
        {
            if (_strm0 == null) {
                throw new InvalidOperationException("Not connected.");
            }
            return _strm0;
        }

        public override NetworkStream GetStream()
        {
            if (_strm == null) {
                _strm = new AndroidNetworkStream(GetStream2());
            }
            return _strm;
        }

        bool? ConnectedInternal
        {
            get
            {
                if (_sock == null) return false;
#if ANDROID_API_LEVEL_14
                return _sock.IsConnected;
#else
                return null;
#endif
            }
        }

        private void CheckNotDoneConnect()
        {
            if (_sock != null) throw new InvalidOperationException("Connect has been called.");
        }

        public override bool Connected
        {
            get
            {
                var conn = ConnectedInternal;
                if (conn.HasValue) {
                    return conn.Value;
                } else {
                    throw new NotSupportedException("Need library compiled for Android level 14.");
                }
            }
        }

        public override Socket Client
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Available
        {
            get { throw new NotImplementedException(); }
        }

        //----
        public override bool Authenticate
        {
            get { return _auth; }
            set
            {
                CheckNotDoneConnect();
                _auth = value;
            }
        }

        public override bool Encrypt
        {
            get { return _encr; }
            set
            {
                CheckNotDoneConnect();
                _encr = value;
            }
        }

        //----
        public override BluetoothEndPoint RemoteEndPoint
        {
            get
            {
                if (ConnectedInternal == false) throw new InvalidOperationException("Not connected.");
                Debug.Assert(_sock != null);
                var addr = AndroidBthUtils.ToBluetoothAddress(
                    _sock.RemoteDevice.Address);
                return new BluetoothEndPoint(addr, BluetoothService.Empty);
            }
        }

        public override string RemoteMachineName
        {
            get
            {
                if (ConnectedInternal == false) throw new InvalidOperationException("Not connected.");
                Debug.Assert(_sock != null);
                return _sock.RemoteDevice.Name;
            }
        }

        public override string GetRemoteMachineName(BluetoothAddress a)
        {
            throw new NotImplementedException();
        }

        //----
        //IAsyncResult IBluetoothClient.BeginDiscoverDevices(int maxDevices, bool authenticated, bool remembered, bool unknown, bool discoverableOnly, AsyncCallback callback, object state)
        protected override void BeginInquiry(int maxDevices, AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler,
            object liveDiscoState, DiscoDevsParams args)
        {
            AndroidBthInquiry inq;
            lock (_lock) {
                if (_inquiry == null) {
                    _inquiry = _fcty.GetInquiry();
                }
                inq = _inquiry;
            }
            inq.DoBeginInquiry(maxDevices, InquiryLength,
                callback, state,
                liveDiscoHandler, liveDiscoState,
                args);
        }

        //IBluetoothDeviceInfo[] IBluetoothClient.EndDiscoverDevices(IAsyncResult asyncResult)
        protected override List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            return _inquiry.EndInquiry(ar);
        }

        protected override List<IBluetoothDeviceInfo> GetKnownRemoteDeviceEntries()
        {
            // "If Bluetooth state is not STATE_ON, this API will return an empty set."
            // "Returns: unmodifiable set of BluetoothDevice, or null on error"
            var knownDevs = _fcty.GetAdapter().BondedDevices;
            if (knownDevs == null)
                throw new InvalidOperationException("Failed to get the known devices.");
            var known = from x in knownDevs
                        select (IBluetoothDeviceInfo)
                            AndroidBthDeviceInfo.CreateFromBondedList(_fcty, x);
            return known.ToList();
        }

        //----
        public override LingerOption LingerState
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Guid LinkKey
        {
            get { throw new NotImplementedException(); }
        }

        public override LinkPolicy LinkPolicy
        {
            get { throw new NotImplementedException(); }
        }

        public override void SetPin(string pin)
        {
            throw new NotImplementedException();
        }

        public override void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

}
#endif

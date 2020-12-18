// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{
    sealed class AndroidBthListener : IBluetoothListener
    {
        readonly AndroidBthFactoryBase _fcty;
        Guid _svcClassOrig;
        Java.Util.UUID _svcClass;
        string _svcName;
        bool _auth, _encr;
        //
        bool _active;
        internal BluetoothServerSocket _server;

        internal AndroidBthListener(AndroidBthFactoryBase fcty)
        {
            _fcty = fcty;
        }

        #region IBluetoothListener Members

        void IBluetoothListener.Construct(Guid service)
        {
            _svcClassOrig = service;
            _svcClass = _fcty.ToJavaUuid(service);
        }

        void IBluetoothListener.Construct(BluetoothAddress localAddress, Guid service)
        {
            ((IBluetoothListener)this).Construct(service);
        }

        void IBluetoothListener.Construct(BluetoothEndPoint localEP)
        {
            if (localEP.HasPort) {
                // Is it possible to implement?
                throw new NotImplementedException("Listen on Port not implemented.");
            }
            ((IBluetoothListener)this).Construct(localEP.Service);
        }

        void IBluetoothListener.Construct(Guid service, byte[] sdpRecord, int channelOffset)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        void IBluetoothListener.Construct(BluetoothAddress localAddress, Guid service, byte[] sdpRecord, int channelOffset)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        void IBluetoothListener.Construct(BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        void IBluetoothListener.Construct(Guid service, ServiceRecord sdpRecord)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        void IBluetoothListener.Construct(BluetoothAddress localAddress, Guid service, ServiceRecord sdpRecord)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        void IBluetoothListener.Construct(BluetoothEndPoint localEP, ServiceRecord sdpRecord)
        {
            throw new NotSupportedException("No support for creating Service Records on Android.");
        }

        //--
        void IBluetoothListener.Start(int backlog)
        {
            Start();
        }

        public void Start()
        {
            var svcName = ServiceName;
            Debug.Assert(_svcClassOrig != null, "NULL _svcClassOrig");
            Debug.Assert(_svcClass != null, "NULL _svcClass");
            var uuid = _svcClass;
            var a = _fcty.GetAdapter();
            if (_auth || _encr) {
                _server = a.ListenUsingRfcommWithServiceRecord(svcName, uuid);
            } else {
                _server = a.ListenUsingInsecureRfcommWithServiceRecord(svcName, uuid);
            }
            _server.ToString(); // ensure non-null
            _active = true;
        }

        public void Stop()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AndroidBthListener()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_active) {
                var svr = _server;
                _active = false;
                svr.Close();
            }
        }

        private void CheckNotListening()
        {
            if (_active) throw new InvalidOperationException("Listener is active.");
        }

        //--
        public string ServiceName
        {
            get { return _svcName; }
            set
            {
                CheckNotListening();
                _svcName = value;
            }
        }

        ServiceRecord IBluetoothListener.ServiceRecord
        {
            get { return null; }
        }

        public ServiceClass ServiceClass
        {
            get { return ServiceClass.None; }
            set { throw new NotSupportedException("Setting Service Class not supported on Android."); }
        }

        //--
        private BluetoothSocket Accept()
        {
            if (!_active)
                throw new InvalidOperationException("Listener is not Start-ed.");
            // TO-DO Want to convert any exception types here?
            var sock = _server.Accept();
            sock.ToString();
            return sock;
        }

        private IBluetoothClient Wrap(BluetoothSocket sock)
        {
            var cli = _fcty.DoGetBluetoothClientForListener(sock);
            return cli;
        }

        IBluetoothClient IBluetoothListener.AcceptBluetoothClient()
        {
            return Wrap(Accept());
        }

        IAsyncResult IBluetoothListener.BeginAcceptBluetoothClient(AsyncCallback callback, object state)
        {
            var ar = new AsyncResult<BluetoothSocket>(callback, state);
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                ar.SetAsCompletedWithResultOf(() => Accept(), AsyncResultCompletion.IsAsync);
            });
            return ar;
        }

        IBluetoothClient IBluetoothListener.EndAcceptBluetoothClient(IAsyncResult asyncResult)
        {
            var ar = (AsyncResult<BluetoothSocket>)asyncResult;
            return Wrap(ar.EndInvoke());
        }

        System.Net.Sockets.Socket IBluetoothListener.AcceptSocket()
        {
            throw new NotSupportedException("No Socket on Android.");
        }

        IAsyncResult IBluetoothListener.BeginAcceptSocket(AsyncCallback callback, object state)
        {
            throw new NotSupportedException("No Socket on Android.");
        }

        System.Net.Sockets.Socket IBluetoothListener.EndAcceptSocket(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("No Socket on Android.");
        }

        //--
        public bool Authenticate
        {
            get { return _auth; }
            set
            {
                CheckNotListening();
                _auth = value;
            }
        }

        public bool Encrypt
        {
            get { return _encr; }
            set
            {
                CheckNotListening();
                _encr = value;
            }
        }

        BluetoothEndPoint IBluetoothListener.LocalEndPoint
        {
            get
            {
                var scn = 0; // !!
                //scn = _server.GetChannel(); // @hide
                return new BluetoothEndPoint(BluetoothAddress.None, BluetoothService.Empty, scn);
            }
        }

        bool IBluetoothListener.Pending()
        {
            throw new NotImplementedException();
        }

        System.Net.Sockets.Socket IBluetoothListener.Server
        {
            get { throw new NotSupportedException("No Socket on Android."); }
        }

        //----
        void IBluetoothListener.SetPin(string pin)
        {
            throw new NotImplementedException();
        }

        void IBluetoothListener.SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
#endif

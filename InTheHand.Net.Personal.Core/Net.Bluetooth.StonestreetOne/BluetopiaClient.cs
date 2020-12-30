// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaClient
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using AR_Inquiry = InTheHand.Net.AsyncResult<System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>,
    InTheHand.Net.Bluetooth.Factory.DiscoDevsParams>;
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.Threading;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaClient : CommonBluetoothClient
    {
        readonly BluetopiaFactory _factory;
        BluetopiaSdpQuery _sdpQuery;

        //----
        internal BluetopiaClient(BluetopiaFactory fcty)
            : base(fcty, new BluetopiaRfcommStream(fcty))
        {
            Debug.Assert(fcty != null, "fcty NULL!");
            _factory = fcty;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "localEP")]
        internal BluetopiaClient(BluetopiaFactory fcty, BluetoothEndPoint localEP)
            : this(fcty)
        {
            throw new NotSupportedException("Don't support binding to a particular local address/port.");
        }

        internal BluetopiaClient(BluetopiaFactory fcty, CommonRfcommStream conn)
            : base(fcty, conn)
        {
            Debug.Assert(fcty != null, "fcty NULL!");
            _factory = fcty;
        }

        //----
#if DEBUG
        internal BluetopiaSdpQuery Testing_GetSdpQuery()
        {
            return _sdpQuery;
        }
#endif

        //----
        protected override void BeforeConnectAttempt(BluetoothAddress target)
        {
            ApplyAddressLessPin(target);
        }

        protected override void AfterConnectAttempt()
        {
            RevokeAddressLessPin();
        }

        //----
        public override IAsyncResult BeginServiceDiscovery(BluetoothAddress address, Guid serviceGuid,
            AsyncCallback asyncCallback, object state)
        {
            if (_sdpQuery == null) {
                _sdpQuery = new BluetopiaSdpQuery(_factory);
            }
            return _sdpQuery.BeginQuery(address, serviceGuid, true, asyncCallback, state);
        }

        public override List<int> EndServiceDiscovery(IAsyncResult ar)
        {
            if (_sdpQuery == null)
                throw new InvalidOperationException("BeginGetServiceRecords not called");
            var list = _sdpQuery.EndQuery(ar);
            var portList = BluetoothConnector.ListAllRfcommPortsInRecords(list);
            return portList;
        }

        //----
        protected override void BeginInquiry(int maxDevices, AsyncCallback callback, object state,
            InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            IAsyncResult ar = _factory.BeginInquiry(maxDevices, InquiryLength,
                callback, state,
                liveDiscoHandler, liveDiscoState, args);
            //} catch (BluetopiaSocketException ex) {
            //    if (ex.BluetopiaErrorCode == (int)BluetopiaError.INVALID_PARAMETER) {
            //        // Apparently means one is already in progress, so just
            //        // complete as if it returned no results.
            //        var ar = new AR_Inquiry(callback, state, args);
            //        var empty = new List<IBluetoothDeviceInfo>();
            //        ar.SetAsCompleted(empty, AsyncResultCompletion.MakeAsync);
            //        return;
            //    }
            //    throw;
            //}
        }

        protected override List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            List<IBluetoothDeviceInfo> devices = _factory.EndInquiry(ar);
            return devices;
        }

        //----
        protected override List<IBluetoothDeviceInfo> GetKnownRemoteDeviceEntries()
        {
            return _factory.GetRememberedDevices(true, true);
        }

        //----
        string _pinWithNoAddressGiven;
        // Given address, or address used at PinNowWithAddress.
        BluetoothAddress _pinAddress;

        public override void SetPin(BluetoothAddress device, string pin)
        {
            _pinAddress = device;
            _pinWithNoAddressGiven = null;
            ApplyPin(device, pin);
        }

        public override void SetPin(string pin)
        {
            _pinAddress = null;
            _pinWithNoAddressGiven = pin;
        }

        //
        private void ApplyPin(BluetoothAddress device, string pin)
        {
            var sec = _factory.DoGetBluetoothSecurity();
            sec.SetPin(device, pin);
        }

        private void ApplyAddressLessPin(BluetoothAddress address)
        {
            if (_pinWithNoAddressGiven != null) {
                Debug.WriteLine("Doing address-less PIN now.");
                _pinAddress = address;
                ApplyPin(_pinAddress, _pinWithNoAddressGiven);
            }
        }

        private void RevokeAddressLessPin()
        {
            if (_pinWithNoAddressGiven != null) {
                Debug.WriteLine("Revoking address-less PIN now.");
                var sec = _factory.DoGetBluetoothSecurity();
                var ret = sec.RevokePin(_pinAddress);
                _pinAddress = null;
                Debug.WriteLine("RevokePin<-RevokeAddressLessPin: " + ret);
            }
        }

        //--
        ~BluetopiaClient()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                base.Dispose(disposing);
            } finally {
                if (_pinAddress != null) {
                    var sec = _factory.DoGetBluetoothSecurity();
                    var ret = sec.RevokePin(_pinAddress);
                    Debug.WriteLine("RevokePin<-Dispose: " + ret);
                }
            }
        }

    }
}

// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezFactory
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License
#if BlueZ
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.Factory;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "InTheHand.Net.Bluetooth.BlueZ")]

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezFactory : BluetoothFactory
    {
        readonly object _lock = new object();
        int? _hDevId, _hDevDescr;
        readonly BluezDbus _busBluez;

        //----
        public BluezFactory()
        {
            var platformStr = Environment.OSVersion.Platform.ToString();
            if (platformStr.StartsWith("Win", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Linux only, NOT Windows.");
            }
            Console.WriteLine("BluezFactory platform: {0} = 0x{0:x}", Environment.OSVersion.Platform);
            _busBluez = new BluezDbus(this);
#if !ONE_RADIO
            InitRadios();
#endif
            GetPrimaryRadio();
            //
            bool hackOnWindows = false;
            if (hackOnWindows) {
                _radioList = new List<IBluetoothRadio>();
                _radioList.Add(new NullBluetoothFactory.NullRadio());
            }
            Console.WriteLine("Done BluezFactory init.");
        }

        protected override void Dispose(bool disposing)
        {
            CloseStack();
            _radioList = null;
            _radioPrimary = null;
        }

        //----
        /// <summary>
        /// Current 0.  0 ==> 10secs? for 
        /// </summary>
        internal readonly int StackTimeout = 0;

        internal int DevId
        {
            get
            {
                OpenStack();
                return _hDevId.Value;
            }
        }

        internal int DevDescr
        {
            get
            {
                OpenStack();
                return _hDevDescr.Value;
            }
        }

        private void OpenStack()
        {
            lock (_lock) {
                if (_hDevDescr == null) {
                    BluezError ret;
                    int hDevId = NativeMethods.hci_get_route(IntPtr.Zero); // TODO (BlueZ: open specific interface)
                    ret = (BluezError)hDevId;
                    BluezUtils.CheckAndThrow(ret, "hci_get_route");
                    int hDevDescr = NativeMethods.hci_open_dev(hDevId);
                    ret = (BluezError)hDevDescr;
                    BluezUtils.CheckAndThrow(ret, "hci_open_dev");
                    if (hDevId < 0 || hDevDescr < 0) {
                        Debug.Fail("should have been detected above");
                        BluezUtils.Throw(ret, "opening socket");
                    }
                    _hDevId = hDevId;
                    _hDevDescr = hDevDescr;
                    Console.WriteLine("Id: {0}, DD: {1}", DevId, DevDescr);
                }
            }
        }

        void CloseStack()
        {
            lock (_lock) {
                var hDevD = _hDevDescr;
                if (hDevD != null) {
                    BluezUtils.close(hDevD.Value);
                }
            }
        }

        public BluezDbus BluezDbus { get { return _busBluez; } }

        //----
        protected override IBluetoothClient GetBluetoothClient()
        {
            return new BluezClient(this);
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            return new BluezClient(this, localEP);
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            return new BluezClient(this, acceptedSocket);
        }

        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStrm)
        {
            throw new NotSupportedException();
        }

        protected override IL2CapClient GetL2CapClient()
        {
            return new BluezL2CapClient(this);
        }

        //----
        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            return BluezDeviceInfo.CreateFromGivenAddress(this, address);
        }

        //----
        protected override IBluetoothListener GetBluetoothListener()
        {
            return new BluezListener(this);
        }

        //----
        IBluetoothRadio _radioPrimary;
        List<IBluetoothRadio> _radioList;

        void InitRadios()
        {
            if (_radioPrimary != null)
                return;
            //
            int idPrimary = NativeMethods.hci_get_route(IntPtr.Zero);
           /////////////////////////////// BluezUtils.CheckAndThrow((BluezError)idPrimary, "hci_get_route");
            var dd = NativeMethods.hci_open_dev(idPrimary);
            Console.WriteLine("InitRadios idPrimary: {0}, dd: {1}", idPrimary, dd);
            _radioPrimary = new BluezRadio(this, dd);
            //
            int maxToTry = 10;
            var list = new List<IBluetoothRadio>();
            for (int curId = 0; curId < maxToTry; ++curId) {
                var curDd = NativeMethods.hci_open_dev(curId);
                var ret = (BluezError)curDd;
                if (BluezUtils.IsSuccess(ret)) {
                    Console.WriteLine("InitRadios curDd: {0}", curDd);
                    var curR = new BluezRadio(this, curDd);
                    list.Add(curR);
                }
            }
            _radioList = list;
            Debug.Assert(_radioList.Count >= 1);
        }

        protected override IBluetoothRadio GetPrimaryRadio()
        {
#if ONE_RADIO
            InitRadios();
            return _primaryRadio;
#else
            return new BluezRadio(this, this.DevDescr);
#endif
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
#if ONE_RADIO
            InitRadios();
            return new IBluetoothRadio[] { GetPrimaryRadio() };
#else
            return _radioList.ToArray();
#endif
        }

        //----
        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            return new BluezSecurity(this);
        }

    }
}
#endif
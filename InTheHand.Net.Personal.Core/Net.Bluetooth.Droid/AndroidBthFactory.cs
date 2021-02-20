// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{
    sealed class AndroidBthFactory : AndroidBthFactoryBase
    {
        public AndroidBthFactory()
            : base(GetDefaultAdapter())
        {
        }

        static BluetoothAdapter GetDefaultAdapter()
        {
            var a = BluetoothAdapter.DefaultAdapter;
            if (a == null) throw new InvalidOperationException("No Android Bluetooth radio.");
            return a;
        }

    }

#if DEBUG
    [CLSCompliant(false)]
    public
#endif
 abstract class AndroidBthFactoryBase : BluetoothFactory
    {
        AndroidBthRadio _radio;

        //--
        protected AndroidBthFactoryBase(BluetoothAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _radio = new AndroidBthRadio(adapter);
        }

        //--
        internal BluetoothAdapter GetAdapter()
        {
            return _radio.Adapter;
        }

        internal AndroidBthInquiry GetInquiry()
        {
            return new AndroidBthInquiry(this);
        }

        //--
        protected override IBluetoothRadio GetPrimaryRadio()
        {
            return _radio;
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            var arr = new IBluetoothRadio[1];
            arr[0] = GetPrimaryRadio();
            return arr;
        }

        //--
        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            return AndroidBthDeviceInfo.CreateFromGivenAddress(this, address, true);
        }

        internal AndroidBthDeviceInfo DoGetBluetoothDeviceInfoInternalOnly(BluetoothAddress address)
        {
            return AndroidBthDeviceInfo.CreateFromGivenAddress(this, address, false);
        }

        //--
        protected override IBluetoothClient GetBluetoothClient()
        {
            return new AndroidBthClient(this);
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            throw new NotImplementedException();
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            throw new NotSupportedException();
        }

        protected internal virtual IBluetoothClient DoGetBluetoothClientForListener(BluetoothSocket sock)
        {
            return AndroidBthClient.CreateFromListener(this, sock);
        }

        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStrm)
        {
            throw new NotImplementedException();
        }

        //--
        protected override IBluetoothListener GetBluetoothListener()
        {
            return new AndroidBthListener(this);
        }

        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            throw new NotImplementedException();
        }

        //--
        protected override void Dispose(bool disposing)
        {
            // throw new NotImplementedException();
        }

        // virtual on Factory so it is mockable.
        public virtual Java.Util.UUID ToJavaUuid(Guid guid)
        {
            var uuid = Java.Util.UUID.FromString(guid.ToString("D")); // Hyphens no braces.
#if true || DEBUG
            var txtN = guid.ToString("D"); // Hyphens no braces.
            var txtJ = uuid.ToString();
            Utils.MiscUtils.Trace_Assert(string.Equals(txtJ, txtN, StringComparison.OrdinalIgnoreCase),
                "NOT equal " + txtJ + " and " + txtN);
#endif
            return uuid;
        }

        public virtual Guid FromJavaUuid(Java.Util.UUID uuid)
        {
            var str = uuid.ToString();
            var guid = new Guid(str);
#if true || DEBUG
            var txtN = guid.ToString("D"); // Hyphens no braces.
            var txtJ = uuid.ToString();
            Utils.MiscUtils.Trace_Assert(string.Equals(txtJ, txtN, StringComparison.OrdinalIgnoreCase),
                "NOT equal " + txtJ + " and " + txtN);
#endif
            return guid;
        }

    }
}
#endif

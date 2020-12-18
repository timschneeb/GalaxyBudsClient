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
using InTheHand.Net.Sockets;
using Android.App;
using Android.Content;

namespace InTheHand.Net.Bluetooth.Droid
{
    public // HACK Android Inquiry PRE-RELEASE!!!
        sealed partial class AndroidBthInquiry : CommonBluetoothInquiry<IBluetoothDeviceInfo>, IDisposable
    {
        [Obsolete("PRE-RELEASE!!!")]
        [CLSCompliant(false)]
        public static Activity MyActivity { get; set; }

        readonly internal AndroidBthFactoryBase _fcty;
        DiscoRcvr1 _rcvr;
        Context _ctx;
        volatile bool _handlerActive;

        internal AndroidBthInquiry(AndroidBthFactoryBase fcty)
        {
            _fcty = fcty;
            //_acty = new Activity();
            var context = GetContext();
            StartReceiver(context);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AndroidBthInquiry()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            RemoveReceiver();
        }

        //--------
        internal void DoBeginInquiry(int maxDevices, TimeSpan inquiryLength,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            System.Threading.ThreadStart startInquiry = DoStartInquiry;
            base.BeginInquiry(maxDevices, inquiryLength, callback, state,
                liveDiscoHandler, liveDiscoState,
                startInquiry, args);
        }

        void DoStartInquiry()
        {
            bool success = false;
            try {
                _handlerActive = true;
                bool ok = _fcty.GetAdapter().StartDiscovery();
                if (!ok)
                    throw new InvalidOperationException("Device Discovery failed; maybe Bluetooth is disabled.");
                success = true;
            } finally {
                if (!success) {
                    _handlerActive = false;
                }
            }//finally
        }

        //--
        protected override IBluetoothDeviceInfo CreateDeviceInfo(IBluetoothDeviceInfo item)
        {
            // We create the BDI in the native handler
            // Maybe we could pass the intent through to here, but why.
            return item;
        }

    }
}
#endif

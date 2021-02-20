// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License
using System.Diagnostics;

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Content;
#if !FAKE_ANDROID_BTH_API
using Java.Lang;
using Android.Runtime;
#endif

namespace InTheHand.Net.Bluetooth.Droid
{

#if !FAKE_ANDROID_BTH_API

    partial class AndroidBthInquiry
    {
        Context GetContext()
        {
            Context contextMono = Application.Context;
            Context contextTmp = MyActivity;
            return contextMono;
            //Context contextApplic = GetApplication ();
            //if (context == null) {
            //    throw new InvalidOperationException ("For now, must set 'AndroidBthInquiry.MyActivity' before calling discovery.");
            //}
            //return context;
        }

        //--------
        class DiscoRcvr1 : BroadcastReceiver
        {
            const string MyFakeActionFound = "uk.me.alanjmcf.action.TEST_DISCO1";
            //
            readonly AndroidBthInquiry _parent;

            //
            internal DiscoRcvr1(AndroidBthInquiry parent)
            {
                _parent = parent;
            }

            //
            public override void OnReceive(Context context, Intent intent)
            {
                //Toast.MakeText (context, "Received intent: '"
                //  + intent.Action + "'",
                //  ToastLength.Long).Show ();
                if (intent.Action == BluetoothAdapter.ActionDiscoveryStarted) {
                    //
                } else if (intent.Action == MyFakeActionFound
                            || intent.Action == BluetoothDevice.ActionFound) {
                    var objD = intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    var dev = (BluetoothDevice)objD;
                    if (!_parent._handlerActive) {
                        // ActionFound occurs if any installed app/system runs inquiry.
                    } else {
                        var objC = intent.GetParcelableExtra(BluetoothDevice.ExtraClass);
                        var cod = (BluetoothClass)objC;
                        var foo = intent.GetStringExtra("foo");
                        string optName = intent.GetStringExtra(BluetoothDevice.ExtraName);
                        short optRssi = intent.GetShortExtra(BluetoothDevice.ExtraRssi, Int16.MinValue);
                        AndroidBthDeviceInfo bdi = AndroidBthDeviceInfo.CreateFromInquiry(
                            _parent._fcty,
                            dev, cod, optName, optRssi);
                        _parent.HandleInquiryResultInd(bdi);
                    }
                } else if (intent.Action == BluetoothAdapter.ActionDiscoveryFinished) {
                    int? numResponses = null;
                    _parent._handlerActive = false;
                    _parent.HandleInquiryComplete(numResponses);
                }
            }

        }//class2

        void StartReceiver(Context context)
        {
            _ctx = context;
            _rcvr = new DiscoRcvr1(this);
            var ifi = new Android.Content.IntentFilter();
            ifi.AddAction(Android.Bluetooth.BluetoothAdapter.ActionDiscoveryStarted);
            ifi.AddAction(Android.Bluetooth.BluetoothAdapter.ActionDiscoveryFinished);
            ifi.AddAction(Android.Bluetooth.BluetoothDevice.ActionFound);
            //ifi.AddAction (MyFakeActionFound);
            //ifi.AddAction ("android.intent.action.CAMERA_BUTTON");
            _ctx.RegisterReceiver(_rcvr, ifi);
        }

        void RemoveReceiver()
        {
            var rcvr = _rcvr;
            _rcvr = null;
            if (rcvr != null) {
                _ctx.UnregisterReceiver(rcvr);
            }
        }


    }


#else
    // FAKE FAKE FAKE
    partial class AndroidBthInquiry
    {
        Context GetContext() { throw new NotImplementedException(); }
        void StartReceiver(Context ctx)
        {
            _rcvr = _rcvr = null;
            _ctx = _ctx = null;
            _handlerActive = _handlerActive = false;
        }
        void RemoveReceiver() { }

        //--------
        class DiscoRcvr1 //: BroadcastReceiver
        { }
    }

#endif
}
#endif
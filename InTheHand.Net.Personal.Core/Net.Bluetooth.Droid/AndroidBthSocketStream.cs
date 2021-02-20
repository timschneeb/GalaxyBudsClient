// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Droid
{
    class AndroidBthSocketStream : Utils.PairStream
    {
        BluetoothSocket _sock;
        readonly Stream _in;
        readonly Stream _out;

        internal AndroidBthSocketStream(BluetoothSocket sock)
            : base(sock.InputStream, sock.OutputStream)
        {
            Debug.Assert(sock != null, "sock IS null");
            _sock = sock;
            _in = _sock.InputStream;
            _out = _sock.OutputStream;
        }

        ~AndroidBthSocketStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                base.Dispose(disposing);
            } finally {
                _sock.Close();
            }
        }

    }
}
#endif

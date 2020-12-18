// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Factory.IBluetoothSecurity
// 
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2011 Alan J. McFarlane, All rights reserved.
//
// This source code is licensed under the MIT License

using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Factory
{
    class TimeoutDecorStream : Stream
    {
        readonly Stream _child;

        //----
        public TimeoutDecorStream(Stream child)
        {
            _child = child;
        }

        //----

        #region Stream members
        public override bool CanRead { get { return _child.CanRead; } }

        public override bool CanSeek
        {
            get
            {
                Debug.Assert(!_child.CanSeek, "INFO: blahhh");
                return false;
            }
        }

        public override bool CanWrite { get { return _child.CanWrite; } }

        public override void Flush()
        {
            _child.Flush();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ar = _child.BeginRead(buffer, offset, count, null, null);
            DoTimeoutIfAndClose(ar, ReadTimeout);
            return _child.EndRead(ar);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var ar = _child.BeginWrite(buffer, offset, count, null, null);
            DoTimeoutIfAndClose(ar, WriteTimeout);
            _child.EndWrite(ar);
        }

        public override int ReadTimeout
        {
            get;
            set;
        }
        public override int WriteTimeout
        {
            get;
            set;
        }
        #endregion


        void DoTimeoutIfAndClose(IAsyncResult ar, int timeout)
        {
            if (!InTheHand.Net.Bluetooth.Factory.CommonRfcommStream.IsInfiniteTimeout(timeout)) {
                var signalled = ar.AsyncWaitHandle.WaitOne(timeout, false);
                if (!signalled) {
                    try {
                        const int WSAETIMEDOUT = 10060;
                        var soex = new SocketException(WSAETIMEDOUT);
                        throw new IOException(soex.Message, soex);
                    } finally {
                        Close();
                    }
                }//if
            }
        }

    }
}

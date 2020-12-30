// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Net.Sockets;


namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    class BlueSoleilSerialPortNetworkStream : SerialPortNetworkStream, IBluesoleilConnection
    {
        // TEMP!!!!
        delegate int DlgtRead(byte[] buffer, int offset, int count);
        delegate void DlgtWrite(byte[] buffer, int offset, int count);
        //
        const string ObjectDisposedException_ObjectName = "Network";
        internal const string WrappingIOExceptionMessage = "IOError on socket.";
        //
        UInt32? _hConn;
        readonly BluesoleilFactory _factory;
        EventWaitHandle _received = new AutoResetEvent(false);
        enum State
        {
            New,
            //Connecting,
            Connected,
            PeerDidClose,
            Closed
        };
        volatile State _state;

        internal BlueSoleilSerialPortNetworkStream(SerialPort port, UInt32 hConn,
            BluesoleilClient cli, BluesoleilFactory factory)
            : this(new SerialPortWrapper(port), hConn, cli, factory)
        {
        }

        internal BlueSoleilSerialPortNetworkStream(ISerialPortWrapper port, UInt32 hConn,
            BluesoleilClient cli, BluesoleilFactory factory)
            : base(port, cli)
        {
            _hConn = hConn;
            // (Don't need Cli?)
            _factory = factory;
            //
            _state = State.Connected;
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        //--------
        void IBluesoleilConnection.CloseNetworkOrInternal()
        {
            _state = State.PeerDidClose;
            //Debug.Assert(base.CanRead, "CanRead before");
            //Debug.Assert(base.CanWrite, "CanWrite before");
            //_port.Close();
            //Debug.Assert(!base.CanRead, "CanRead after");
            //Debug.Assert(!base.CanWrite, "CanWrite after");
            // Unblock any Reader.
            _received.Set();
            _received.Close();
            _hConn = null;
        }

        protected override void Dispose(bool disposing)
        {
            _state = State.Closed;
            try {
                var hConn = _hConn;
                if (hConn != null) {
                    _hConn = null;
                    BtSdkError ret = _factory.Api.Btsdk_Disconnect(hConn.Value);
                    Debug.Assert(ret == BtSdkError.OK, "Btsdk_Disconnect ret: " + ret);
                }
                // Unblock any Reader.
                try {
                    _received.Set();
                } catch (ObjectDisposedException) {
                    Debug.Assert(_state == State.PeerDidClose || _state == State.Closed,
                        "_received event already closed but unexpected state: " + _state);
                }
                _received.Close();
            } finally {
                //Debug.Assert(base.CanRead, "CanRead before");
                //Debug.Assert(base.CanWrite, "CanWrite before");
                base.Dispose(disposing);
                //Debug.Assert(!base.CanRead, "CanRead after");
                //Debug.Assert(!base.CanWrite, "CanWrite after");
            }
        }

        internal override bool Connected
        {
            get
            {
                // TODO SerialPortNetworkStream.Connected, result based on last IO.
                return _state == State.Connected;
            }
        }

        //--------
        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _received.Set();
        }


        //internal sealed class BeginReadParameters // HACK from Widcomm
        //{
        //    //Unused: public readonly int startTicks = Environment.TickCount;
        //    public byte[] buffer;
        //    public int offset;
        //    public int count;
        //
        //    public BeginReadParameters(byte[] buffer, int offset, int count)
        //    {
        //        this.buffer = buffer;
        //        this.offset = offset;
        //        this.count = count;
        //    }
        //}

        //AsyncResult<int, BeginReadParameters> _oneBeginReadOperation;


        DlgtRead _readDlgt;

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (_readDlgt == null) { _readDlgt = Read; }
            return _readDlgt.BeginInvoke(buffer, offset, count, callback, state);
            //
            //throw new NotImplementedException();
            //var args = new BeginReadParameters(buffer, offset, count);
            //var ar = new AsyncResult<int, BeginReadParameters>(callback, state, args);
            //var prev = Interlocked.CompareExchange(ref _oneBeginReadOperation, ar, null);
            //if (prev != null) {
            //    throw new InvalidOperationException("Currently only support one (Begin-)Read operation at a time.");
            //}
            //return ar;
            //int btr = _port.BytesToRead;
            //if (btr >= 1) {
            //    //_received.WaitOne();
            //    //IAsyncResult arBase = base.BeginRead(buffer, offset, count, null, null);
            //    //return 
            //}
            //throw new NotImplementedException();
        }

        //void Reader_Runner(object state)
        //{
        //    int btr = _port.BytesToRead;
        //    if (btr < 1) {
        //        _received.WaitOne();
        //    }
        //    IAsyncResult arBase = base.BeginRead(buffer, offset, count, null, null);
        //}

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _readDlgt.EndInvoke(asyncResult);
            //
            //throw new NotImplementedException();
            //return base.EndRead(asyncResult);
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            //return EndRead(BeginRead(buffer, offset, count, null, null));
            int readLen;
            if (_state == State.Closed) {
                goto isClosedBefore;
            }
            // Block all other threads from Reading until we're done.
            lock (_received) {
                if (_state == State.Closed) {
                    goto isClosedInterrupted;
                }
                int btrX;
                //
                while ((btrX = _port.BytesToRead) < 1) {
                    if (_state == State.PeerDidClose) {
                        // Otherwise we'll block waiting for data that will never come!
                        //Debug.Assert(_received.SafeWaitHandle.IsClosed, "NOT SafeWaitHandle.IsClosed");
                        goto isClosedBefore;
                    }
                    _received.Reset();
                    _received.WaitOne();
                    if (_state != State.Connected) {
                        goto isClosedInterrupted;
                    }
                    // This might help get the right value of BytesToRead.
                    Thread.MemoryBarrier();
                }
                readLen = base.Read(buffer, offset, count);
            }
            return readLen;
        isClosedBefore:
            switch (_state) {
                case State.PeerDidClose:
                    return 0;
                case State.Closed:
                    throw new ObjectDisposedException(ObjectDisposedException_ObjectName);
                case State.New:
                case State.Connected:
                default:
                    Debug.Fail("Unexpected _isClosed_ state: " + _state);
                    goto case State.Closed;
            }
        isClosedInterrupted:
            switch (_state) {
                case State.PeerDidClose:
                    return 0;
                case State.Closed:
                    throw new IOException(WrappingIOExceptionMessage,
                        new SocketException((int)SocketError.Interrupted));
                case State.New:
                case State.Connected:
                default:
                    Debug.Fail("Unexpected _isClosed_ state: " + _state);
                    goto case State.Closed;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_state == State.Closed)
                throw new ObjectDisposedException(ObjectDisposedException_ObjectName);
            // Limit each write to half the buffer size.  See if this stops data loss...
            int limit = _port.WriteBufferSize / 2;
            int remainingCount = count;
            int curOffset = offset;
            while (true) {
                int curCount = Math.Min(limit, remainingCount);
                try {
                    if (_state == State.PeerDidClose)
                        // We don't close the underlying stream in that case so
                        // must jump to the closed handling code ourselves.
                        throw new ObjectDisposedException("INTERNAL!!!");
                    base.Write(buffer, curOffset, curCount);
                } catch (ObjectDisposedException) {
                    if (_state == State.Connected) {
                        // Unexpected! Fall through to throw
                    } else if (_state == State.PeerDidClose) {
                        var soex = new SocketException((int)SocketError.ConnectionAborted);
                        throw new IOException(soex.Message, soex);
                    } else if (_state == State.Closed) {
                        throw new IOException(WrappingIOExceptionMessage,
                            new SocketException((int)SocketError.Interrupted));
                    } else {
                        Debug.Fail("unexpected state at Write=ObjDispEx: " + _state);
                        // fall through to throw
                    }
                    throw;
                }
                curOffset += curCount;
                remainingCount -= curCount;
                Debug.Assert(remainingCount >= 0, "NOT curCount >= 0, is: " + remainingCount);
                if (remainingCount <= 0) {
                    break;
                }
            }//while
        }

        // TEMP!!!!
        DlgtWrite _writeDlgt;

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // TEMP!!!!
            if (_writeDlgt == null) { _writeDlgt = Write; }
            return _writeDlgt.BeginInvoke(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            // TEMP!!!!
            _writeDlgt.EndInvoke(asyncResult);
        }

    }
}

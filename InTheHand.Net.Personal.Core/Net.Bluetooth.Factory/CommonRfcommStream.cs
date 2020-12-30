// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2008-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using InTheHand.Net;
#if NETCF
using InTheHand.Net.Sockets;
#endif
//
using WriteAsyncResult = InTheHand.Net.AsyncNoResult<InTheHand.Net.Bluetooth.Factory.CommonRfcommStream.BeginReadParameters>;
using ReadAsyncResult = InTheHand.Net.AsyncResult<int, InTheHand.Net.Bluetooth.Factory.CommonRfcommStream.BeginReadParameters>;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    /// <summary>
    /// 
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Sub-class must call various methods at the following events:
    /// <list type="bullet">
    /// <item><term>open</term>
    /// <description><see cref="HandleCONNECTED"/>
    /// or <see cref="HandleCONNECT_ERR"/> on failure</description>
    /// </item>
    /// <item><term>close</term>
    /// <description><see cref="HandleCONNECT_ERR"/></description>
    /// </item>
    /// <item><term>data arrival</term>
    /// <description><see cref="HandlePortReceive"/></description>
    /// </item>
    /// <item><term>flow control off</term>
    /// <description><see cref="FreePendingWrites"/></description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    [CLSCompliant(false)] // Due to UInt16 params in: DoWrite(byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written);
    public abstract class CommonRfcommStream : Stream
    {
        // !!!
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member '
        const string ObjectDisposedException_ObjectName = "Network";
        internal const string WrappingIOExceptionMessage = "IOError on socket.";
        //
        protected enum State
        {
            New,
            //Connecting,
            Connected,
            PeerDidClose,
            Closed
        };
        protected State m_state_;
        // Socket.get_Connected reports false only after a (failed) user IO operation,
        // so we should do the same.  So m_state reports the live state, and this
        // property reports the user-visible connected state.
        bool m_connected;
        protected/*TODO?*/ object _lockKey = new object();
        BluetoothAddress m_remoteAddress;
        // Connect
        BluetoothAddress m_addressToConnect;
        int m_ocScn;
        AsyncResultNoResult m_arConnect, m_arConnectCompleted;
        // Receive
        int m_receivedDataFirstBlockOffset;
        Queue<byte[]> m_receivedData = new Queue<byte[]>();
        int m_amountInReadBuffers;
        Queue<ReadAsyncResult> m_arReceiveList = new Queue<AsyncResult<int, BeginReadParameters>>();
        // Write
        Queue<WriteAsyncResult> m_arWriteQueue = new Queue<WriteAsyncResult>();
        ManualResetEvent m_writeEmptied;
        //
#if DEBUG && ! PocketPC
        string m_creationStackTrace;
#endif

        //--------
        protected CommonRfcommStream()
        {
#if DEBUG
#if TRACE_TO_FILE
            //m_sbLogging = new StringBuilder();
            //StringWriter wtr = new StringWriter(m_sbLogging, System.Globalization.CultureInfo.InvariantCulture);
            TextWriter wtr = File.CreateText("RfcommStream.txt");
            m_logWtr = TextWriter.Synchronized(wtr);
#else
            m_log = new string[LogNumberOfLines];
#endif
#endif
#if DEBUG && ! PocketPC
            m_creationStackTrace = Environment.StackTrace;
#endif
        }

        //----------------
#if DEBUG
#if TRACE_TO_FILE
        readonly StringBuilder m_sbLogging;
        readonly TextWriter m_logWtr;
#else
        const int LogNumberOfLines = 64;
        string[] m_log;
        int m_logIdx;
#endif
#endif

        [Conditional("DEBUG")]
        protected void Log(string message)
        {
            Utils.MiscUtils.Trace_WriteLine(message); // for PC command-line etc
#if DEBUG
#if TRACE_TO_FILE
            m_logWtr.WriteLine(message);  // for NETCF+debugger
#else
            int idx0, idx;
            idx = idx0 = Interlocked.Increment(ref m_logIdx);
            uint ui = unchecked((uint)idx);
            ui %= (uint)m_log.Length;
            idx = checked((int)ui);
            m_log[idx] = "" + idx0 + "-- " + message; // for NETCF+debugger
#endif
#endif
        }

        [Conditional("DEBUG")]
        protected void LogFormat(string format, params object[] args)
        {
            Log(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                format, args));
        }

        protected virtual void ResetDebugId()
        {
        }

        internal protected virtual string DebugId { get { return "NULL_DEBUGID"; } }

        //----------------
        private LingerOption m_lingerState = new LingerOption(true, 0);

        internal LingerOption LingerState
        {
            get { return m_lingerState; }
            set
            {
                if (!value.Enabled)
                    throw new ArgumentException("No support for non-Linger mode in Widcomm wrapper.");
                m_lingerState = value;
            }
        }

        //----------------
        /// <summary>
        /// Fails if state is not Connected.
        /// </summary>
        private void EnsureOpenForWrite()
        {
            EnsureOpenForRead();
            if (m_state_ == State.Connected)
                return;
            //
            if (m_state_ == State.PeerDidClose) { // Extra negative condition to EnsureOpenForRead
                SetAsDisconnected();
                throw new IOException(WrappingIOExceptionMessage,
                    FooSocketExceptions.ConnectionIsPeerClosed());
            }
            Debug.Fail("Unknown state: " + m_state_);
            throw new InvalidOperationException("Not connected.");
        }

        /// <summary>
        /// Fails if state is not Connected or PeerDidClose.
        /// </summary>
        private void EnsureOpenForRead()
        {
            if (m_state_ == State.Closed)
                throw new IOException(WrappingIOExceptionMessage,
                    new ObjectDisposedException(ObjectDisposedException_ObjectName));
            if (m_state_ == State.New)
                throw new InvalidOperationException("Not connected.");
            if (!m_connected)
                throw new IOException(WrappingIOExceptionMessage,
                    FooSocketExceptions.ConnectionIsPeerClosed());
            if (m_state_ == State.Connected || m_state_ == State.PeerDidClose)
                return;
            //
            Debug.Fail("Unknown state: " + m_state_);
            throw new InvalidOperationException("Not connected.");
        }

        /// <summary>
        /// Used by Client, note from MSDN Socket.Connected:
        /// "Gets a value that indicates whether a Socket is connected to a remote host as of the last Send or Receive operation."
        /// </summary>
        /// -
        /// <remarks>
        /// <para>From MSDN <see cref="P:System.Net.Sockets.Socket.Connected"/>:
        /// "Gets a value that indicates whether a Socket is connected to a remote host as of the last Send or Receive operation."
        /// From MSDN <see cref="P:System.Net.Sockets.TcpClient.Connected"/>:
        /// "true if the Client socket was connected to a remote resource as of the most recent operation; otherwise, false."
        /// </para>
        /// </remarks>
        internal bool Connected
        {
            get { return m_connected; }
        }

        internal bool LiveConnected
        {
            get { return m_state_ == State.Connected; }
        }

        void SetAsDisconnected()
        {
            Debug.Assert(m_connected, "already not m_connected");
            SetAsDisconnectedFromDisposal();
        }

        void SetAsDisconnectedFromDisposal()
        {
            Debug.Assert(m_state_ == State.Closed || m_state_ == State.PeerDidClose, "m_state open!");
            m_connected = false;
        }

        private void ImplicitPeerClose()
        {
            m_state_ = State.PeerDidClose;
        }

        protected override void Dispose(bool disposing)
        {
            //LogFormat("Dispose({0})", disposing);
            if (disposing) {
                /*DEBUG*/
            } else {
                /*DEBUG*/
            }
            //
            State m_prevState = m_state_;
            ReadAsyncResult[] readsToAbort;
            WriteAsyncResult[] writesToAbort;
            AsyncResultNoResult connectToAbort = null;
            Exception exToThrow = null;
            bool dbgExpectEmptyWriteQueue = false;
            try {
                if (m_state_ != State.Closed) {
                    try {
                        m_state_ = State.Closed;
                        SetAsDisconnectedFromDisposal();
                        DisposeLinger(disposing, out exToThrow, out dbgExpectEmptyWriteQueue);
                        lock (_lockKey) {
                            //Log("Dispose: in lock");
                            if (m_prevState == State.PeerDidClose) { // Don't know whether its ok to call twice...
                                // COVERAGE
                            }
                            //Debug.WriteLine(DateTime.Now.TimeOfDay.ToString()
                            //    + ": RemovePort from Close (state: " + m_state_ + ")");
                            RemovePortRecords();
                            DoPortClose(disposing);
                            ClearAllReadRequests_inLock(out readsToAbort);
                            ClearAllWriteRequests_inLock(out writesToAbort);
                            if (m_arConnect != null) {
                                connectToAbort = m_arConnect;
                                m_arConnectCompleted = m_arConnect;
                                m_arConnect = null;
                            }
                            ManualResetEvent wee = m_writeEmptied;
                            if (disposing && wee != null) {
                                m_writeEmptied = null;
                                wee.Close();
                            }
                        }//lock
                        // Do we need to abort these when being Finalized? XXXXXXXXXXXXXXXXXX
                        // TODO SetAsC on same thread (only on user/finalizer thread though).
                        AbortReads_WithEof(readsToAbort);
                        AbortWrites_AsPeerClose(writesToAbort);
                        if (connectToAbort != null) {
                            // (Leave this one as callback on this thread, its not a stack thread).
                            connectToAbort.SetAsCompleted(new ObjectDisposedException(
                                ObjectDisposedException_ObjectName), false);
                        }
                    } finally {
                        DoOtherPreDestroy(disposing);
                        //
                        // Could there be a race of an event arriving just as we call Close?
                        // And thus the callback called when the object are being deleted.
                        // So do we need to delay etc before calling this??
                        DoPortDestroy(disposing);
                    }
                }
            } finally {
                base.Dispose(disposing);
#if DEBUG
#if TRACE_TO_FILE
                m_logWtr.Flush(); // let it just finalize -- in case we write after this
#endif
#endif
            }
            Debug.Assert(!dbgExpectEmptyWriteQueue || m_arWriteQueue.Count == 0,
                "Told to expect no remaining Send data, but was some...");
            if (exToThrow != null) {
                throw exToThrow;
            }
        }

        /// <summary>
        /// <see cref="DoPortClose"/>
        /// </summary>
        protected abstract void RemovePortRecords();
        /// <summary>
        /// <see cref="DoPortClose"/>
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected abstract void DoOtherPreDestroy(bool disposing);

        /// <summary>
        /// Called from CloseInternal and Dispose;
        /// RemovePortRecords is called before from both places.
        /// Dispose then calls DoOtherPreDestroy and DoPortDestroy in that order.
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected abstract void DoPortClose(bool disposing);
        /// <summary>
        /// <see cref="DoPortClose"/>
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected abstract void DoPortDestroy(bool disposing);

        //----
        private void DisposeLinger(bool disposing, out Exception exToThrow, out bool dbgExpectEmptyWriteQueue)
        {
            if (disposing) {
                if (!LingerState.Enabled) {
                    throw new NotSupportedException("Background non-linger Close not supported.");
                } else {
                    if (LingerState.LingerTime == 0) {
                        // Hard close, so just proceed and abort the writes.
                        dbgExpectEmptyWriteQueue = true;
                        exToThrow = null;
                    } else {
                        // Wait for the linger-time to see if the writes are processed.
                        lock (_lockKey) {
                            if (m_arWriteQueue.Count == 0) {
                                // NOP
                            } else {
                                m_writeEmptied = new ManualResetEvent(false);
                            }
                        }
                        if (m_writeEmptied == null) {
                            Debug.Assert(m_arWriteQueue.Count == 0);
                            exToThrow = null;
                            dbgExpectEmptyWriteQueue = true;
                        } else {
                            const int MillisPerSeconds = 1000;
                            int msTime = LingerState.LingerTime * MillisPerSeconds;
                            bool emptied = m_writeEmptied.WaitOne(msTime, false);
                            if (emptied) {
                                Debug.Assert(m_arWriteQueue.Count == 0);
                                exToThrow = null;
                                dbgExpectEmptyWriteQueue = true;
                            } else {
                                Debug.Assert(m_arWriteQueue.Count != 0);
                                exToThrow = new Exception("Linger time-out FIXME");
                                dbgExpectEmptyWriteQueue = false;
                            }
                        }
                    }
                }
            } else {
                exToThrow = null;
                dbgExpectEmptyWriteQueue = false;
            }
        }

        ~CommonRfcommStream()
        {
            Dispose(false);
        }

        private static void MemoryBarrier()
        {
#if ! PocketPC
            Thread.MemoryBarrier();
#endif
        }

        protected virtual void VerifyPortIsInRange(BluetoothEndPoint bep)
        {
            VerifyRfcommScnIsInRange(bep);
        }

        protected static void VerifyRfcommScnIsInRange(BluetoothEndPoint bep)
        {
            if (bep.Port < BluetoothEndPoint.MinScn || bep.Port > BluetoothEndPoint.MaxScn)
                throw new ArgumentOutOfRangeException("bep", "Channel Number must be in the range 1 to 30.");
        }

        protected static void VerifyL2CapPsmIsInRange(BluetoothEndPoint bep)
        {
            var psm = checked((ushort)bep.Port);
        }



        protected internal IAsyncResult BeginConnect(BluetoothEndPoint bep, //string pin,
            AsyncCallback asyncCallback, Object state)
        {
            if (bep.Port == 0 || bep.Port == -1)
                throw new ArgumentException("Channel Number must be set in the BluetoothEndPoint, i.e. SDP lookup done.", "bep");
            VerifyPortIsInRange(bep);
            int scn = bep.Port;
            //
            lock (_lockKey) {
                if (m_state_ == State.Closed)
                    throw new ObjectDisposedException(ObjectDisposedException_ObjectName);
                if (m_state_ != State.New) {
                    throw new SocketException((int)SocketError.IsConnected); // [Begin]Connect called twice
                }
                if (m_arConnect != null)
                    throw new InvalidOperationException("Another Connect operation is already in progress.");
                Debug.Assert(m_arConnectCompleted == null);
                AsyncResultNoResult ar = new AsyncResultNoResult(asyncCallback, state);
                //
                DoOtherSetup(bep, scn);
                // Initiate the connect attempt, and then wait for the CONNECTED Event.
                m_ocScn = scn;
                m_addressToConnect = bep.Address;
                DoOpenClient(m_ocScn, m_addressToConnect);
                AddPortRecords();
                Debug.Assert(ar != null);
                m_arConnect = ar;
                return ar;
            }
        }

        /// <summary>
        /// Called before DoOpenClient.
        /// For instance is empty on BTPS, on Widcomm it calls SetScnForPeerServer and SetSecurityLevelClient.
        /// </summary>
        /// <param name="bep">Endpoint</param>
        /// <param name="scn">Channel number</param>
        protected abstract void DoOtherSetup(BluetoothEndPoint bep, int scn);
        protected abstract void AddPortRecords();

        /// <summary>
        /// Starts the connect process.  The async completion should call
        /// either <see cref="HandleCONNECTED"/> or <see cref="HandleCONNECT_ERR"/>.
        /// </summary>
        /// <param name="scn">scn</param>
        /// <param name="addressToConnect">addr</param>
        protected abstract void DoOpenClient(int scn, BluetoothAddress addressToConnect);

        internal void EndConnect(IAsyncResult ar)
        {
            // (Can't lock here as that would block the callback methods).
            if (ar != m_arConnect && ar != m_arConnectCompleted)
                throw new InvalidOperationException("Unknown IAsyncResult.");
            try {
                ((AsyncResultNoResult)ar).EndInvoke();
            } finally {
                MemoryBarrier();
                Debug.Assert(m_arConnect == null);
                m_arConnectCompleted = null;
            }
        }

        //----
        internal IAsyncResult BeginAccept(BluetoothEndPoint bep, string serviceName, AsyncCallback asyncCallback, Object state)
        {
            if (bep == null)
                throw new ArgumentNullException("bep");
            if (bep.Port == 0 || bep.Port == -1)
                throw new ArgumentException("Channel Number must be set in the BluetoothEndPoint, i.e. SDP lookup done.", "bep");
            VerifyPortIsInRange(bep);
            int scn = bep.Port;
            //
            lock (_lockKey) {
                if (m_state_ == State.Closed)
                    throw new ObjectDisposedException(ObjectDisposedException_ObjectName);
                if (m_state_ != State.New) {
                    throw new SocketException((int)SocketError.IsConnected); // [Begin]Connect/Accept called twice
                }
                //
                if (m_arConnect != null)
                    throw new InvalidOperationException("Another Connect operation is already in progress.");
                Debug.Assert(m_arConnectCompleted == null);
                AsyncResultNoResult ar = new AsyncResultNoResult(asyncCallback, state);
                //
                // Initiate the connect attempt, and then wait for the CONNECTED Event.
                DoOpenServer(scn);
                AddPortRecords();
                Debug.Assert(ar != null);
                m_arConnect = ar;
                return ar;
            }
        }

        protected abstract void DoOpenServer(int scn);

        internal void EndAccept(IAsyncResult ar)
        {
            // (Can't lock here as that would block the callback methods).
            if (ar != m_arConnect && ar != m_arConnectCompleted)
                throw new InvalidOperationException("Unknown IAsyncResult.");
            try {
                ((AsyncResultNoResult)ar).EndInvoke();
            } finally {
                MemoryBarrier();
                Debug.Assert(m_arConnect == null);
                m_arConnectCompleted = null;
            }
        }

        //----------------
        internal BluetoothAddress RemoteAddress { get { return m_remoteAddress; } }

        //----------------
        public override bool CanRead { get { return Connected; } }
        public override bool CanWrite { get { return Connected; } }
        public override bool CanSeek { get { return false; } }

        //----

        /// <summary>
        /// Call when connection is successfully made.
        /// </summary>
        /// <param name="eventId">Used for logging etc.  Pass a string
        /// containing the name of the stack's event/status that occurred.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void HandleCONNECTED(string eventId)
        {
            AsyncResultNoResult sacAr; // Call SetAsCompleted outside the lock.
            lock (_lockKey) {
                Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "CONNECTED {0}; m_state: {1}; m_arConnect {2}, IsCompleted: {3}.",
                    DebugId, m_state_,
                    (m_arConnect == null) ? "(null)" : "(set)",
                    (m_arConnect == null) ? "n/a" : m_arConnect.IsCompleted.ToString()));
                if (m_arConnect != null && !m_arConnect.IsCompleted) {
                    // Success!
                    m_state_ = State.Connected;
                    m_connected = true;
                    sacAr = m_arConnect;
                    m_arConnectCompleted = m_arConnect;
                    m_arConnect = null;
                    // Get the remote address
                    try {
                        bool connected = DoIsConnected(out m_remoteAddress);
                        Debug.Assert(connected || TestUtilities.IsUnderTestHarness(), "!CRfCommPort.IsConnected");
                    } catch (Exception ex) {
                        Debug.Assert(TestUtilities.IsUnderTestHarness(), DebugId + ": " + "m_port.IsConnected exception: " + ex);
                    }
                } else {
                    Debug.Assert(m_state_ == State.Connected
                        || m_state_ == State.Closed // Consumer called Close soon after connect.
                        ,
                        DebugId + ": " +
                        eventId + ": m_state wanted: " + "Connected" + ", but was: " + m_state_
                        + ((m_arConnect == null) ? " ar==(null)" : " ar==(non-null)")
                        );
                    sacAr = null;
                }
            }//lock
            if (sacAr != null)
                sacAr.SetAsCompleted(null, AsyncResultCompletion.MakeAsync);
        }

        /// <summary>
        /// Get the remote address.
        /// </summary>
        /// -
        /// <param name="p_remote_bdaddr">On return contains the address to which we are connected.
        /// </param>
        /// -
        /// <returns><see langword="true"/> if connected, but we ignore the result.
        /// </returns>
        protected virtual bool DoIsConnected(out BluetoothAddress p_remote_bdaddr)
        {
            if (m_addressToConnect != null) {
                p_remote_bdaddr = m_addressToConnect;
            } else {
                Debug.Assert(TestUtilities.IsUnderTestHarness(), "Don't know remote address!  Probably because we're used by BtLsnr...");
                p_remote_bdaddr = BluetoothAddress.None;
            }
            return true;
        }


        /// <summary>
        /// Call when connection is un-successfully made (fails),
        /// and also when the connection closes.
        /// </summary>
        /// <param name="eventId">Used for logging etc.  Pass a string
        /// containing the name of the stack's event/status that occurred.
        /// </param>
        /// <param name="socketErrorCode">The socket error code for this failure
        /// -- known.
        /// Pass for instance a value from <see cref="T:System.Net.Sockets.SocketError"/>
        /// as an <see cref="T:System.Int32"/>;
        /// or <see langword="null"/> respectively.
        /// </param>
        protected void HandleCONNECT_ERR(string eventId, int? socketErrorCode)
        {
            AsyncResultNoResult sacAr; // Call SetAsCompleted outside the lock.
            Exception sacEx;
            ReadAsyncResult[] allRead = null;
            WriteAsyncResult[] allWrite = null;
            lock (_lockKey) {
                Debug.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "CONNECT_ERR {0}, m_state: {1}, m_arConnect {2}",
                    DebugId, m_state_,
                    (m_arConnect == null) ? "(null)" : "(set)"));
                if (m_arConnect != null) {
                    Debug.Assert(m_state_ == State.New, eventId + ": m_state wanted: " + "New" + ", but was: " + m_state_);
                    Utils.MiscUtils.Trace_WriteLine("HandlePortEvent: connect failed.");
                    // It **could** be that the peer needs authentication/bonding,
                    // unfortunately there's no specific error returned in the CONNECT_ERR
                    // event, so we'll try a Bond and try OpenClient again if there's
                    // a Passcode provided.  Three states are possible:
                    // 1. No Passcode, so we didn't retry.
                    // 2. Something failed, return that error to the consumer.
                    // 3. We are retrying, so expect CONNECT/CONNECT_ERR.
                    Exception bondEx;
                    Debug.Assert(m_arConnect != null);
                    Debug.Assert(!m_arConnect.IsCompleted, "!m_arConnect.IsCompleted");
                    bool retrying = TryBondingIf_inLock(m_addressToConnect, m_ocScn, out bondEx);
                    if (retrying) {
                        Debug.Assert(bondEx == null);
                        sacAr = null;
                        sacEx = null;
                    } else if (bondEx != null) {
                        // Report the new failure.
                        sacAr = m_arConnect;
                        sacEx = bondEx;
                        m_arConnectCompleted = m_arConnect;
                        m_arConnect = null;
                    } else {
                        // Report the original failure.
                        sacAr = m_arConnect;
                        sacEx = FooSocketExceptions.CreateConnectFailed("PortCONNECT_ERR=" + eventId, socketErrorCode);
                        m_arConnectCompleted = m_arConnect;
                        m_arConnect = null;
                    }
                } else if (m_state_ == State.Closed) {
                    // On Win32, at least, we get CONNECT_ERR on calling Close
                    //Debug.Fail("Info: here be CONNECT_ERR after calling Close");
                    sacAr = null;
                    sacEx = null;
                } else {
                    Debug.Assert(m_state_ == State.Connected
                            || m_state_ == State.PeerDidClose, // We've seen CONNECT_ERR occur twice
                        DebugId + ": " +
                        eventId + ": m_state wanted: " + "Connected" + ", but was: " + m_state_);
                    Utils.MiscUtils.Trace_WriteLine("HandlePortEvent: closed when open.");
                    CloseInternal(out allRead, out allWrite);
                    sacAr = null;
                    sacEx = null;
                }
            }//lock
            if (sacAr != null)
                sacAr.SetAsCompleted(sacEx, AsyncResultCompletion.MakeAsync);
            AbortIf_withPeerCloseAndEof(allRead, allWrite);
        }

        /// <summary>
        /// Used: 1. when we get CONNECT_ERR from the stack, and POSSIBLY 2. when we close the 
        /// stream to do consumer timeout (SO_RCVTIMEO/etc).
        /// </summary>
        /// <param name="allRead">Out: to call <see cref="M:InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.AbortIf(System.Collections.Generic.IList{InTheHand.Net.AsyncResult{System.Int32,InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.BeginReadParameters}}, System.Collections.Generic.IList{InTheHand.Net.AsyncNoResult{InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.BeginReadParameters}})"/>
        /// on.</param>
        /// <param name="allWrite">Out: to call <see cref="M:InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.AbortIf(System.Collections.Generic.IList{InTheHand.Net.AsyncResult{System.Int32,InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.BeginReadParameters}}, System.Collections.Generic.IList{InTheHand.Net.AsyncNoResult{InTheHand.Net.Bluetooth.Widcomm.WidcommRfcommStream.BeginReadParameters}})"/>
        /// on.</param>
        private void CloseInternal(out ReadAsyncResult[] allRead, out WriteAsyncResult[] allWrite)
        {
            Debug.WriteLine(DateTime.Now.TimeOfDay.ToString()
                + ": RemovePort from CloseInternal (state: " + m_state_ + ")");
            RemovePortRecords();
            // For Listener an old port (previously accepted and closed by
            // the peer) is re-used to accept a new connection even when other
            // ports have been create'd/OpenServer'd, so close immmediately.
            if (m_state_ != State.PeerDidClose) { // Don't know whether its ok to call twice...
                // i.e. Debug.Assert(m_state_ == State.Connected);
                // TODO !! Make this DoPortClose(disposing: true)
                //   Was mistakenly set to false at first use in r62686.
                DoPortClose(false);
            }
            // No information whether this is a hard or clean close! :-(
            m_state_ = State.PeerDidClose;
            if (GetPendingReceiveDataLength() == 0) {
                // Must signal EoS to release any pending Reads for which no data will arrive.
                ClearAllReadRequests_inLock(out allRead);
            } else {
                allRead = null;
            }
            ClearAllWriteRequests_inLock(out allWrite);
        }

        /// <summary>
        /// Close the connection from the network/stack side (not from the consumer side).
        /// </summary>
        /// -
        /// <remarks>
        /// <para>When we call Close the object is disposed and outstanding and
        /// new operations fail with ObjectDisposedException.  This method
        /// instead closes the connection from the network/stack side and thus
        /// operations fail with an IO error etc.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Aborting, don't want errors.")]
        internal void CloseInternalAndAbort_willLock()
        {
            ReadAsyncResult[] allRead; WriteAsyncResult[] allWrite;
            AsyncResultNoResult connectToAbort = null;
            lock (_lockKey) {
                CloseInternal(out allRead, out allWrite);
                if (m_arConnect != null) {
                    connectToAbort = m_arConnect;
                    m_arConnectCompleted = m_arConnect;
                    m_arConnect = null;
                }
            }
            AbortIf_withPeerCloseAndEof(allRead, allWrite);
            if (connectToAbort != null) {
                //const int SocketError_Interrupted = 10004;
                //const int SocketError_NetworkDown = 10050;
                try {
                    connectToAbort.SetAsCompleted(new SocketException((int)SocketError.NetworkDown),
                        AsyncResultCompletion.MakeAsync);
                } catch (Exception ex) {
                    Utils.MiscUtils.Trace_WriteLine("Port CloseInternal, SaC SocketError_NetworkDown ex: " + ex);
                    Debug.Fail("Port CloseInternal, SaC SocketError_NetworkDown ex: " + ex);
                }
            }//if
        }

        /// <summary>
        /// DEPRECATED, should return false.
        /// </summary>
        /// <returns>Whether Bonding was attempted and thus the connect should be retried.
        /// </returns>
        protected abstract bool TryBondingIf_inLock(BluetoothAddress addressToConnect,
            int ocScn, out Exception err);

        //--------------------------------------------------------------
        void ClearAllReadRequests_inLock(out ReadAsyncResult[] all)
        {
            all = m_arReceiveList.ToArray();
            m_arReceiveList.Clear();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Aborting, don't want errors.")]
        void AbortReads_WithEof(IList<ReadAsyncResult> all)
        {
            foreach (ReadAsyncResult ar in all) {
                try {
                    if (!ar.IsCompleted)
                        ar.SetAsCompleted(0, AsyncResultCompletion.MakeAsync);
                } catch {
                }
            }
        }

        void ClearAllRequests_inLock(out ReadAsyncResult[] allRead, out WriteAsyncResult[] allWrite)
        {
            ClearAllReadRequests_inLock(out allRead);
            ClearAllWriteRequests_inLock(out allWrite);
        }

        void AbortIf_withPeerCloseAndEof(IList<ReadAsyncResult> allRead, IList<WriteAsyncResult> allWrite)
        {
            if (allRead != null)
                AbortReads_WithEof(allRead);
            if (allWrite != null)
                AbortWrites_AsPeerClose(allWrite);
        }

        //--------------------------------------------------------------
        protected void HandlePortReceive(byte[] buffer)
        {
            lock (_lockKey) {
                // Put it on the queue.
                m_receivedData.Enqueue(buffer);
                System.Diagnostics.Debug.Assert(m_amountInReadBuffers >= 0);
                m_amountInReadBuffers += buffer.Length;
                // TODO Check high-water mark and set SetFlowControl.
            }//lock
            // Free any waiting reads.
            int readLen;
            AsyncResult<int, BeginReadParameters> ar; // Call SetAsCompleted outside the lock.
            while (true) {
                lock (_lockKey) {
                    if (m_arReceiveList.Count == 0 || GetPendingReceiveDataLength() == 0)
                        break;
                    ar = m_arReceiveList.Dequeue();
                    BeginReadParameters args = ar.BeginParameters;
                    readLen = ReturnSomeReceivedData_MustBeInLock(args.buffer, args.offset, args.count);
                }//lock
                ar.SetAsCompleted(readLen, AsyncResultCompletion.MakeAsync);
            }//while
        }

        private int GetPendingReceiveDataLength()
        {
            if (m_receivedData.Count == 0)
                return 0;
            else {
                int firstLen, firstOffset;
                FirstReceivedBlockInfo_(out firstLen, out firstOffset);
                return firstLen;
            }
        }

        private int ReturnSomeReceivedData_MustBeInLock(byte[] buffer, int offset, int count)
        {
            // Use the data; returning as much of the first block that will fit.
            // Possible TO-DO: Or we could return data from many of the blocks to fill the buffer.
            int firstLen, firstOffset;
            FirstReceivedBlockInfo_(out firstLen, out firstOffset);
            int returnedLen;
            if (firstLen <= count) { // Use all of the first block and remove it.
                byte[] data = m_receivedData.Dequeue();
                Array.Copy(data, m_receivedDataFirstBlockOffset, buffer, offset, firstLen);
                m_receivedDataFirstBlockOffset = 0;
                Debug.Assert(firstLen != 0, "Mustn't return 0 except at closed.");
                returnedLen = firstLen;
            } else { // Use some of the first block.
                byte[] data = m_receivedData.Peek();
                int lenToRead = Math.Min(count, firstLen);
                Array.Copy(data, firstOffset, buffer, offset, lenToRead);
                m_receivedDataFirstBlockOffset += lenToRead;
                Debug.Assert(m_receivedDataFirstBlockOffset <= data.Length, "Should not leave an empty block.");
                Debug.Assert(lenToRead != 0 || count == 0, "Mustn't return 0 except at closed, or when requested.");
                returnedLen = lenToRead;
            }
            m_amountInReadBuffers -= returnedLen;
            System.Diagnostics.Debug.Assert(m_amountInReadBuffers >= 0, "m_amountInReadBuffers: " + m_amountInReadBuffers);
            // TODO Check low-water mark and clear SetFlowControl
            return returnedLen;
        }

        private void FirstReceivedBlockInfo_(out int length, out int offset)
        {
            Debug.Assert(m_receivedData.Count != 0);
            byte[] data = m_receivedData.Peek();
            offset = m_receivedDataFirstBlockOffset;
            length = data.Length - offset;
            Debug.Assert(length != 0, "Found empty block on receive list.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // TODO Add parameter checking
            EnsureOpenForRead();
            IAsyncResult ar;
            lock (_lockKey) {
                if (m_receivedData.Count != 0) {
                    return ReturnSomeReceivedData_MustBeInLock(buffer, offset, count);
                } else {
                    // No pending data, due to closed?  Otherwise wait.
                    if (m_state_ == State.PeerDidClose)
                        return 0;
                    ar = this.BeginRead(buffer, offset, count, null, null);
                    // We must exit the lock to allow new data to arrive!
                }
            }//lock
            Debug.Assert(ar != null, "Should be here only after calling BeginRead");
            if (!IsInfiniteTimeout(_readTimeout)) {
                ApplyTimeout(ar, _readTimeout, m_arReceiveList);
            }
            return this.EndRead(ar);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            // TODO Add parameter checking
            EnsureOpenForRead();
            AsyncResult<int, BeginReadParameters> sacAr; // Call SetAsCompleted outside the lock.
            int readLen;
            lock (_lockKey) {
                BeginReadParameters args = new BeginReadParameters(buffer, offset, count);
                AsyncResult<int, BeginReadParameters> ar
                    = new AsyncResult<int, BeginReadParameters>(callback, state, args);
                if (m_receivedData.Count == 0) {
                    switch (m_state_) {
                        case State.Connected:
                            // Wait for some more data then.
                            m_arReceiveList.Enqueue(ar);
                            return ar;
                        default: // Expect only "PeerDidClose", others blocked by EnsureOpen.
                            Debug.Assert(m_state_ == State.PeerDidClose, "Unexpected State: " + m_state_);
                            // So, we're at EoF -> Completed Synchronously
                            sacAr = ar;
                            readLen = 0;
                            break;
                    }
                } else {
                    // Data available -> Completed Synchronously
                    readLen = ReturnSomeReceivedData_MustBeInLock(args.buffer, args.offset, args.count);
                    sacAr = ar;
                }
            }//lock
            Debug.Assert(sacAr != null, "Only get here when want to call SetAsCompleted!");
            sacAr.SetAsCompleted(readLen, true);
            return sacAr;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            // (Can't lock here as that would block the callback methods).
            AsyncResult<int, BeginReadParameters> ar
                = (AsyncResult<int, BeginReadParameters>)asyncResult;
            int readLen = ar.EndInvoke();
            Debug.Assert(!m_arReceiveList.Contains(ar), "Should have cleaned up outstanding IAsyncResult list");
            return readLen;
        }

        internal sealed class BeginReadParameters
        {
            //Unused: public readonly int startTicks = Environment.TickCount;
            public byte[] buffer;
            public int offset;
            public int count;

            public BeginReadParameters(byte[] buffer, int offset, int count)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }
        }

        public virtual bool DataAvailable { get { return AmountInReadBuffers > 0; } }

        internal int AmountInReadBuffers
        {
            get
            {
                lock (_lockKey) {
                    System.Diagnostics.Debug.Assert(m_amountInReadBuffers >= 0);
                    return m_amountInReadBuffers;
                }
            }
        }

        //--------
        int _readTimeout = Timeout.Infinite;
        int _writeTimeout = Timeout.Infinite;

        public override bool CanTimeout { get { return true; } }

        public override int ReadTimeout
        {
            get { return _readTimeout; }
            set { _readTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _writeTimeout; }
            set { _writeTimeout = value; }
        }

        internal static bool IsInfiniteTimeout(int timeout)
        {
            if (timeout == Timeout.Infinite)
                return true;
            if (timeout == 0)
                return true;
            return false;
        }

        private void ApplyTimeout<TAsyncResult>(IAsyncResult ar, int timeout,
            Queue<TAsyncResult> queue)
            where TAsyncResult : AsyncResultNoResult
        {
            Debug.Assert(timeout != Timeout.Infinite, "Called with invalid timeout (-1).");
            Debug.Assert(timeout != 0, "Called with invalid timeout (0).");
            TAsyncResult sacAr = null;
            ReadAsyncResult[] allRead = null;
            WriteAsyncResult[] allWrite = null;
            int watchdog = 0;
            while (true) { // See comment in loop-else-else
                Debug.Assert(sacAr == null);
                bool signalled = ar.AsyncWaitHandle.WaitOne(timeout, false);
                if (signalled) {
                    break;
                } else {
                    lock (_lockKey) {
                        if (queue.Count == 0) {
                            // The operation timed-out but completed before we
                            // got inside the lock...
                            // Either there were two (or more) operations in the
                            // queue and they timed out together, so the first
                            // one aborted all of them.
                            // Or possibly one timing-out operation completed
                            // successfully just *after* we timed it out, but
                            // just *before* we entered the lock.
                            Debug.WriteLine("Queue empty in ApplyTimeout (isc: " + ar.IsCompleted + ")");
                            //A bit racy--Debug.Assert(ar.IsCompleted, "NOT ar.IsCompleted but queue empty!");
                            sacAr = null;
                            break;
                        } else {
                            TAsyncResult peekFirstAr = queue.Peek();
                            if (peekFirstAr == ar) {
                                sacAr = queue.Dequeue();
                                Debug.Assert(sacAr == ar, "How did a different AR get to the front of the queue?!?");
                                CloseInternal(out allRead, out allWrite);
                                break;
                            } else {
                                // There are waiting Reads in the queue ahead of us
                                // and we can't remove ours from behind them, so we
                                // just have to wait some more...
                                sacAr = null;
                            }
                        }
                    }//lock
                }
                ++watchdog;
                //if (watchdog > 10) {
                //    Debug.Fail("Looping in DoTimeout!");
                //    break;
                //}
            }//while
            if (sacAr != null) { // Set completion with error.
                const int WSAETIMEDOUT = 10060;
                Exception ex0 = new SocketException(WSAETIMEDOUT);
                sacAr.SetAsCompleted(new IOException(ex0.Message, ex0), true);
                AbortIf_withPeerCloseAndEof(allRead, allWrite);
            } else {
                Debug.Assert(allRead == null, "inconsistent allRead");
                Debug.Assert(allWrite == null, "inconsistent allWrite");
            }
        }

        //--------------------------------------------------------------

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void FreePendingWrites()
        {
            // Release the async operations outside the lock.
            Queue<WriteAsyncResult> sacArQueue = new Queue<WriteAsyncResult>();
            WriteAsyncResult sacErroredLast = null;
            Exception sacEx = null;
            lock (_lockKey) {
                int loops = 0;
                while (m_arWriteQueue.Count != 0) {
                    ++loops;
                    WriteAsyncResult arPeek = m_arWriteQueue.Peek();
                    BeginReadParameters work = arPeek.BeginParameters;
                    int lenWritten;
                    try {
                        lenWritten = PortWrite(work.buffer, work.offset, work.count);
                    } catch (Exception ex) {
                        if (!TestUtilities.IsUnderTestHarness())
                            Debug.Fail("Write failed at FPW -- Not seen (on Widcomm) in reality");
                        ImplicitPeerClose();
                        SetAsDisconnected();
                        WriteAsyncResult ar = m_arWriteQueue.Dequeue();
                        sacErroredLast = ar;
                        sacEx = ex;
                        break;
                    }
                    if (lenWritten == work.count) {
                        WriteAsyncResult ar = m_arWriteQueue.Dequeue();
                        sacArQueue.Enqueue(ar);
                    } else {
                        Debug.Assert(lenWritten < work.count, "lenWritten: " + lenWritten + ", work.count: " + work.count);
                        work.count -= lenWritten;
                        work.offset += lenWritten;
                        break; // Need to wait for the next event!
                    }
                }//while
                if (m_arWriteQueue.Count == 0 && m_writeEmptied != null) {
                    m_writeEmptied.Set();
                }
                //
                if (loops == 0) {
                    // DEBUG test-coverage
                } else if (loops == 1) {
                    // DEBUG test-coverage
                } else {
                    // DEBUG test-coverage
                }
            }//lock
            while (sacArQueue.Count != 0) {
                WriteAsyncResult ar = sacArQueue.Dequeue();
                ar.SetAsCompleted(null, AsyncResultCompletion.MakeAsync);
            }
            Debug.Assert((sacErroredLast == null) == (sacEx == null));
            if (sacErroredLast != null) {
                sacErroredLast.SetAsCompleted(sacEx, AsyncResultCompletion.MakeAsync);
            }
        }

        void ClearAllWriteRequests_inLock(out WriteAsyncResult[] all)
        {
            all = m_arWriteQueue.ToArray();
            m_arWriteQueue.Clear();
        }

        // TODO (?Use distinct errors for different abort reasons network/timeout/radio off).
        void AbortWrites_AsPeerClose(IList<WriteAsyncResult> all)
        {
            foreach (WriteAsyncResult ar in all) {
                ar.SetAsCompleted(new IOException(WrappingIOExceptionMessage,
                    FooSocketExceptions.ConnectionIsPeerClosed()), AsyncResultCompletion.MakeAsync);
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            EnsureOpenForWrite();
            WriteAsyncResult sacAr; // SetAsComplete outside the lock.
            lock (_lockKey) {
                WriteAsyncResult ar;
                if (m_arWriteQueue.Count == 0) { // Try to complete the send immediately.
                    bool success = false;
                    int lenWritten;
                    try {
                        lenWritten = PortWrite(buffer, offset, count);
                        success = true;
                    } finally {
                        if (!success) {
                            if (!TestUtilities.IsUnderTestHarness())
                                Debug.Fail("Write failed at BW -- Not seen in reality");
                            ImplicitPeerClose();
                            SetAsDisconnected();
                        }
                    }
                    if (lenWritten == count) { // -> CompletedSynchronously
                        ar = new WriteAsyncResult(callback, state, null);
                        sacAr = ar;
                    } else { // Queue the remainder...
                        BeginReadParameters work = new BeginReadParameters(
                            buffer, offset + lenWritten, count - lenWritten);
                        Debug.Assert(work.offset < buffer.Length, "work.offset: " + work.offset + ", buffer.Length: " + buffer.Length);
                        Debug.Assert(work.count > 0, "work.count: " + work.count);
                        ar = new WriteAsyncResult(callback, state, work);
                        m_arWriteQueue.Enqueue(ar);
                        return ar;
                    }
                } else { //-----------------------
                    // Queue it all...
                    ar = new WriteAsyncResult(callback, state,
                        new BeginReadParameters(buffer, offset, count));
                    m_arWriteQueue.Enqueue(ar);
                    return ar;
                }
            }
            // Fall throught from complete write.
            sacAr.SetAsCompleted(null, true);
            return sacAr;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            bool success = false;
            try {
                WriteAsyncResult ar2 = (WriteAsyncResult)asyncResult;
                ar2.EndInvoke();
                success = true;
            } finally {
                if (!success) {
                    Debug.Assert(!LiveConnected, "EndWrite on !success !LiveConnected already");
                    Debug.Assert(!LiveConnected || !Connected, "EndWrite on !success !Connected already");
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            IAsyncResult ar = BeginWrite(buffer, offset, count, null, null);
            if (!IsInfiniteTimeout(_writeTimeout)) {
                ApplyTimeout(ar, _writeTimeout, m_arWriteQueue);
            }
            EndWrite(ar);
        }

        private int PortWrite(byte[] buffer, int offset, int count)
        {
            int totalLenWritten = 0;
            UInt16 lenAttemptedToWrite, lenWritten;
            // Try in 16KB chunks
            while (count > 0) {
                PortWriteMax16kb(buffer, offset, count, out lenAttemptedToWrite, out lenWritten);
                Debug.Assert(lenWritten <= lenAttemptedToWrite, "lenWritten<= lenToWrite ("
                    + lenWritten + "," + lenAttemptedToWrite + ")");
                Debug.Assert(lenWritten >= 0, "NOT +ve lenWritten: " + lenWritten);
                count -= lenWritten;
                offset += lenWritten;
                Debug.Assert(count >= 0, "count: " + count);
                Debug.Assert(offset <= buffer.Length, "offset: " + offset + ", buffer.Length: " + buffer.Length);
                totalLenWritten += lenWritten;
                //
                if (lenAttemptedToWrite != lenWritten)
                    break; // port.Write accepted only part!  The rest needs to be queued.
            }
            return totalLenWritten;
        }

        private void PortWriteMax16kb(byte[] buffer, int offset, int count, out UInt16 lenAttemptToWrite, out UInt16 lenWritten)
        {
            lenAttemptToWrite = (UInt16)Math.Min(count, UInt16.MaxValue);
            byte[] data;
            if (offset == 0 && lenAttemptToWrite == count && buffer.Length == lenAttemptToWrite) {
                // perf optimisation
                data = buffer;
            } else {
                data = new byte[lenAttemptToWrite];
                Array.Copy(buffer, offset, data, 0, lenAttemptToWrite);
            }
            //Log("m_port.Write");
            DoWrite(data, lenAttemptToWrite, out lenWritten);
            //Utils.MiscUtils.Trace_WriteLine("m_port.Write: len in: {0}, len out: {1}", lenAttemptToWrite, lenWritten);
            Debug.Assert(lenWritten <= lenAttemptToWrite, "lenWritten<= lenToWrite ("
                + lenWritten + "," + lenAttemptToWrite + ")");
        }

        protected abstract void DoWrite(byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written);

        public override void Flush()
        {
            // Can we do anything here?  We're not a buffered stream so there is 
            // no need to flush.  Any data in the write queue is there due to flow 
            // control, we can only send it when the stack signals that its ready.
        }

        //--------
        public override long Length { get { throw NewNotSupportedException(); } }

        public override long Position
        {
            get { throw NewNotSupportedException(); }
            set { throw NewNotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw NewNotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw NewNotSupportedException();
        }

        //--------
        private static Exception NewNotSupportedException()
        {
            // no message, for NETCF
            throw new NotSupportedException();
        }

    }//class

    static class FooSocketExceptions
    {
        internal static Exception ConnectionIsPeerClosed()
        {
            return CommonSocketExceptions.ConnectionIsPeerClosed();
        }


        internal static Exception CreateConnectFailed(string p, int? socketErrorCode)
        {
            return CommonSocketExceptions.CreateConnectFailed(p, socketErrorCode);
        }
    }

}

// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.CommonBluetoothListener
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using AsyncResult_BABc = InTheHand.Net.AsyncResult<InTheHand.Net.Bluetooth.Factory.CommonRfcommStream>;
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace InTheHand.Net.Bluetooth.Factory
{
    abstract class CommonBluetoothListener : IBluetoothListener
    {
        readonly BluetoothFactory m_factory;
        // Note we don't currently clear this on Stop+Start, maybe we should??
        volatile Exception _brokenE; // Error occured on starting a new port etc!
        BluetoothEndPoint m_requestedLocalEP;
        BluetoothEndPoint m_liveLocalEP;
        private ServiceRecord m_serviceRecord;
        private bool m_manualServiceRecord;
        bool m_ServiceRecordAdded;
        string m_serviceName;
        protected/*HACK protected*/ bool m_authenticate, m_encrypt;
        // 1. We have a queue of Accept-IAsyncResults to handle when receive 
        // (Begin)AcceptBluetoothClient when there is no new connection(s) queued.
        // It's likely that there will never be more than one such pending Accept 
        // call so just store one pending accept and return error on any others?
        //
        // Also:
        // 2. We have a queue of already accepted connections.
        // 3. Have one, or a queue, of prepared ports to accept new connections.
        // [Restricted to *one* at the moment, as it seems to work with multiple
        // but diagnostics report odd behaviour].
        // Presumable the sum of the lengths of the queues #2 and #3 should equal
        // 'backlog'.
        Queue<AsyncResult_BABc> m_callers = new Queue<AsyncResult_BABc>();
        Queue<AcceptedPort> m_accepted = new Queue<AcceptedPort>();
        Dictionary<CommonRfcommStream, CommonRfcommStream> m_listening = new Dictionary<CommonRfcommStream, CommonRfcommStream>();
        object m_key = new object();
        //
        const int DefaultBacklog = 1;
        const int RestrictPortCount = 1;
        int m_backlog = -1;
        //
#if DEBUG && ! PocketPC
        string _creationStackTrace;
#endif

        //--------
        class AcceptedPort
        {
            public readonly CommonRfcommStream _port;
            public readonly Exception _error;

            public AcceptedPort(CommonRfcommStream port, Exception error)
            {
                if (port == null)
                    throw new ArgumentNullException("port");
                _port = port;
                _error = error;
                if (_error != null) { // COVERAGE
                }
            }
        }

        //--------
        protected CommonBluetoothListener(BluetoothFactory factory)
        {
#if DEBUG && ! PocketPC
            _creationStackTrace = Environment.StackTrace;
#endif
            m_factory = factory;
            GC.SuppressFinalize(this); // Finalization only needed for IRfcommIf.
        }

        //--------
        public void Construct(BluetoothEndPoint localEP)
        {
            if (localEP == null)
                throw new ArgumentNullException("localEP");
            if (localEP.Address != BluetoothAddress.None)
                throw new NotSupportedException("Don't support binding to a particular local address/port.");
            m_requestedLocalEP = (BluetoothEndPoint)localEP.Clone();
        }

        public void Construct(BluetoothAddress localAddress, Guid service)
        {
            Construct(new BluetoothEndPoint(localAddress, service));
        }

        public void Construct(Guid service)
        {
            Construct(new BluetoothEndPoint(BluetoothAddress.None, service));
        }

        //----
        public void Construct(BluetoothEndPoint localEP, ServiceRecord sdpRecord)
        {
            Construct(localEP);
            m_serviceRecord = sdpRecord;
            m_manualServiceRecord = true;
        }

        public void Construct(BluetoothAddress localAddress, Guid service, ServiceRecord sdpRecord)
        {
            Construct(new BluetoothEndPoint(localAddress, service), sdpRecord);
        }

        public void Construct(Guid service, ServiceRecord sdpRecord)
        {
            Construct(new BluetoothEndPoint(BluetoothAddress.None, service), sdpRecord);
        }

        //----
        // Parse the supplied raw record and call the corresponding ServiceRecord ctor.
        //

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "channelOffset")]
        private static ServiceRecord ParseRaw(byte[] sdpRecord, int channelOffset)
        {
            ServiceRecordParser parser = new ServiceRecordParser();
            Debug.Assert(!parser.SkipUnhandledElementTypes);
            ServiceRecord rcd = parser.Parse(sdpRecord);
            return rcd;
        }

        public void Construct(BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset)
        {
            Construct(localEP, ParseRaw(sdpRecord, channelOffset));
        }

        public void Construct(BluetoothAddress localAddress, Guid service,
            byte[] sdpRecord, int channelOffset)
        {
            Construct(localAddress, service, ParseRaw(sdpRecord, channelOffset));
        }

        public void Construct(Guid service, byte[] sdpRecord, int channelOffset)
        {
            Construct(service, ParseRaw(sdpRecord, channelOffset));
        }


        //----------------
        public void Start()
        {
            Start(3 + 1);
        }

        public void Start(int backlog)
        {
            BluetoothEndPoint liveLocalEP;
            SetupListener(m_requestedLocalEP, out liveLocalEP);
            m_liveLocalEP = liveLocalEP;
            m_backlog = Math.Max(1, Math.Min(backlog, 10));
            lock (m_key) {
                StartEnoughNewListenerPort_inLock();
            }
            if (_brokenE != null) {
                throw _brokenE;
            }
            AddServiceRecord(ref m_serviceRecord, m_liveLocalEP.Port,
                m_requestedLocalEP.Service, m_serviceName);
            m_ServiceRecordAdded = true;
        }

        protected virtual void VerifyPortIsInRange(BluetoothEndPoint bep)
        {
            if (bep.Port < BluetoothEndPoint.MinScn || bep.Port > BluetoothEndPoint.MaxScn)
                throw new ArgumentOutOfRangeException("bep", "Channel Number must be in the range 1 to 30.");
        }

        private void SetupListener(BluetoothEndPoint bep, out BluetoothEndPoint liveLocalEP)
        {
            if (bep == null)
                throw new ArgumentNullException("bep");
            int requestedScn;
            if ((bep.Port == 0 || bep.Port == -1)) {
                // Let the stack choose one
                requestedScn = 0;
            } else {
                // Requesting a specific port, check it's valid.
                VerifyPortIsInRange(bep);
                requestedScn = bep.Port;
            }
            //
            SetupListener(bep, requestedScn, out liveLocalEP);
            Debug.Assert(liveLocalEP != null, "null: out liveLocalEP!!!");
            Debug.WriteLine("Listening on " + liveLocalEP.ToString());
        }

        private void AddServiceRecord(ref ServiceRecord fullServiceRecord,
            int livePort, Guid serviceClass, string serviceName)
        {
            //Debug.Assert(liveLocalEP.Service.Equals(m_requestedLocalEP.Service));
            if (fullServiceRecord != null) {
                AddCustomServiceRecord(ref fullServiceRecord, livePort);
            } else {
                AddSimpleServiceRecord(out fullServiceRecord, livePort, serviceClass, serviceName);
            }
        }

        protected abstract void SetupListener(BluetoothEndPoint bep, int scn, out BluetoothEndPoint liveLocalEP);

        protected abstract void AddCustomServiceRecord(ref ServiceRecord fullServiceRecord,
            int livePort);

        protected abstract void AddSimpleServiceRecord(out ServiceRecord fullServiceRecord,
            int livePort, Guid serviceClass, string serviceName);

        //--------
        protected abstract bool IsDisposed { get; }

        private bool IsListening
        {
            get { return m_liveLocalEP != null && !IsDisposed; }
        }

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "This method is equivalent to Dispose")]
        public void Stop() // In place of Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // We call GC.ReRegisterForFinalize is Start is called again.
        }

        ~CommonBluetoothListener()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        void Dispose(bool disposing)
        {
            Debug.WriteLine("PortAccepted Dispose(" + disposing + ")");
            AsyncResult_BABc[] callersToAbort;
            CommonRfcommStream[] listenersToAbort;
            AcceptedPort[] acceptedToAbort;
            try {
                OtherDispose(disposing);
                Debug.Assert(IsDisposed, "NOT IsDisposed, but just set it so!");
            } finally {
                lock (m_key) {
                    ClearAllCallers_inLock(out callersToAbort);
                    ClearAllListeners_inLock(out listenersToAbort);
                    ClearAllAccepted_inLock(out acceptedToAbort);
                }//lock
                Abort(callersToAbort);
                if (disposing) {
                    OtherDisposeMore();
                    Abort(listenersToAbort);
                    Abort(acceptedToAbort);
                }
            }// finally
        }

        //T0-DO rename like CommonBtCli
        protected abstract void OtherDispose(bool disposing);
        protected abstract void OtherDisposeMore();

        void ClearAllCallers_inLock(out AsyncResult_BABc[] all)
        {
            all = m_callers.ToArray();
            m_callers.Clear();
        }

        void ClearAllListeners_inLock(out CommonRfcommStream[] all)
        {
            System.Collections.ICollection all0 = m_listening.Values;
            all = new CommonRfcommStream[all0.Count];
            all0.CopyTo(all, 0);
            m_listening.Clear();
        }

        void ClearAllAccepted_inLock(out AcceptedPort[] all)
        {
            all = m_accepted.ToArray();
            m_accepted.Clear();
        }

        void Abort(IList<AsyncResult_BABc> all)
        {
            foreach (AsyncResult_BABc ar in all) {
                ar.SetAsCompleted(new ObjectDisposedException("BluetoothListener"), false);
            }
        }

        void Abort(IList<CommonRfcommStream> all)
        {
            foreach (CommonRfcommStream ar in all) {
                ar.Close();
            }
        }

        void Abort(IList<AcceptedPort> all)
        {
            foreach (AcceptedPort ar in all) {
                if (ar._port != null)
                    ar._port.Close();
            }
        }

        //--------
        public bool Pending()
        {
            lock (m_key) {
                return m_accepted.Count > 0;
            }
        }

        public System.Net.Sockets.Socket Server
        {
            get { throw new NotSupportedException("This stack does not use sockets."); }
        }

        //--------
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void StartEnoughNewListenerPort_inLock()
        {
            if (IsDisposed) {
                Debug.Fail("INFO expected to have noticed this elsewhere.");
                return;
            }
            int createdNew = 0;
            while (m_listening.Count + m_accepted.Count < m_backlog) {
                if (m_listening.Count >= RestrictPortCount) {
                    break;
                }
                //
                int oldLC = m_listening.Count;
                try {
                    _StartOneNewListenerPort_inLock();
                } catch (Exception ex) {
                    _brokenE = ex;
                    Utils.MiscUtils.Trace_WriteLine("Error calling StartEnoughNewListenerPort_inLock ex: " + ex);
                    break;
                }
                if (m_listening.Count != oldLC + 1) {
                    Debug.Fail("huh");
                    break;
                }
                ++createdNew;
            }
            if (_brokenE != null) {
                AsyncResult_BABc[] callersToAbort;
                ClearAllCallers_inLock(out callersToAbort);
                foreach (var cur in callersToAbort) {
                    cur.SetAsCompleted(new System.Reflection.TargetInvocationException(
                        _brokenE), AsyncResultCompletion.MakeAsync);
                }
                return;
            }
            Debug.WriteLine("Started " + createdNew + " new port(s).");
            Debug.Assert(m_listening.Count + m_accepted.Count == m_backlog
                    || m_listening.Count == RestrictPortCount,
                $"didn't reach backlog (#L + #A = {m_listening.Count} + {m_accepted.Count} = {m_listening.Count + m_accepted.Count} != {m_backlog})");
        }

        void _StartOneNewListenerPort_inLock()
        {
            //Console.WriteLine("Starting a new listening port...");
            CommonRfcommStream port = GetNewPort();
            m_listening.Add(port, port);
            IAsyncResult ar = port.BeginAccept(m_liveLocalEP, null, PortAcceptCallback, port);
            Debug.Assert(ar is AsyncResultNoResult, "assert type");
            Debug.WriteLine(
                $"StartOneNewListenerPort {port.DebugId}.");
        }

        protected abstract CommonRfcommStream GetNewPort();

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "will rethrow")]
        void PortAcceptCallback(IAsyncResult ar)
        {
            AsyncResult_BABc arSac; // Call SetAsCompleted outside the lock.
            Exception exSac;
            AsyncResultNoResult ar2 = (AsyncResultNoResult)ar;
            CommonRfcommStream port = (CommonRfcommStream)ar2.AsyncState;
            lock (m_key) {
                try {
                    port.EndAccept(ar2);
                    exSac = null;
                } catch (Exception ex) {
                    if (IsDisposed) {
                        Debug.WriteLine("INFO PortAccepted was an error (Stopped: true): " + ex.Message);
                    } else {
                        Debug.WriteLine("PortAccepted was an error: " + ex.ToString());
                    }
                    if (IsDisposed) {
                        return;
                    }
                    exSac = ex;
                }
                bool found = m_listening.Remove(port);
                Debug.Assert(found, "NOT found");
                if (m_callers.Count > 0) {
                    // Release one Accept immediately
                    Debug.Assert(m_accepted.Count == 0, "Why clients waiting when also pending accepted!?!");
                    arSac = m_callers.Dequeue();
                    Debug.WriteLine("PortAccepted Dequeued a caller");
                } else {
                    // Queue the new port for later accept call
                    if (exSac != null) { // DEBUG
                    }
                    m_accepted.Enqueue(new AcceptedPort(port, exSac));
                    arSac = null;
                    Debug.WriteLine("PortAccepted Enqueued");
                }
                StartEnoughNewListenerPort_inLock();
            }// lock
            // Raise events if any.
            var args = new RaiseAcceptParams
            {
                arSac = arSac,
                exSac = exSac,
                port = port
            };
            ThreadPool.QueueUserWorkItem(RaiseAccept, args);
        }

        class RaiseAcceptParams
        {
            internal AsyncResult_BABc arSac { get; set; }
            internal Exception exSac { get; set; }
            internal CommonRfcommStream port { get; set; }
        }

        private static void RaiseAccept(object state)
        {
            var args = (RaiseAcceptParams)state;
            if (args.arSac != null) {
                if (args.exSac != null) {
                    args.arSac.SetAsCompleted(args.exSac, false);
                } else {
                    args.arSac.SetAsCompleted(args.port, false);
                }
            }
        }

        //----
        public IBluetoothClient AcceptBluetoothClient()
        {
            IAsyncResult ar = BeginAcceptBluetoothClient(null, null);
            return EndAcceptBluetoothClient(ar);
        }

        public IAsyncResult BeginAcceptBluetoothClient(AsyncCallback callback, object state)
        {
            AsyncResult_BABc ar = new AsyncResult_BABc(callback, state);
            AsyncResult_BABc arSac;
            AcceptedPort port;
            Exception error;
            lock (m_key) {
                if (m_accepted.Count > 0) {
                    // Complete immediately from the accepted queue.
                    port = m_accepted.Dequeue();
                    arSac = ar;
                    error = null;
                    Debug.WriteLine("BeginAccept Dequeued a Port");
                    StartEnoughNewListenerPort_inLock();
                } else {
                    port = null;
                    if (!IsListening) {
                        arSac = ar;
                        const string MsgFromTcpLnsr = "Not listening. You must call the Start() method before calling this method.";
                        error = new InvalidOperationException(MsgFromTcpLnsr);
                    } else if (_brokenE != null) {
                        arSac = ar;
                        error = _brokenE;
                    } else {
                        // Queue for new accept.
                        m_callers.Enqueue(ar);
                        arSac = null;
                        error = null;
                        Debug.WriteLine("BeginAccept Enqueued");
                    }
                }
            }// lock
            if (arSac != null) {
                if (error != null) {
                    arSac.SetAsCompleted(error, true);
                } else if (port._error != null) {
                    // Just complete the port as before, lets do some more testing
                    // before we re-raise the error.
                    Debug.Fail("BABC should raise port._error: " + port._error);
                    //TODO ! arSac.SetAsCompleted(port._error, true);
                    arSac.SetAsCompleted(port._port, true);
                } else {
                    arSac.SetAsCompleted(port._port, true);
                }
            }
            return ar;
        }

        public IBluetoothClient EndAcceptBluetoothClient(IAsyncResult asyncResult)
        {
            AsyncResult_BABc ar2 = (AsyncResult_BABc)asyncResult;
            CommonRfcommStream strm = ar2.EndInvoke();
            IBluetoothClient cli0 = GetBluetoothClientForListener(strm);
            Debug.Assert(cli0.Connected, "cli0.Connected");
            return cli0;
        }

        protected virtual IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream strm)
        {
            IBluetoothClient cli0 = m_factory.DoGetBluetoothClientForListener(strm);
            return cli0;
        }

        //----
        public System.Net.Sockets.Socket AcceptSocket()
        {
            throw new NotSupportedException("This stack does not use Sockets.");
        }

        public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
        {
            throw new NotSupportedException("This stack does not use Sockets.");
        }

        public System.Net.Sockets.Socket EndAcceptSocket(IAsyncResult asyncResult)
        {
            throw new NotSupportedException("This stack does not use Sockets.");
        }

        //--------
        #region IBluetoothListener Members

        public BluetoothEndPoint LocalEndPoint
        {
            get { return m_liveLocalEP; } // TODO (Active check?)
        }

        public ServiceClass ServiceClass
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
            set { /* Not supported -- no API on Widcomm. */ }
        }

        public string ServiceName
        {
            get { return m_serviceName; }
            set
            {
                if (m_ServiceRecordAdded) { // TODO Test me, was: (m_sdpService != null)
                    throw new InvalidOperationException("Can not change ServiceName when started.");
                }
                if (m_manualServiceRecord) {
                    throw new InvalidOperationException("ServiceName may not be specified when a custom Service Record is being used.");
                }
                m_serviceName = value;
            }
        }

        public ServiceRecord ServiceRecord
        {
            get { return m_serviceRecord; }
        }

        public bool Authenticate
        {
            get { return m_authenticate; }
            set
            {
                //Debug.Assert(!IsActive, "Changing Authenticate after Started.  Throw??");
                m_authenticate = value;
            }
        }

        public bool Encrypt
        {
            get { return m_encrypt; }
            set
            {
                //Debug.Assert(!IsActive, "Changing Encrypt after Started.  Throw??");
                m_encrypt = value;
            }
        }

        public void SetPin(string pin)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #endregion
    }
}

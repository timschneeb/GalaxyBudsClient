// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2008-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using Utils;

namespace InTheHand.Net.Bluetooth.Factory
{
    internal abstract class BluetoothConnector
    {
        //[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Is used in the DEBUG build.")]
        protected //??
            bool m_disposed;
        IUsesBluetoothConnectorImplementsServiceLookup _parent;
        AsyncResultNoResult m_arConnect;
        //
        protected //??
            int? _remotePort;  // Only known in the client case, not in the BtLsnr->BtCli case.
        protected //??
            BluetoothEndPoint m_remoteEndPoint;

        //----
        internal BluetoothConnector(IUsesBluetoothConnectorImplementsServiceLookup parent)
        {
            _parent = parent;
        }

        protected //???
            void Connect_SetAsCompleted_CompletedSyncFalse(AsyncResultNoResult arConnect_Debug, Exception ex)
        {
            // Read state to check not already completed (is null).  And set it to null always.
            AsyncResultNoResult arOrig = Interlocked.Exchange(ref m_arConnect, null);
            if (arOrig == null) {
                // We use m_arConnect being null as an indication that it is already SetAsCompleted.
                Debug.Assert(m_disposed, "arConnect is already IsCompleted but NOT m_cancelled");
                Debug.Assert(arConnect_Debug == null || arConnect_Debug.IsCompleted, "NOT arConnect.IsCompleted: How!? Different instances?");
#if CODE_ANALYSIS
                Trace.Assert(m_disposed, "arConnect is already IsCompleted but NOT m_cancelled");
#endif
            } else {
                Debug.Assert(arConnect_Debug == null || arConnect_Debug == arOrig, "arConnect != m_arConnect: should only be one instance!");
                // Set!
                var args = new RaiseConnectParams { arOrig = arOrig, ex = ex };
                ThreadPool.QueueUserWorkItem(RaiseConnect, args);
            }
        }

        class RaiseConnectParams
        {
            internal Exception ex { get; set; }
            internal AsyncResultNoResult arOrig { get; set; }
        }

        private static void RaiseConnect(object state)
        {
            var args = (RaiseConnectParams)state;
            args.arOrig.SetAsCompleted(args.ex, false);
        }


        public void Connect(BluetoothEndPoint remoteEP)
        {
            IAsyncResult ar = BeginConnect(remoteEP, null, null);
            EndConnect(ar);
        }

        public IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            // Just in case the user modifies the original endpoint or address!!!
            BluetoothEndPoint rep2 = (BluetoothEndPoint)remoteEP.Clone();
            //
            AsyncResultNoResult arConnect = new AsyncResultNoResult(requestCallback, state);
            AsyncResultNoResult origArConnect = System.Threading.Interlocked.CompareExchange(
                ref m_arConnect, arConnect, null);
            if (origArConnect != null)
                throw new InvalidOperationException("Another Connect operation is already in progress.");
            BeginFillInPort(rep2, Connect_FillInPortCallback,
                new BeginConnectState(rep2, arConnect));
            return arConnect;
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            AsyncResultNoResult ar2 = (AsyncResultNoResult)asyncResult;
            ar2.EndInvoke();
            Debug.Assert(m_arConnect == null, "NOT m_arConnect == null");
        }

        sealed class BeginConnectState
        {
            //Unused: internal BluetoothEndPoint inputEP;
            internal AsyncResultNoResult arCliConnect;

            public BeginConnectState(BluetoothEndPoint inputEP, AsyncResultNoResult arCliConnect)
            {
                //this.inputEP = inputEP;
                this.arCliConnect = arCliConnect;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is rethrown in EndXxxx.")]
        void Connect_FillInPortCallback(IAsyncResult ar)
        {
            BeginConnectState bcState = (BeginConnectState)ar.AsyncState;
            AsyncResultNoResult arConnect = bcState.arCliConnect;
            try {
                BluetoothEndPoint remoteEpWithPort = EndFillInPort(ar);
                if (arConnect.IsCompleted) {
                    // User called Close/Dispose when we were in (slow!) SDP lookup.
                    Debug.Assert(m_disposed, "arConnect.IsCompleted but NOT m_cancelled");
                    return;
                }
                _remotePort = remoteEpWithPort.Port;
                Debug.Assert(_remotePort != -1 && _remotePort != 0, "port is 'empty' is: " + _remotePort);
                ConnBeginConnect(remoteEpWithPort, Connect_ConnCallback, bcState);
            } catch (Exception ex) {
                Connect_SetAsCompleted_CompletedSyncFalse(arConnect, ex);
            }
        }

        protected abstract IAsyncResult ConnBeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state);
        protected abstract void ConnEndConnect(IAsyncResult asyncResult);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is rethrown in EndXxxx.")]
        void Connect_ConnCallback(IAsyncResult ar)
        {
            BeginConnectState bsState = (BeginConnectState)ar.AsyncState;
            AsyncResultNoResult arCliConnect = bsState.arCliConnect;
            try {
                ConnEndConnect(ar);
                Connect_SetAsCompleted_CompletedSyncFalse(arCliConnect, null);
            } catch (Exception ex) {
                Connect_SetAsCompleted_CompletedSyncFalse(arCliConnect, ex);
            }
        }

        //--------------------------------------------------------------
        private IAsyncResult BeginFillInPort(BluetoothEndPoint bep, AsyncCallback asyncCallback, Object state)
        {
            MiscUtils.Trace_WriteLine("BeginFillInPortState");
            AsyncResult<BluetoothEndPoint> arFIP = new AsyncResult<BluetoothEndPoint>(asyncCallback, state);
            if (bep.Port != 0 && bep.Port != -1) { // Thus, no modification required.
                MiscUtils.Trace_WriteLine("BeginFillInPort, has port -> Completed Syncronously");
                //Debug.Assert(bep.Port >= BluetoothEndPoint.MinScn && bep.Port <= BluetoothEndPoint.MaxScn, "!!Port=" + bep.Port);
                arFIP.SetAsCompleted(bep, true);
                return arFIP;
            }
            IAsyncResult ar2 = _parent.BeginServiceDiscovery(bep.Address, bep.Service,
                FillInPort_ServiceDiscoveryCallback, new BeginFillInPortState(bep, arFIP));
            return arFIP;
        }

        private BluetoothEndPoint EndFillInPort(IAsyncResult ar)
        {
            AsyncResult<BluetoothEndPoint> arFIP = (AsyncResult<BluetoothEndPoint>)ar;
            return arFIP.EndInvoke();
        }

        sealed class BeginFillInPortState
        {
            internal BluetoothEndPoint inputEP;
            internal AsyncResult<BluetoothEndPoint> arFillInPort;

            public BeginFillInPortState(BluetoothEndPoint inputEP, AsyncResult<BluetoothEndPoint> arFillInPort)
            {
                this.inputEP = inputEP;
                this.arFillInPort = arFillInPort;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Exception is rethrown in EndXxxx.")]
        void FillInPort_ServiceDiscoveryCallback(IAsyncResult ar)
        {
            BeginFillInPortState fipState = (BeginFillInPortState)ar.AsyncState;
            AsyncResult<BluetoothEndPoint> arFIP = fipState.arFillInPort;
            try {
                DoEndServiceDiscovery(ar, fipState.inputEP, arFIP);
            } catch (Exception ex) {
                arFIP.SetAsCompleted(ex, false);
            }
        }

        internal static List<int> ListAllRfcommPortsInRecords(List<ServiceRecord> list)
        {
            var portList = new List<int>();
            foreach (var curSR in list) {
                int possiblePort = ServiceRecordHelper.GetRfcommChannelNumber(curSR);
                portList.Add(possiblePort);
            }//for
            return portList;
        }

        internal static List<int> ListAllL2CapPortsInRecords(List<ServiceRecord> list)
        {
            var portList = new List<int>();
            foreach (var curSR in list) {
                int possiblePort = ServiceRecordHelper.GetL2CapChannelNumber(curSR);
                portList.Add(possiblePort);
            }//for
            return portList;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "SetAsCompleted")]
        void DoEndServiceDiscovery(IAsyncResult ar, BluetoothEndPoint inputEP,
            AsyncResult<BluetoothEndPoint> arFipToBeSACd)
        {
            try {
                var portList = _parent.EndServiceDiscovery(ar);
                if (portList.Count == 0) {
                    MiscUtils.Trace_WriteLine("DoEndServiceDiscovery, zero records!");
                    arFipToBeSACd.SetAsCompleted(CommonSocketExceptions.Create_NoResultCode(
                            CommonSocketExceptions.SocketError_NoSuchService, "PortLookup_Zero"),
                        false);
                } else {
                    MiscUtils.Trace_WriteLine("DoEndServiceDiscovery, got {0} records.", portList.Count);
                    Debug.Assert(portList.Count >= 1, "NOT portList.Count>=1, is: " + portList.Count);
                    foreach (var cur in portList) {
                        if (cur != -1) {
                            MiscUtils.Trace_WriteLine("FillInPort_ServiceDiscoveryCallback, got port: {0}", cur);
                            BluetoothEndPoint epWithPort = new BluetoothEndPoint(
                                inputEP.Address, inputEP.Service, cur);
                            arFipToBeSACd.SetAsCompleted(epWithPort, false);
                            return;
                        }
                    }//for
                    MiscUtils.Trace_WriteLine("FillInPort_ServiceDiscoveryCallback, no scn found");
                    // -> Error. No Rfcomm SCN!
                    arFipToBeSACd.SetAsCompleted(CommonSocketExceptions.Create_NoResultCode(
                            CommonSocketExceptions.SocketError_ServiceNoneRfcommScn, "PortLookup_NoneRfcomm"),
                        false);
                }
            } catch (Exception ex) {
                arFipToBeSACd.SetAsCompleted(ex, false);
            }
        }

    }//class

}

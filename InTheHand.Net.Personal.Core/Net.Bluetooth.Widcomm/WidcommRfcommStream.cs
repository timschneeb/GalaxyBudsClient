// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

//define TRACE_TO_FILE
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
//
using WriteAsyncResult = InTheHand.Net.AsyncNoResult<InTheHand.Net.Bluetooth.Factory.CommonRfcommStream.BeginReadParameters>;
using ReadAsyncResult = InTheHand.Net.AsyncResult<int, InTheHand.Net.Bluetooth.Factory.CommonRfcommStream.BeginReadParameters>;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    // TODO ! Make 'sealed' WidcommRfcommStream
    /*
     * 1. Rename WidcommRfcommStream to WidcommRfcommStreamBase
     * 2. Recompile including Tests!
     * 3. Make WidcommRfcommStreamBase abstract, and add the concrete "sealed" version.
     * 4. Compile and fix the references needed to the concrete class.
    */
    internal sealed class WidcommRfcommStream : WidcommRfcommStreamBase
    {
        internal WidcommRfcommStream(IRfcommPort port, IRfCommIf rfCommIf, WidcommBluetoothFactoryBase factory)
            : base(port, rfCommIf, factory)
        {
        }
    }


    internal abstract class WidcommRfcommStreamBase : InTheHand.Net.Bluetooth.Factory.CommonRfcommStream
    {
        WidcommBluetoothFactoryBase m_factory;
        IRfcommPort m_port;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Is used in the DEBUG build.")]
        IRfcommPort m_origPort;//DEBUG
        // Connect
        string m_passcodeToTry;
        WidcommRfcommInterface m_RfCommIf;
        // Single threading
        WidcommPortSingleThreader _singleThreader;
        // Debug
        string m_debugId, m_origPortId, m_curPortId;

        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "If we don't create the native object, there's no need for finalization")]
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected WidcommRfcommStreamBase(IRfcommPort port, IRfCommIf rfCommIf, WidcommBluetoothFactoryBase factory)
        {
            m_factory = factory;
            //----
            _singleThreader = factory.GetSingleThreader();
            bool created = false;
            try {
                SetPort(port);
                if (rfCommIf != null) {
                    m_RfCommIf = new WidcommRfcommInterface(rfCommIf);
                    rfCommIf.Create();
                }
                created = true;
            } finally {
                if (!created) { GC.SuppressFinalize(this); }
            }
        }

        private void SetPort(IRfcommPort port)
        {
            if (this.m_port != null) {
                this.m_origPort = this.m_port;
            }
            this.m_port = port;
            port.SetParentStream(this);
            DoPortCreate(port);
            //
            ResetDebugId(); // force re-create
            if (m_origPortId == null) {
                m_origPortId = port.DebugId;
            } else {
                m_curPortId = port.DebugId;
            }
        }

        private void DoPortCreate(IRfcommPort port)
        {
            if (_singleThreader != null) {
                WidcommPortSingleThreader.PortCreateCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.PortCreateCommand(port));
                cmd.WaitCompletion();
            } else {
                Debug.Assert(TestUtilities.IsUnderTestHarness(), "Why no _singleThreader?!?");
                port.Create();
            }
        }

        #region DebugId
        protected override void ResetDebugId()
        {
            m_debugId = null; // force re-create
        }

        internal protected override string DebugId
        {
            get
            {
                if (m_debugId == null) {
                    string id = m_origPortId;
                    if (m_curPortId != null) {
                        id += "->" + m_curPortId;
                    }
                    m_debugId = id;
                }
                return m_debugId;
            }
        }
        #endregion

        //--------
        protected override void RemovePortRecords()
        {
            m_factory.RemovePort(this);
        }

        protected override void DoPortClose(bool disposing)
        {
            PORT_RETURN_CODE ret;
            if (_singleThreader != null) {
                WidcommPortSingleThreader.PortCloseCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.PortCloseCommand(m_port));
                // At finalization the thread could have shutdown...
                ret = cmd.WaitCompletion(disposing);
            } else {
                 ret = m_port.Close();
            }
            if (disposing && ret != PORT_RETURN_CODE.SUCCESS) {
                Debug.WriteLine("port.Close ret: " + ret);
            }
        }

        protected override void DoOtherPreDestroy(bool disposing)
        {
            if (m_RfCommIf != null)
                m_RfCommIf.Dispose(disposing);
        }

        protected override void DoPortDestroy(bool disposing)
        {
            if (_singleThreader != null) {
                ThreadStart dlgt = delegate { m_port.Destroy(); };
                WidcommPortSingleThreader.MiscNoReturnCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.MiscNoReturnCommand(dlgt));
                cmd.WaitCompletion(disposing);
            } else {
                m_port.Destroy();
            }
        }

        //--------
        internal IAsyncResult BeginConnect(BluetoothEndPoint bep, string pin,
            AsyncCallback asyncCallback, Object state)
        {
            m_passcodeToTry = pin;
            return base.BeginConnect(bep, asyncCallback, state);
        }

        //----
        protected override void AddPortRecords()
        {
            m_factory.AddPort(this);
        }

        protected override void DoOtherSetup(BluetoothEndPoint bep, int scn)
        {
            m_RfCommIf.SetScnForPeerServer(bep.Service, scn);
            m_RfCommIf.SetSecurityLevelClient(BTM_SEC.NONE);
        }

        protected override void DoOpenClient(int scn, BluetoothAddress addressToConnect)
        {
            byte[] address = WidcommUtils.FromBluetoothAddress(addressToConnect);
            PORT_RETURN_CODE ret;
            if (_singleThreader != null) {
                WidcommPortSingleThreader.OpenClientCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.OpenClientCommand(scn, address, m_port));
                ret = cmd.WaitCompletion();
            } else {
                ret = m_port.OpenClient(scn, address);
            }
            Utils.MiscUtils.Trace_WriteLine("OpenClient ret: {0}=0x{0:X}", ret);
            if (ret != PORT_RETURN_CODE.SUCCESS)
                throw WidcommSocketExceptions.Create(ret, "OpenClient");
        }

        protected override void DoOpenServer(int scn)
        {
            PORT_RETURN_CODE ret;
            if (_singleThreader != null) {
                WidcommPortSingleThreader.OpenServerCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.OpenServerCommand(scn, m_port));
                ret = cmd.WaitCompletion();
            } else {
                ret = m_port.OpenServer(scn);
            }
            Utils.MiscUtils.Trace_WriteLine("OpenServer ret: {0}=0x{0:X}", ret);
            if (ret != PORT_RETURN_CODE.SUCCESS)
                throw WidcommSocketExceptions.Create(ret, "OpenServer");
        }

        //----
        PORT_EV PORT_EV_ModemSignal
            = PORT_EV.CTS | PORT_EV.DSR | PORT_EV.RLSD | PORT_EV.BREAK | PORT_EV.ERR
            | PORT_EV.RING | PORT_EV.CTSS | PORT_EV.DSRS | PORT_EV.RLSDS | PORT_EV.OVERRUN;

        internal void HandlePortEvent(PORT_EV eventId, IRfcommPort port)
        {
            LogFormat("{3}: {0} ({1}) port: {2}\r\n", eventId, m_state_, port, DebugId);
            lock (_lockKey) {
                if (port != m_port) {
                    return;
                }
            }
            int handledCount = 0;
            //
            if ((eventId & PORT_EV_ModemSignal) != 0) {
                ++handledCount;
                ; //NOP
            }
            if ((eventId & PORT_EV.CONNECTED) != 0) {
                ++handledCount;
                HandleCONNECTED(eventId);
            }
            if ((eventId & PORT_EV.CONNECT_ERR) != 0) {
                ++handledCount;
                HandleCONNECT_ERR(eventId);
            }
            if ((eventId & PORT_EV.TXCHAR) != 0) {
                ++handledCount;
                ;//NOP
            }
            if ((eventId & PORT_EV.TXEMPTY) != 0) {
                ++handledCount;
                FreePendingWrites();
            }
            if ((eventId & PORT_EV.FCS) != 0) { // FlowControl On
                ++handledCount;
            }
            if (((eventId & PORT_EV.FC) != 0)
                    && ((eventId & PORT_EV.FCS) == 0)) { // FlowControl Off
                ++handledCount;
                ; // NOP
            }
            //
            if (handledCount == 0)
                Utils.MiscUtils.Trace_WriteLine(DebugId + ": " + "Unknown event: '{0}'=0x{0:X}", eventId);
        }

        private void HandleCONNECTED(PORT_EV eventId)
        {
            HandleCONNECTED(eventId.ToString());
        }


        protected override bool DoIsConnected(out BluetoothAddress p_remote_bdaddr)
        {
            bool connected;
            if (_singleThreader != null) {
                Func<IsConnectedResult> dlgt = delegate {
                    IsConnectedResult rslt = new IsConnectedResult();
                    rslt._connected = m_port.IsConnected(out rslt._p_remote_bdaddr);
                    return rslt;
                };
                WidcommPortSingleThreader.MiscReturnCommand<IsConnectedResult> cmd = AddCommand(
                    new WidcommPortSingleThreader.MiscReturnCommand<IsConnectedResult>(dlgt));
                IsConnectedResult ret = cmd.WaitCompletion();
                connected = ret._connected;
                p_remote_bdaddr = ret._p_remote_bdaddr;
            } else {
                connected = m_port.IsConnected(out p_remote_bdaddr);
            }
            return connected;
        }

        //--------------------------------------------------------------
        internal void HandlePortReceive(byte[] buffer, IRfcommPort port)
        {
            //Log("HandlePortReceive port: " + port.DebugId);
            lock (_lockKey) {
                //Log("HandlePortReceive: in lock");
                if (port != m_port) {
                    return;
                }
            }
            base.HandlePortReceive(buffer);
        }

        //--------
        protected override void DoWrite(byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written)
        {
            PORT_RETURN_CODE ret;
            if (_singleThreader != null) {
                WidcommPortSingleThreader.PortWriteCommand cmd = AddCommand(
                    new WidcommPortSingleThreader.PortWriteCommand(p_data, len_to_write, m_port));
                ret = cmd.WaitCompletion(out p_len_written);
            } else {
                ret = m_port.Write(p_data, len_to_write, out p_len_written);
            }
            if (ret != PORT_RETURN_CODE.SUCCESS)
                throw new IOException(WrappingIOExceptionMessage,
                    WidcommSocketExceptions.Create(ret, "Write"));
        }

        delegate PORT_RETURN_CODE FuncPortWrite(
            byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written,
            Thread callerThread);

        PORT_RETURN_CODE BackgroundWrite(byte[] p_data, UInt16 len_to_write, out UInt16 p_len_written,
            Thread callerThread)
        {
            Debug.Assert(Thread.CurrentThread != callerThread, "Same thread!!! "
                + Thread.CurrentThread.ManagedThreadId + " =?= " + callerThread.ManagedThreadId);
            PORT_RETURN_CODE ret = m_port.Write(p_data, len_to_write, out p_len_written);
            return ret;
        }


        //--------
        class IsConnectedResult
        {
            public bool _connected;
            public BluetoothAddress _p_remote_bdaddr;
        }

        private void HandleCONNECT_ERR(PORT_EV eventId)
        {
            HandleCONNECT_ERR(eventId.ToString(), null);
        }

        //----
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override bool TryBondingIf_inLock(BluetoothAddress addressToConnect, int ocScn, out Exception error)
        {
            const bool Retrying = true;
            const bool NotRetrying = false;
            //
            /*
             * if(havePin)
             *   if(Bond success)
             *     if(OpenClient success)
             *       return Retrying;
             * return NotRetrying;
             */
            try { // Mustn't die on this thread, need to report all exceptions back!!
                //
                if (m_passcodeToTry != null) {
                    if (addressToConnect == null) {
                        Debug.Fail("In retry, have passcode, but looks like Server mode!!");
                        error = null;
                        return NotRetrying;
                    }
                    //
                    string passcodeToTry = m_passcodeToTry;
                    m_passcodeToTry = null;
                    Debug.Assert(addressToConnect != null, "addressToConnect != null");
                    bool didPair = Bond(addressToConnect, passcodeToTry);
                    if (didPair) {
                        //TODO Destroy old port!
                        SetPort(m_factory.GetWidcommRfcommPort());
                        PORT_RETURN_CODE ret = m_port.OpenClient(ocScn, WidcommUtils.FromBluetoothAddress(addressToConnect));
                        Utils.MiscUtils.Trace_WriteLine("OpenClient/AB ret: {0}=0x{0:X}", ret);
                        if (ret == PORT_RETURN_CODE.SUCCESS) {
                            error = null;
                            return Retrying;
                        } else {
                            error = WidcommSocketExceptions.Create(ret, "OpenClient/AB");
                            return NotRetrying;
                        }
                    }
                }
                //
                error = null;
            } catch (Exception ex) {
                error = ex;
            }
            return NotRetrying;
        }

        /// <summary>
        /// Wrapper around CBtIf::Bond().
        /// </summary>
        /// <param name="device"><see cref="BluetoothAddress"/></param>
        /// <param name="passcode"><see cref="T:System.String"/></param>
        /// <returns><see langword="true"/> if pairing was completed.
        /// <see langword="false"/> if were already paired, or pairing failed.
        /// </returns>
        private bool Bond(BluetoothAddress device, string passcode)
        {
            //bool pd = BluetoothSecurity.PairRequest(device, passcode);
            BOND_RETURN_CODE rc = ((WidcommBluetoothSecurity)m_factory.DoGetBluetoothSecurity()).Bond_(device, passcode);
            Log("Bond: " + rc);
            switch (rc) {
                case BOND_RETURN_CODE.SUCCESS:
                    return true;
                case BOND_RETURN_CODE.FAIL:
                    // TODO ? BOND_RETURN_CODE.FAIL -> Report this in the exception?
                    return false;
                case BOND_RETURN_CODE.ALREADY_BONDED:
                    // (BTW:"maintained for compatibility")
                    return false;
                case BOND_RETURN_CODE.REPEATED_ATTEMPTS:
                    // What is this??
                    return false;
                case BOND_RETURN_CODE.BAD_PARAMETER:
                    // TODO ? BOND_RETURN_CODE.BAD_PARAMETER -> Report this in the exception?
                    return false;
                case BOND_RETURN_CODE.NO_BT_SERVER:
                default:
                    return false;
            }
        }


        //----
        #region SingleThread Actions
        private T AddCommand<T>(T cmd)
            where T : WidcommPortSingleThreader.StCommand
        {
            return _singleThreader.AddCommand(cmd);
        }
        #endregion

    }

}

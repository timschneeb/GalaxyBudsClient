// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaRfcommStream
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    sealed class BluetopiaRfcommStream : InTheHand.Net.Bluetooth.Factory.CommonRfcommStream
    {
        readonly BluetopiaFactory _fcty;
        readonly NativeMethods.SPP_Event_Callback _callbackAAAA;
        //
        uint? _hPortClient, _hPortServer;
        BluetoothAddress _serverIndAddress;

        internal BluetopiaRfcommStream(BluetopiaFactory factory)
        {
            Debug.Assert(factory != null, "factory NULL");
            _fcty = factory;
            _callbackAAAA = HandleSPP_Event_Callback;
        }

#if DEBUG
        internal uint Testing_GetPortId()
        {
            return PortHandle;
        }
#endif

        internal uint PortHandle
        {
            get
            {
                Debug.Assert(_hPortClient.HasValue ^ _hPortServer.HasValue,
                    "hPCli (" + _hPortClient.HasValue + ") vs hPSvr (" + _hPortServer.HasValue + ")");
                if (_hPortClient.HasValue) {
                    return _hPortClient.Value;
                } else if (_hPortServer.HasValue) {
                    return _hPortServer.Value;
                } else {
                    throw new InvalidOperationException("Not connected.");
                }
            }
        }

        #region Logging
        protected override void ResetDebugId()
        {
        }

        protected internal override string DebugId
        {
            get { return "DEBUGID"; }
        }
        #endregion

        #region Close
        protected override void RemovePortRecords()
        {
            //To-Do
        }

        protected override void DoOtherPreDestroy(bool disposing)
        { }

        protected override void DoPortClose(bool disposing)
        {
            // We have the event handle now run all our code on a threadpool
            // thread so there can't be any deadlock, but leave this 
            //    // We saw an apparent deadlock
            //    // ThreadB: BTPS->HandleSpp_Event_Callback->HandleCONNECT_ERR -> Monitor.Enter
            //    // ThreadA: MeThreadPool->...
            //    //   ->CommonRfcommStream.Dispose [Holding Monitor]->DoPortClose->SPP_Close_Port
            //    ThreadPool.QueueUserWorkItem(PortClose_Runner, disposing);
            //}
            //
            //void PortClose_Runner(object state)
            //{
            //    bool disposing = (bool)state;
            if (_hPortClient.HasValue) {
                var portId = _hPortClient.Value;
                _hPortClient = null;
                var ret = _fcty.Api.SPP_Close_Port(_fcty.StackId, portId);
                if (m_state_ == State.PeerDidClose) {
                    // Get here in two cases: real-PeerDidClose *and* Timeout!
                    // So *need* to close the latter case so will get a SUCCESS then.
                    Debug.Assert(ret == BluetopiaError.INVALID_PARAMETER, "INFO: SPP_Close_Port expecting IP (state: " + m_state_ + ")");
                } else if (m_state_ == State.Closed) {
                    // Likely the Finalizer thread called us at the same time 
                    // as an app thread did, so there's a race on accessing/overwriting 
                    // _hPortClient.  Just spat the error.
                    // Ideally we'd solve  the race by Interlock.Exchang-ing 
                    // _hPortClient or by locking.  But closing twice is not 
                    // really a problem.
                    Debug.Assert(ret == BluetopiaError.RFCOMM_INVALID_DLCI, "INFO: SPP_Close_Port expecting RID (state: " + m_state_ + ")");
                    /*
                        The thread 0x85ac1746 has exited with code 0 (0x0).
                        The thread 0x85ac1746 has exited with code 0 (0x0).
                        CONNECT_ERR DEBUGID, m_state: Connected, m_arConnect (null)
                        HandlePortEvent: closed when open.
                        14:56:38: RemovePort from CloseInternal (state: Connected)
                        Function: InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRfcommStream.DoPortClose(bool), Thread: 0x85AC1746 <No Name>; m_state_: Connected
                     ** Function: InTheHand.Net.Bluetooth.StonestreetOne.BluetopiaRfcommStream.DoPortClose(bool), Thread: 0x8711DFC6 GC Finalizer Thread; m_state_: Closed
                        The thread '<No Name>' (0x8711dfc6) has exited with code 0 (0x0).
                     */
                } else {
                    BluetopiaUtils.Assert(ret, "SPP_Close_Port expecting OK (state: " + m_state_ + ")");
                }
            }
            if (_hPortServer.HasValue) {
                var portId = _hPortServer.Value;
                _hPortServer = null;
                var ret = _fcty.Api.SPP_Close_Server_Port(_fcty.StackId, portId);
                if (m_state_ == State.PeerDidClose) {
                    // Get here in two cases: real-PeerDidClose *and* Timeout!
                    // So *need* to close the latter case so will get a SUCCESS then.
                    Debug.Assert(ret == BluetopiaError.INVALID_PARAMETER, "SPP_Close_Server_Port expecting IP  (state: " + m_state_ + ")");
                } else if (m_state_ == State.Closed) {
                    // Likely the Finalizer thread called us at the same time 
                    // as an app thread did, so there's a race on accessing/overwriting 
                    // _hPortClient.  Just splat the assert.  See above.
                    Debug.Assert(ret == BluetopiaError.RFCOMM_INVALID_DLCI, "INFO: SPP_Close_Server_Port expecting RID (state: " + m_state_ + ")");
                } else {
                    BluetopiaUtils.Assert(ret, "SPP_Close_Server_Port expecting OK (state: " + m_state_ + ")");
                }
            }
        }

        protected override void DoPortDestroy(bool disposing)
        { }
        #endregion

        #region Open
        protected override void DoOtherSetup(BluetoothEndPoint bep, int scn)
        { }

        protected override void AddPortRecords()
        {
            //To-Do
        }

        protected override void DoOpenClient(int scn, BluetoothAddress addressToConnect)
        {
            _fcty.CancelAllQueryNames();
            var addrX = BluetopiaUtils.BluetoothAddressAsInteger(addressToConnect);
            int hConn = _fcty.Api.SPP_Open_Remote_Port(_fcty.StackId, addrX, (uint)scn,
                _callbackAAAA, 0);
            var ret = (BluetopiaError)hConn;
            int i;
            for (i = 0; i < 5 && ret == BluetopiaError.RFCOMM_UNABLE_TO_CONNECT_TO_REMOTE_DEVICE; ++i) {
                // Sometimes see this error here, my guess is that the baseband
                // connection used by the SDP Query is closing right when as we
                // wanted to connect, so we fail to initiate the connect.  In 
                // the debugger when we retry it succeeds, so retry.
                if (i > 0) Thread.Sleep(100); // Try right away, then after 100ms sleeps
                hConn = _fcty.Api.SPP_Open_Remote_Port(_fcty.StackId, addrX, (uint)scn,
                    _callbackAAAA, 0);
                ret = (BluetopiaError)hConn;
            }
            if (i > 0) {
                Debug.WriteLine("Auto-retry " + i + " after RFCOMM_UNABLE_TO_CONNECT_TO_REMOTE_DEVICE for SPP_Open_Remote_Port");
            }
            BluetopiaUtils.CheckAndThrow(ret, "SPP_Open_Remote_Port");
            _hPortClient = checked((uint)ret);
        }

        protected override void DoOpenServer(int scn)
        {
            int hConn = _fcty.Api.SPP_Open_Server_Port(_fcty.StackId, (uint)scn,
                _callbackAAAA, 0);
            var ret = (BluetopiaError)hConn;
            BluetopiaUtils.CheckAndThrow(ret, "SPP_Open_Remote_Port");
            Debug.WriteLine("SPP_Open_Remote_Port portId: " + ret);
            _hPortServer = checked((uint)ret);
        }

        protected override bool TryBondingIf_inLock(BluetoothAddress addressToConnect, int ocScn, out Exception error)
        {
            const bool NotRetrying = false;
            error = null;
            return NotRetrying;
        }

        //NEW protected new internal IAsyncResult BeginConnect(BluetoothEndPoint bep, //string pin,
        //    AsyncCallback asyncCallback, Object state)
        //{
        //    //_hackRemoteAddress = bep.Address;
        //    //_hackRemotePort = checked((byte)bep.Port);
        //    return base.BeginConnect(bep, asyncCallback, state);
        //}
        #endregion

        #region Write
        protected override void DoWrite(byte[] p_data, ushort len_to_write, out ushort p_len_written)
        {
            int writeLen = _fcty.Api.SPP_Data_Write(_fcty.StackId, PortHandle, len_to_write, p_data);
            var ret = (BluetopiaError)writeLen;
            BluetopiaUtils.CheckAndThrow(ret, "SPP_Data_Write");
            p_len_written = checked((ushort)writeLen);
        }
        #endregion

        //----
        const int SocketError_InvalidArgument = 10022;
        const int SocketError_TimedOut = 10060;
        const int SocketError_ConnectionRefused = 10061;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal // for test!
            void HandleSPP_Event_Callback(uint BluetoothStackID,
            ref Structs.SPP_Event_Data eventData, uint CallbackParameter)
        {
            try {
                HandleSPP_Event_Callback2(BluetoothStackID,
                    ref eventData, CallbackParameter);
            } catch (Exception ex) {
                Utils.MiscUtils.Trace_WriteLine("Exception in our HandleSPP_Event_Callback_t: " + ex);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "BluetoothStackID")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "CallbackParameter")]
        void HandleSPP_Event_Callback2(uint BluetoothStackID,
            ref Structs.SPP_Event_Data eventData, uint CallbackParameter)
        {
            //Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
            //    "HandleSPP_Event_Callback2: {0}=0x{1:X}",
            //    eventData.Event_Data_Type, (int)eventData.Event_Data_Type));
            switch (eventData.Event_Data_Type) {
                case StackConsts.SPP_Event_Type.Port_Open_Indication:
                    EventConnectInd(eventData);
                    break;
                case StackConsts.SPP_Event_Type.Port_Open_Confirmation:
                    EventConnectConfOrNeg(eventData);
                    break;
                case StackConsts.SPP_Event_Type.Port_Close_Port_Indication:
                    HandleCONNECT_ERR("REMOTE-CLOSE", null);
                    break;
                case StackConsts.SPP_Event_Type.Port_Status_Indication:
                    break;
                case StackConsts.SPP_Event_Type.Port_Data_Indication:
                    HandleEventRead(eventData);
                    break;
                case StackConsts.SPP_Event_Type.Port_Transmit_Buffer_Empty_Indication:
                    FreePendingWrites();
                    break;
                case StackConsts.SPP_Event_Type.Port_Line_Status_Indication:
                case StackConsts.SPP_Event_Type.Port_Send_Port_Information_Indication:
                case StackConsts.SPP_Event_Type.Port_Send_Port_Information_Confirmation:
                case StackConsts.SPP_Event_Type.Port_Query_Port_Information_Indication:
                case StackConsts.SPP_Event_Type.Port_Query_Port_Information_Confirmation:
                    // NO-OP
                    break;
                case StackConsts.SPP_Event_Type.Port_Open_Request_Indication:
                    // We use automatic accept mode so we won't see this event.
                    Debug.Fail("Don't expect Port_Open_Request_Indication event.");
                    break;
                default:
                    Debug.Fail("unknown event: " + eventData.Event_Data_Type);
                    break;
            }
        }

        #region Connecting
        private Structs.SPP_Event_Data EventConnectConfOrNeg(Structs.SPP_Event_Data SPP_Event_Data)
        {
            var data = (Structs.SPP_Open_Port_Confirmation_Data)
                Marshal.PtrToStructure(SPP_Event_Data.pEventData,
                    typeof(Structs.SPP_Open_Port_Confirmation_Data));
            int? socketErrorCode;
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "EventConnectConf: {0}=0x{1:X}",
                data.PortOpenStatus, (int)data.PortOpenStatus));
            switch (data.PortOpenStatus) {
                case StackConsts.SPP_OPEN_PORT_STATUS.Success:
                    socketErrorCode = null;
                    break;
                case StackConsts.SPP_OPEN_PORT_STATUS.ConnectionRefused:
                    socketErrorCode = SocketError_ConnectionRefused;
                    break;
                case StackConsts.SPP_OPEN_PORT_STATUS.ConnectionTimeout:
                    socketErrorCode = SocketError_TimedOut;
                    break;
                case StackConsts.SPP_OPEN_PORT_STATUS.UnknownError:
                    socketErrorCode = SocketError_InvalidArgument;
                    break;
                default:
                    Debug.WriteLine("Unknown SPP_Open_Port_Confirmation_Data_t code: 0x"
                        + ((int)data.PortOpenStatus).ToString("X"));
                    socketErrorCode = SocketError_InvalidArgument;
                    break;
            }
            ThreadPool.QueueUserWorkItem(delegate {
                if (socketErrorCode == null) {
                    HandleCONNECTED(data.PortOpenStatus.ToString());
                } else {
                    HandleCONNECT_ERR(data.PortOpenStatus.ToString(), socketErrorCode);
                }
            });
            return SPP_Event_Data;
        }

        private Structs.SPP_Event_Data EventConnectInd(Structs.SPP_Event_Data SPP_Event_Data)
        {
            var data = (Structs.SPP_Open_Port_Indication_Data)
                Marshal.PtrToStructure(SPP_Event_Data.pEventData,
                    typeof(Structs.SPP_Open_Port_Indication_Data));
            Debug.WriteLine("EventConnectInd portId: " + data.SerialPortID);
            Debug.Assert(_hPortServer.Value == data.SerialPortID, "thought this should be the same id");
            _serverIndAddress = BluetopiaUtils.ToBluetoothAddress(data.BD_ADDR);
            ThreadPool.QueueUserWorkItem(delegate {
                HandleCONNECTED("INDICATE");
            });
            return SPP_Event_Data;
        }

        protected override bool DoIsConnected(out BluetoothAddress p_remote_bdaddr)
        {
            if (_serverIndAddress != null) {
                p_remote_bdaddr = _serverIndAddress;
                return true;
            } else {
                return base.DoIsConnected(out p_remote_bdaddr);
            }
        }
        #endregion

        private void HandleEventRead(Structs.SPP_Event_Data eventData)
        {
            // TODO OK to do SPP_Data_Read in callback???!!!!
            var data = (Structs.SPP_Data_Indication_Data)
                Marshal.PtrToStructure(eventData.pEventData,
                    typeof(Structs.SPP_Data_Indication_Data));
            ushort bufLen = data.DataLength;
            byte[] buf = new byte[bufLen];
            var readLen = _fcty.Api.SPP_Data_Read(_fcty.StackId, PortHandle,
                bufLen, buf);
            if (readLen < 0) {
                var ret = (BluetopiaError)readLen;
                Debug.WriteLine("SPP_Data_Read error: " + ret);
                Debug.Fail("SPP_Data_Read error: " + ret);
            } else if (readLen > 0) {
                if (readLen != bufLen) {
                    Debug.Fail("SPP_Data_Read different size result!!!");
                } else {
                    HandlePortReceive(buf);
                }
            } else {
                Debug.Assert(readLen == 0);
            }
        }

    }
}

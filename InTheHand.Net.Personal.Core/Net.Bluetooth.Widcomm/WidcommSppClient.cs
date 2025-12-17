// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSppClient
// 
// Copyright (c) 2011-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2011-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using InTheHand.Net.Ports;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed partial class WidcommSppClient : WidcommSerialPort
    {
        static int _sfInConnect;
        //
        WidcommBluetoothFactoryBase _fcty;
        WidcommPortSingleThreader _singleThreader;
        NativeMethods.OnClientStateChange _handleClientStateChange;
        IntPtr _pSppCli;
        ManualResetEvent _waitConnect = new ManualResetEvent(false);
        //
        volatile SPP_STATE_CODE _statusState;
        byte[] _statusBda;
        byte[] _statusDevClass;
        byte[] _statusName;
        short? _statusComPort;
        //
        volatile bool _disposed;
        BluetoothAddress _addr;
        short? _comNum;
        string _comPortName;

        //----
        internal WidcommSppClient(WidcommBluetoothFactoryBase fcty)
        {
            _fcty = fcty;
            _singleThreader = fcty.GetSingleThreader();
            if (_singleThreader == null)
                throw new InvalidOperationException("Internal Error: No GetSingleThreader");
            _handleClientStateChange = HandleClientStateChange;
            bool success = false;
            try {
                _singleThreader.AddCommand(new WidcommPortSingleThreader.MiscNoReturnCommand(
                    () => NativeMethods.SppClient_Create(out _pSppCli, _handleClientStateChange)
                )).WaitCompletion();
                if (_pSppCli == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to initialise CSppClient.");
                success = true;
            } finally {
                if (!success) {
                    Debug.Assert(_pSppCli == IntPtr.Zero, "but failed!");
                }
            }
        }

        //----
        #region Properties
        public override BluetoothAddress Address { get { return _addr; } }

        public override Guid Service { get { return BluetoothService.SerialPort; } }

        public override string PortName
        {
            get
            {
                if (_comPortName == null)
                    throw new InvalidOperationException("Not connected");
                return _comPortName;
            }
        }
        #endregion

        #region Dispose
        ~WidcommSppClient()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_pSppCli == IntPtr.Zero) {
                Debug.Assert(_disposed == true, "NOT _disposed == true");
                return;
            }
            _disposed = true;
            var sppCli = _pSppCli;
            try {
                _singleThreader.AddCommand(new WidcommPortSingleThreader.MiscNoReturnCommand(
                    // Seems ok not to call Remove.
                    () => NativeMethods.SppClient_Destroy(sppCli)
                )).WaitCompletion(disposing);
            } finally {
                _pSppCli = IntPtr.Zero;
                _waitConnect.Close();
            }
        }
        #endregion

        //----
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void HandleClientStateChange(IntPtr bdAddr, IntPtr devClass,
            IntPtr deviceName, short com_port, SPP_STATE_CODE state)
        {
            try {
                Debug.WriteLine($"{DateTime.Now.TimeOfDay} HandleServerStateChange: state: {state} = 0x{(int)state:X}, COM: {com_port}");
                WidcommUtils.GetBluetoothCallbackValues(bdAddr, devClass, deviceName,
                    out _statusBda, out _statusDevClass, out _statusName);
                _statusComPort = com_port;
                _statusState = state;
                MemoryBarrier();
                if (state == SPP_STATE_CODE.CONNECTED) {
                    if (!IsSet(_waitConnect)) {
                        string comPort = MakePortName(_statusComPort.Value);
                        _comNum = com_port;
                        _comPortName = comPort;
                        _waitConnect.Set();
                    } else {
                        Debug.WriteLine("_waitConnect already set!");
                    }
                }
                switch (state) {
                    case SPP_STATE_CODE.CONNECTED:
                        break;
                    case SPP_STATE_CODE.DISCONNECTED:
                        break;
                    case SPP_STATE_CODE.RFCOMM_CONNECTION_FAILED:
                        break;
                    case SPP_STATE_CODE.PORT_IN_USE:
                        break;
                    case SPP_STATE_CODE.PORT_NOT_CONFIGURED:
                        break;
                    case SPP_STATE_CODE.SERVICE_NOT_FOUND:
                        break;
                    case SPP_STATE_CODE.ALLOC_SCN_FAILED:
                        break;
                    case SPP_STATE_CODE.SDP_FULL:
                        break;
                    default:
                        break;
                }
                // TODO SyncCtx.Post
                ThreadPool.QueueUserWorkItem(Event_Runner,
                    new PortStatusChangedEventArgs(
                        state == SPP_STATE_CODE.CONNECTED, _comPortName, _addr));
            } catch (Exception ex) {  // Let's not kill a Widcomm thread!
                Utils.MiscUtils.Trace_WriteLine("HandleClientStateChange ex: " + ex);
            }
        }

        void Event_Runner(object state)
        {
            var e = (PortStatusChangedEventArgs)state;
            OnPortStatusChanged(this, e);
        }

        //----

        internal string CreatePort(BluetoothAddress addr)
        {
            if (IsSet(_waitConnect))
                throw new InvalidOperationException("Already used.");
            _addr = addr;
            byte[] bd_addr = WidcommUtils.FromBluetoothAddress(addr);
            byte[] tcharzServiceName = { 0, 0 };
            var inUse = Interlocked.CompareExchange(ref _sfInConnect, 1, 0);
            if (inUse != 0)
                throw new InvalidOperationException("Widcomm only allows one SPP Connect attempt at a time.");
            SPP_CLIENT_RETURN_CODE ret = (SPP_CLIENT_RETURN_CODE)(-1);
            _singleThreader.AddCommand(new WidcommPortSingleThreader.MiscNoReturnCommand(
                () => ret = NativeMethods.SppClient_CreateConnection(
                    _pSppCli, bd_addr, tcharzServiceName)
            )).WaitCompletion();
            Debug.WriteLine("SppClient_CreateConnection ret: " + ret);
            int timeout = 30000;
            bool signalled = _waitConnect.WaitOne(timeout, false);
            // Eeek want to set this even when we got NO callback........
            // Do for now because Win32 is not working at all......
            Interlocked.Exchange(ref _sfInConnect, 0);
            if (!signalled)
                throw CommonSocketExceptions.Create_NoResultCode(
                    WidcommSppSocketExceptions.SocketError_Misc,
                    "CreatePort failed (time-out).");
            MemoryBarrier();
            if (_statusState != SPP_STATE_CODE.CONNECTED)
                throw WidcommSppSocketExceptions.Create(_statusState, "CreatePort");
            if (_statusComPort == null)
                throw CommonSocketExceptions.Create_NoResultCode(
                    WidcommSppSocketExceptions.SocketError_Misc,
                    "CreatePort did not complete (cpn).");
            //
            // TODO Move these into the native-event handler.
            //string comPort = MakePortName(_statusComPort.Value);
            //_comNum = _statusComPort.Value;
            //_comPortName = comPort;
            Debug.Assert(_comPortName != null, "_comPortName IS null");
            Debug.Assert(WidcommUtils.ToBluetoothAddress(_statusBda) == addr,
                "addr NOT equal, is: " + WidcommUtils.ToBluetoothAddress(_statusBda));
            return _comPortName;
        }

        private string MakePortName(short comPortNumber)
        {
            Debug.Assert(comPortNumber > 0, "NOT >0 : " + comPortNumber);
            string comPort = "COM" + comPortNumber;
#if NETCF
            if (comPortNumber >= 10) {
                int num2 = comPortNumber - 10;
                Debug.Assert(num2 > 0, "NOT >0 : " + num2);
                Debug.Assert(num2 < 10, "NOT <10 : " + num2);
                const string WidcommCeHighPortPrefix = "BTC";
                comPort = WidcommCeHighPortPrefix + num2;
            }
#endif
            return comPort;
        }

        //----
        tBT_CONN_STATS GetConnectionStats()
        {
            tBT_CONN_STATS stats = new tBT_CONN_STATS();
            SPP_CLIENT_RETURN_CODE ret = _singleThreader.AddCommand(
                new WidcommPortSingleThreader.MiscReturnCommand<SPP_CLIENT_RETURN_CODE>(
                    () => NativeMethods.SppClient_GetConnectionStats(_pSppCli,
                        out stats, Marshal.SizeOf(stats))
            )).WaitCompletion();
            if (ret != SPP_CLIENT_RETURN_CODE.SUCCESS)
                throw WidcommSppSocketExceptions.Create(ret, "GetConnectionStats");
            return stats;
        }

        //--------
        static bool IsSet(/*this*/ WaitHandle waiter)
        {
            bool signalled = waiter.WaitOne(0, false);
            return signalled;
        }

        void MemoryBarrier()
        {
#if !NETCF
            Thread.MemoryBarrier();
#endif
        }

        private static class NativeMethods
        {
            internal delegate void OnClientStateChange(IntPtr bdAddr,
                IntPtr devClass, IntPtr deviceName,
                short com_port, SPP_STATE_CODE state);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SppClient_Create(out IntPtr ppSppClient,
                OnClientStateChange clientStateChange);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void SppClient_Destroy(IntPtr pSppClient);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SPP_CLIENT_RETURN_CODE SppClient_RemoveConnection();

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SPP_CLIENT_RETURN_CODE SppClient_CreateConnection(
                IntPtr pSppClient, byte[] bda, IntPtr tcharzServiceName);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SPP_CLIENT_RETURN_CODE SppClient_CreateConnection(
                IntPtr pSppClient, byte[] bda, byte[] tcharzServiceName);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern SPP_CLIENT_RETURN_CODE SppClient_GetConnectionStats(
                IntPtr pObj, out tBT_CONN_STATS pStats, int cb);

        }

        /// <summary>
        /// Define SPP connection states
        /// </summary>
        internal enum SPP_STATE_CODE : int
        {
            /// <summary>
            /// port now connected
            /// </summary>
            CONNECTED,
            /// <summary>
            /// port now disconnected
            /// </summary>
            DISCONNECTED,
            /// <summary>
            ///rfcomm connction failed
            /// </summary>
            RFCOMM_CONNECTION_FAILED,

            //for SPP Client only
            /// <summary>
            /// Port in use, for SPPClient only [for SPP Client only]
            /// </summary>
            PORT_IN_USE,
            /// <summary>
            /// no port configured [for SPP Client only]
            /// </summary>
            PORT_NOT_CONFIGURED,
            /// <summary>
            /// service not found [for SPP Client only]
            /// </summary>
            SERVICE_NOT_FOUND,

            //for SPP Server Only
            /// <summary>
            /// [for SPP Server Only]
            /// </summary>
            ALLOC_SCN_FAILED,
            /// <summary>
            /// [for SPP Server Only]
            /// </summary>
            SDP_FULL
        }

        /// <summary>
        /// Define SPP connection states
        /// </summary>
        enum SPP_STATE_CODE__WCE
        {
            /// <summary>
            /// port now connected
            /// </summary>
            CONNECTED,
            /// <summary>
            /// port now disconnected
            /// </summary>
            DISCONNECTED
        } ;


        /// <summary>
        /// Define return code for SPP Client functions
        /// </summary>
        internal enum SPP_CLIENT_RETURN_CODE : int
        {
            /// <summary>
            /// Operation initiated without error
            /// </summary>
            SUCCESS,
            /// <summary>
            /// COM server could not be started
            /// </summary>
            NO_BT_SERVER,
            /// <summary>
            /// attempt to connect before previous connection closed
            /// </summary>
            ALREADY_CONNECTED,
            /// <summary>
            /// attempt to close unopened connection
            /// </summary>
            NOT_CONNECTED,
            /// <summary>
            /// local processor could not allocate memory for open
            /// </summary>
            NOT_ENOUGH_MEMORY,
            /// <summary>
            /// One or more of function parameters are not valid
            /// </summary>
            INVALID_PARAMETER__CE_UE,
            /// <summary>
            /// Any condition other than the above
            /// </summary>
            UNKNOWN_ERROR__CE_IP,
            /// <summary>
            /// no empty port
            /// </summary>
            NO_EMPTY_PORT,
            /// <summary>
            /// license error
            /// </summary>
            LICENSE_ERROR
        }

        /// <summary>
        /// Define return code for SPP Client functions
        /// </summary>
        enum SPP_CLIENT_RETURN_CODE__WCE
        {
            SUCCESS,             // Operation initiated without error
            NO_BT_SERVER,        // COM server could not be started
            ALREADY_CONNECTED,   // attempt to connect before previous connection closed
            NOT_CONNECTED,       // attempt to close unopened connection
            NOT_ENOUGH_MEMORY,   // local processor could not allocate memory for open
            UNKNOWN_ERROR,       // Any condition other than the above
            INVALID_PARAMETER    // One or more of function parameters are not valid
        }

    }

}

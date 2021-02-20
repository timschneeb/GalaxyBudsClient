// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using BTCONNHDL = System.UInt32;
using BTDEVHDL = System.UInt32;
//
using BTINT32 = System.Int32;
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Ports;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    class BluesoleilClient : CommonDiscoveryBluetoothClient
    {
        static readonly int? BufferSize = null; // = 64 * 1024;
        //
        readonly BluesoleilFactory _factory;
        readonly Action<BluetoothEndPoint> _beginConnectDlgt;
        //
        UInt32? _hDev;
        SerialPortNetworkStream _stream;
        BluetoothEndPoint _remoteEp;

        //----
        internal BluesoleilClient(BluesoleilFactory fcty)
        {
            if (fcty == null) throw new ArgumentNullException("fcty");
            _factory = fcty;
            _beginConnectDlgt = new Action<BluetoothEndPoint>(Connect);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "localEP")]
        internal BluesoleilClient(BluesoleilFactory fcty, BluetoothEndPoint localEP)
            : this(fcty)
        {
            Debug.Fail("Ignoring localEP");
        }

        //----
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (_stream != null) _stream.Close();
            }
            // TODO Kill DeviceDiscovery etc
        }

        //----
        protected override void BeginInquiry(int maxDevices, AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            _factory.BeginInquiry(maxDevices, InquiryLength,
                callback, state,
                liveDiscoHandler, liveDiscoState, args);
        }

        protected override List<IBluetoothDeviceInfo> GetKnownRemoteDeviceEntries()
        {
            bool authenticated = true;
            bool remembered = true;
            List<IBluetoothDeviceInfo> knownDevices = _factory.GetRememberedDevices(
                authenticated, remembered);
            return knownDevices;
        }

        protected override List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            List<IBluetoothDeviceInfo> devices = _factory.EndInquiry(ar);
            return devices;
        }

        //--------------------------------------------------------------
        public override System.Net.Sockets.Socket Client
        {
            get { throw new NotSupportedException("This stack does not use Sockets."); }
            set { throw new NotSupportedException("This stack does not use Sockets."); }
        }

        //----
        void ConnectRfcommPreAllocateComPort(UInt16 svcClass16, UInt32 hDev,
            out UInt32 hConn, out byte channel, out int comPort
            //, out UInt32 comSerialNum
            )
        {
            UInt32 comSerialNum;
            //
            UInt32 comSerialNum0;
            BtSdkError ret;
            UInt32 osComPort;
            //
            const UInt32 UsageTypeConst = 1;
            const StackConsts.COMM_SET flags
                = StackConsts.COMM_SET.Record | StackConsts.COMM_SET.UsageType;
            const UInt16 BTSDK_CLS_SERIAL_PORT = 0x1101;
            const int Timeout = 2200;
            //
            comSerialNum0 = _factory.Api.Btsdk_GetASerialNum();
            comSerialNum = comSerialNum0;
            Debug.Assert(comSerialNum != 0, "INFO comSerialNum == 0 wierd maybe???");
            bool success = _factory.Api.Btsdk_PlugInVComm(comSerialNum, out osComPort,
                UsageTypeConst, flags, Timeout);
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "Btsdk_GetASerialNum comSerialNum: {0}, Btsdk_PlugInVComm success: {1}, osComPort: {2}",
                comSerialNum0, success, osComPort));
            if (!success)
                BluesoleilUtils.CheckAndThrow(BtSdkError.OPERATION_FAILURE, "Btsdk_PlugInVComm");
            comPort = checked((int)osComPort);
            ret = _factory.Api.Btsdk_InitCommObj(checked((byte)osComPort),
                BTSDK_CLS_SERIAL_PORT);
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "Btsdk_InitCommObj ret: {0}", ret));
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_InitCommObj");
            //
            bool connSuccess = false;
            try {
                var sppStru = new Structs.BtSdkSPPConnParamStru(osComPort);
                ret = _factory.Api.Btsdk_ConnectEx(hDev, svcClass16,
                    ref sppStru, out hConn);
                Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "ret: {0}, hConn: 0x{1:X}", ret, hConn));
                BluesoleilUtils.CheckAndThrow(ret, "Btsdk_ConnectEx");
                _hDev = hDev;
                //
                channel = 0; // unknown
                Console.WriteLine("Connect remote SPP Service with local COM{0}\n", osComPort);
                connSuccess = true;
            } finally {
                if (!connSuccess) {
                    FreeComIndex(_factory, comPort, comSerialNum0);
                }
            }
        }

        internal static void FreeComIndex(BluesoleilFactory factory, int comNum, uint comSerialNum)
        {
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BluesoleilClient.FreeComIndex IN: comNum: {0}, comSerialNum: {1}",
                comNum, comSerialNum));
            BtSdkError ret;
            var comNum8 = checked((byte)comNum);
            ret = factory.Api.Btsdk_DeinitCommObj(comNum8);
            BluesoleilUtils.Assert(ret, "Btsdk_DeinitCommObj");
            factory.Api.Btsdk_PlugOutVComm(comSerialNum, StackConsts.COMM_SET.Record);
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BluesoleilClient.FreeComIndex OUT: Btsdk_DeinitCommObj ret: {0}", ret));
        }

        //--
        void PlayConnectSppSvc()
        {
            BTCONNHDL s_currSPPConnHdl = StackConsts.BTSDK_INVALID_HANDLE;
            BTDEVHDL s_currRmtSppDevHdl = StackConsts.BTSDK_INVALID_HANDLE;
            UInt32 s_ComSerialNum = 0;
            //----
            BtSdkError ulRet = 0;
            UInt32 osComPort;
            //
            const UInt32 UsageTypeConst = 1;
            const StackConsts.COMM_SET flags
                = StackConsts.COMM_SET.Record | StackConsts.COMM_SET.UsageType;
            const UInt16 BTSDK_CLS_SERIAL_PORT = 0x1101;
            const int Timeout = 2200;
            //
            s_ComSerialNum = _factory.Api.Btsdk_GetASerialNum();
            _factory.Api.Btsdk_PlugInVComm(s_ComSerialNum, out osComPort,
                UsageTypeConst, flags, Timeout);
            ulRet = _factory.Api.Btsdk_InitCommObj(checked((byte)osComPort),
                BTSDK_CLS_SERIAL_PORT);
            //
            if (ulRet != 0) {
                var sppStru = new Structs.BtSdkSPPConnParamStru(osComPort);
                BtSdkError ret = _factory.Api.Btsdk_ConnectEx(s_currRmtSppDevHdl, BTSDK_CLS_SERIAL_PORT,
                            ref sppStru, out s_currSPPConnHdl);
                if (ret != 0) {
                    Console.WriteLine("Connect remote SPP Service with local COM{0}\n", osComPort);
                }
            }
        }

        void PlayDisconnect()
        {
            BTCONNHDL s_currSPPConnHdl = StackConsts.BTSDK_INVALID_HANDLE;
            Int16 comNum16 = 0;
            byte comNum = 0;
            byte s_ComSerialNum = 0;
            //--
            comNum16 = _factory.Api.Btsdk_GetClientPort(s_currSPPConnHdl);
            comNum = checked((byte)comNum16);
            _factory.Api.Btsdk_Disconnect(s_currSPPConnHdl);
            s_currSPPConnHdl = StackConsts.BTSDK_INVALID_HANDLE;
            _factory.Api.Btsdk_DeinitCommObj(comNum);
            _factory.Api.Btsdk_PlugOutVComm(s_ComSerialNum, StackConsts.COMM_SET.Record);
        }

        //----
        private void ConnectRfcomm(BluetoothEndPoint remoteEP, UInt32 hDev,
            out UInt32 hConn, out byte channel, out int comPort)
        {
            Structs.BtSdkAppExtSPPAttrStru sppAttr = new Structs.BtSdkAppExtSPPAttrStru(remoteEP);
            //
            Debug.WriteLine("Gonna Btsdk_ConnectAppExtSPPService with: " + sppAttr.ToString());
            BtSdkError ret = _factory.Api.Btsdk_ConnectAppExtSPPService(hDev, ref sppAttr, out hConn);
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "ret: {0}, hConn: 0x{1:X}, with: {2}", ret, hConn, sppAttr.ToString()));
            _hDev = hDev;
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_ConnectAppExtSPPService");
            //
            var comNumGetClientPort = _factory.Api.Btsdk_GetClientPort(hConn);
            Debug.WriteLine("comNumGetClientPort: " + comNumGetClientPort);
            //
            if (sppAttr.rf_svr_chnl != 0) {
                Debug.Assert(sppAttr.com_index != 0, "Argghhhh com_index is zero! (When rf_svr_chnl isn't).");
                channel = sppAttr.rf_svr_chnl;
                comPort = sppAttr.com_index;
            } else {
                // Some profiles are handled specifically OBEX, etc etc
                // so they don't create a COM port when that 
                Debug.Assert(sppAttr.com_index == 0, "Don't expect a COM port to be created in this (fail) case.");
                //
                // Connecting to SPP 0x1101 also returns no com port in the
                // struct but a COM port is connected for it. Btsdk_GetClientPort
                // DOES return the correct port see whether we're in that case.
                if (comNumGetClientPort != 0) {
                    comPort = comNumGetClientPort;
                    channel = 0; // Unknown!
                } else {
                    // Highly likely an OPP/etc connection was made, and not a RFCOMM
                    // connection, and thus no COM port we can use. :-(  So fail!
                    Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "BlueSoleil seems no RFCOMM connection made, closing. (channel: {0}, COM: {1})",
                        sppAttr.rf_svr_chnl, sppAttr.com_index));
                    // (Note: Add a dummy record so RemoveLiveConnection works ok).
                    int liveCountB = _factory.AddConnection(hConn, NullBluesoleilConnection.Instance);
                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "BlueSoleilClient.Connect non-RFCOMM LiveConns count: {0}.", liveCountB));
                    var retD = _factory.Api.Btsdk_Disconnect(hConn);
                    BluesoleilUtils.Assert(retD, "Close non-RFCOMM connection");
                    throw BluesoleilUtils.ErrorConnectIsNonRfcomm();
                }
            }
            //System.Windows.Forms.MessageBox.Show("BlueSoleil created port: '" + comPort + "'.");
        }


        public override void Connect(BluetoothEndPoint remoteEP)
        {
            if (remoteEP.Port != 0 && remoteEP.Port != -1)
                throw new NotSupportedException("Don't support connect to particular port.  Please can someone tell me how.");
            //
            _factory.SdkInit();
            _factory.RegisterCallbacksOnce(); // Need to use event DISC_IND to close the Stream/SerialPort.
            //
            BluesoleilDeviceInfo bdi = BluesoleilDeviceInfo.CreateFromGivenAddress(remoteEP.Address, _factory);
            UInt32 hDev = bdi.Handle;
            UInt32 hConn;
            byte channel;
            int comPort;
            ConnectRfcomm(remoteEP, hDev, out hConn, out channel, out comPort);
            //if (_HasPort(remoteEP)) {
            //    ConnectRfcomm(remoteEP, hDev, out hConn, out channel, out comPort);
            //} else {
            //    UInt16? svcClass16;
            //    if (!ServiceRecordUtilities.IsUuid16Value(remoteEP.Service)) {
            //        svcClass16 = null;
            //    } else {
            //        svcClass16 = (UInt16)ServiceRecordUtilities.GetAsUuid16Value(remoteEP.Service);
            //    }
            //    UInt16[] specialClasses = { };
            //    if (svcClass16.HasValue && IsIn(svcClass16.Value, specialClasses)) {
            //        throw new NotImplementedException("Internal Error: Should not use this code.");
            //// The original method we used for connecting did not work for
            //// profiles that BlueSoleil has support for -- returning no COM
            //// port but enabling per-profile support
            //// We have a second way, which seems to work in those cases
            //// but it is more complex -- we manually allocate the port etc
            //// (also it doesn't give us the remote RFCOMM channel).
            //// Use the first method for now except in special cases...
            //        //UInt32 comSerialNum;
            //        //UInt32? comSerialNum;
            //        //ConnectRfcommPreAllocateComPort(svcClass16.Value, hDev,
            //        //    out hConn, out channel, out comPort
            //        //    //, out comSerialNum
            //        //    );
            //    } else {
            //        ConnectRfcomm(remoteEP, hDev, out hConn, out channel, out comPort);
            //    }
            //}
            _remoteEp = new BluetoothEndPoint(remoteEP.Address, BluetoothService.Empty, channel);
            //
            var serialPort = CreateSerialPort();
            if (BufferSize.HasValue) {
                serialPort.ReadBufferSize = BufferSize.Value;
                serialPort.WriteBufferSize = BufferSize.Value;
            }
            serialPort.PortName = "COM" + comPort;
            serialPort.Handshake = Handshake.RequestToSend;
            serialPort.Open();
            //((System.ComponentModel.IComponent)serialPort).Disposed
            //    += delegate { System.Windows.Forms.MessageBox.Show("BlueSoleil's SerialPort disposed"); };
            //
            // We pass '_hConn' as the Stream handles calling
            // Btsdk_Disconnect to close the RFCOMM connection.
            var strm = new BlueSoleilSerialPortNetworkStream(serialPort, hConn, this, _factory);
            _stream = strm;
            int liveCount = _factory.AddConnection(hConn, strm);
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "BlueSoleilClient.Connect LiveConns count: {0}.", liveCount));
            // TODO who handles closing the connection if opening the port fails
        }

        private ISerialPortWrapper CreateSerialPort()
        {
            ISerialPortWrapper serialPort;
            if (CreateSerialPortMethod != null) {
                serialPort = CreateSerialPortMethod();
            } else {
                serialPort = new SerialPortWrapper(new System.IO.Ports.SerialPort());
            }
            return serialPort;
        }


        public Func<ISerialPortWrapper> CreateSerialPortMethod { get; set; }

        //----
        public override IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state)
        {
            return _beginConnectDlgt.BeginInvoke(remoteEP, requestCallback, state);
        }

        public override void EndConnect(IAsyncResult asyncResult)
        {
            _beginConnectDlgt.EndInvoke(asyncResult);
        }

        public override bool Connected
        {
            get
            {
                var strm = _stream;
                if (strm == null) return false;
                if (!_hDev.HasValue) {
                    // When does ths occur?????
                    //Don't need it likely.........return false;
                }
                return strm.Connected;
            }
        }

        public override int Available
        {
            get
            {
                SerialPortNetworkStream strm = _stream;
                if (strm == null) throw new InvalidOperationException("Not connected.");
                return strm.Available;
            }
        }

        public override bool Authenticate
        {
            get { return false; }
            set
            {
                throw new NotSupportedException("BlueSoleil does not support setting authentication/encryption.");
            }
        }

        public override bool Encrypt
        {
            get { return false; }
            set
            {
                throw new NotSupportedException("BlueSoleil does not support setting authentication/encryption.");
            }
        }

        public override string GetRemoteMachineName(BluetoothAddress device)
        {
            IBluetoothDeviceInfo bdi = _factory.DoGetBluetoothDeviceInfo(device);
            return bdi.DeviceName;
        }

        public override System.Net.Sockets.NetworkStream GetStream()
        {
            if (_stream == null)
                throw new InvalidOperationException("Not connected.");
            return _stream;
        }

        public override System.Net.Sockets.LingerOption LingerState
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        //----
        public override Guid LinkKey
        {
            get { throw new NotImplementedException(); }
        }

        public override LinkPolicy LinkPolicy
        {
            get { throw new NotImplementedException(); }
        }

        public override BluetoothEndPoint RemoteEndPoint
        {
            get
            {
                if (!Connected || _remoteEp == null)
                    throw new InvalidOperationException("Not connected.");
                return _remoteEp;
            }
        }

        public override string RemoteMachineName
        {
            get
            {
                if (!Connected || _hDev == null)
                    throw new InvalidOperationException("Not connected.");
                IBluetoothDeviceInfo bdi = BluesoleilDeviceInfo
                    .CreateFromHandleFromConnection(_hDev.Value, _factory);
                return bdi.DeviceName;
            }
        }

        //----
        public override void SetPin(string pin)
        {
            throw new NotImplementedException();
        }

        public override void SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

    }
}

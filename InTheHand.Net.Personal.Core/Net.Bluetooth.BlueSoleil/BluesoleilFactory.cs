// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using InTheHand.Net.Sockets;
using System.Globalization;
//
using BtSdkUUIDStru = System.Guid;
using BTDEVHDL = System.UInt32;
using BTSVCHDL = System.UInt32;
using BTCONNHDL = System.UInt32;
using BTSHCHDL = System.UInt32;
using BTSDKHANDLE = System.UInt32;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "InTheHand.Net.Bluetooth.BlueSoleil")]

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Used by Reflection.")]
    class BluesoleilFactory : BluetoothFactory
    {
        readonly Records _records;
        //
        readonly IBluesoleilApi _api;
        readonly BluesoleilSecurity _sec;
        //

        //----
        public BluesoleilFactory()
            : this(new RealBluesoleilApi())
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "(GetAllRadios)")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BluesoleilFactory(IBluesoleilApi api)
        {
            if (api == null) throw new ArgumentNullException("api");
            _api = api;
            _records = new Records(api);
            Debug.Assert(!api.Btsdk_IsSDKInitialized(), "IsSDKInitialized unexectedly true!");
            SdkInit();
            if (!Api.Btsdk_IsBluetoothHardwareExisted()) {
                throw new PlatformNotSupportedException("BlueSoleil Bluetooth stack not supported (HardwareExisted).");
            }
            _inquiryHandler = new BluesoleilInquiry(this);
            _sec = new BluesoleilSecurity(this);
            GetAllRadios();
        }

        [DebuggerStepThrough]
        internal void SdkInit()
        {
            _records.SdkInit();
        }

        //void RunWithTiming_(ThreadStart method, out TimeSpan time)
        //{
        //    time = TimeSpan.FromSeconds(-99);
        //    DateTime startT0 = DateTime.UtcNow;
        //    try {
        //        method();
        //    } finally {
        //        time = DateTime.UtcNow - startT0;
        //    }
        //}

        //----
        protected override void Dispose(bool disposing)
        {
            if (_records != null) // Should never be null, but...
                _records.Dispose(disposing);
        }

        //----
        internal IBluesoleilApi Api
        {
            [DebuggerStepThrough]
            get
            {
                SdkInit();
                return _api;
            }
        }

        //----

        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            return BluesoleilDeviceInfo.CreateFromGivenAddress(address, this);
        }

        protected override IBluetoothRadio GetPrimaryRadio()
        {
            SdkInit();
            return new BluesoleilRadio(this);
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            return new IBluetoothRadio[] { GetPrimaryRadio() };
        }

        protected override IBluetoothClient GetBluetoothClient()
        {
            return new BluesoleilClient(this);
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            return new BluesoleilClient(this, localEP);
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            throw new NotSupportedException("Cannot create a BluetoothClient from a Socket on the BlueSoleil stack.");
        }
        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStream)
        {
            throw new NotSupportedException();
        }

        protected override IBluetoothListener GetBluetoothListener()
        {
            throw new NotSupportedException("There seems to be no API in BlueSoleil for RFCOMM servers.");
        }

        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            return _sec;
        }

        //----------------------------------------------------------------------
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<IBluetoothDeviceInfo> GetRememberedDevices(bool authenticated, bool remembered)
        {
            if (!remembered && !authenticated) {
                //Debug.Fail("Why call me?? No work to do!");
                return new List<IBluetoothDeviceInfo>();
            }
            const uint AnyCod = 0;
            UInt32[] handlesList = new UInt32[1000];
            int num = Api.Btsdk_GetStoredDevicesByClass(AnyCod, handlesList, handlesList.Length);
            var result = new List<IBluetoothDeviceInfo>();
            for (int i = 0; i < num; ++i) {
                var hDev = handlesList[i];
                BluesoleilDeviceInfo dev = BluesoleilDeviceInfo.CreateFromHandleFromStored(hDev, this);
                Debug.Assert(dev.Remembered, "NOT dev.Remembered but FromStored!");
                if (remembered || (authenticated && dev.Authenticated)) {
                    result.Add(dev);
                }
            }//for
            return result;
        }

        //--------
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Stop the reverse P/Invoke delegate from being GC'd.")]
        NativeMethods.Btsdk_Inquiry_Result_Ind_Func _inquiryResultIndFunc;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Stop the reverse P/Invoke delegate from being GC'd.")]
        NativeMethods.Btsdk_Inquiry_Complete_Ind_Func _inquiryCompleteIndFunc;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Stop the reverse P/Invoke delegate from being GC'd.")]
        NativeMethods.Btsdk_UserHandle_Pin_Req_Ind_Func _pinReqIndFunc;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "Stop the reverse P/Invoke delegate from being GC'd.")]
        NativeMethods.Btsdk_Connection_Event_Ind_Func _connectionEventIndFunc;
        NativeMethods.Func_ReceiveBluetoothStatusInfo _statusCallback;


        internal void RegisterCallbacksOnce()
        {
            if (_inquiryResultIndFunc != null) {
                return;
            }
            BtSdkError ret;
            Structs.BtSdkCallbackStru val;
            //
            _inquiryResultIndFunc = _inquiryHandler.HandleInquiryResultInd;
            val = new Structs.BtSdkCallbackStru(_inquiryResultIndFunc);
            Debug.Assert(val._type == StackConsts.CallbackType.INQUIRY_RESULT_IND);
            ret = Api.Btsdk_RegisterCallback4ThirdParty(ref val);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RegisterCallback4ThirdParty");
            //
            _inquiryCompleteIndFunc = HandleInquiryComplete;
            val = new Structs.BtSdkCallbackStru(_inquiryCompleteIndFunc);
            Debug.Assert(val._type == StackConsts.CallbackType.INQUIRY_COMPLETE_IND);
            ret = Api.Btsdk_RegisterCallback4ThirdParty(ref val);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RegisterCallback4ThirdParty");
            //
            //----
            _pinReqIndFunc = _sec.HandlePinReqInd;
            val = new Structs.BtSdkCallbackStru(_pinReqIndFunc);
            Debug.Assert(val._type == StackConsts.CallbackType.PIN_CODE_IND);
            ret = Api.Btsdk_RegisterCallback4ThirdParty(ref val);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RegisterCallback4ThirdParty");
            //
            //----
            _connectionEventIndFunc = _records.HandleConnectionEventInd;
            val = new Structs.BtSdkCallbackStru(_connectionEventIndFunc);
            Debug.Assert(val._type == StackConsts.CallbackType.CONNECTION_EVENT_IND);
            ret = Api.Btsdk_RegisterCallback4ThirdParty(ref val);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RegisterCallback4ThirdParty");
            //
            _statusCallback = HandleReceiveBluetoothStatusInfo;
            ret = Api.Btsdk_RegisterGetStatusInfoCB4ThirdParty(ref _statusCallback);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RegisterGetStatusInfoCB4ThirdParty");
            ret = Api.Btsdk_SetStatusInfoFlag(StackConsts.BTSDK_BLUETOOTH_STATUS_FLAG);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_SetStatusInfoFlag");
        }

        void HandleReceiveBluetoothStatusInfo(UInt32 usMsgType, UInt32 pulData, UInt32 param, IntPtr arg)
        {
            Debug.Assert(usMsgType == StackConsts.BTSDK_BLUETOOTH_STATUS_FLAG);
            var status = (StackConsts.BTSDK_BTSTATUS)pulData;
            Debug.WriteLine("BlueSoleil status change: " + status);
            if (status == StackConsts.BTSDK_BTSTATUS.TurnOff) {
                // We need to close connections manually at Radio Off.
                // (Connections are automatically closed at HwPulled).
                ThreadPool.QueueUserWorkItem(_ => _records.CloseAnyLiveConnections(), null);
            }
        }

        //--------
        internal IAsyncResult BeginInquiry(int maxDevices, TimeSpan inquiryLength,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            return _inquiryHandler.BeginInquiry(maxDevices, inquiryLength,
                callback, state,
                liveDiscoHandler, liveDiscoState, args);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            return _inquiryHandler.EndInquiry(ar);
        }

        void HandleInquiryComplete()
        {
            _inquiryHandler.HandleInquiryComplete(null);
        }

        //----
        readonly BluesoleilInquiry _inquiryHandler;

        class BluesoleilInquiry : CommonBluetoothInquiry<BTDEVHDL>
        {
            readonly BluesoleilFactory _fcty;

            //----
            internal BluesoleilInquiry(BluesoleilFactory fcty)
            {
                _fcty = fcty;
            }

            //----
            const uint AnyClass = 0;

            internal IAsyncResult BeginInquiry(int maxDevices, TimeSpan inquiryLength,
                AsyncCallback callback, object state,
                BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
                DiscoDevsParams args)
            {
                _fcty.SdkInit();
                _fcty.RegisterCallbacksOnce();
                //
                byte maxDurations;
                byte maxNum;
                CommonDiscoveryBluetoothClient.ConvertBthInquiryParams(maxDevices, inquiryLength, out maxNum, out maxDurations);
                return BeginInquiry(maxDevices, inquiryLength,
                    callback, state,
                    liveDiscoHandler, liveDiscoState,
                    delegate() {
                        BtSdkError ret = _fcty.Api.Btsdk_StartDeviceDiscovery(
                            AnyClass, maxNum, maxDurations);
                        BluesoleilUtils.CheckAndThrow(ret, "Btsdk_StartDeviceDiscovery");
                    }, args);
            }

            //----
            protected override IBluetoothDeviceInfo CreateDeviceInfo(UInt32 item)
            {
                IBluetoothDeviceInfo bdi = BluesoleilDeviceInfo.CreateFromHandleFromInquiry(item, _fcty);
                return bdi;
            }

        }


        internal int AddConnection(BTCONNHDL conn_hdl, IBluesoleilConnection newConn)
        {
            return _records.AddConnection(conn_hdl, newConn);
        }

        //----------------------------------
        class Records : System.Runtime.ConstrainedExecution.CriticalFinalizerObject
        {
            // Since we're Finalizable, try to hold references to as few things
            // as possible, we don't want to keep many things alive in GC when
            // in Finalizable queue.
            readonly IBluesoleilApi _api;
            bool _needsDispose;
            // We keep reference to all open connections here, we need the
            // hConn list for clean shutdown, but could we keep the references
            // to the connection objects elsewhere????
            Dictionary<BTCONNHDL, IBluesoleilConnection> _liveConns = new Dictionary<BTCONNHDL, IBluesoleilConnection>();

            internal Records(IBluesoleilApi api)
            {
                _api = api;
            }

            //----
            ~Records()
            {
                Dispose(false);
            }

            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
            internal void Dispose(bool disposing)
            {
                if (!_needsDispose)
                    return;
                //
                CloseAnyLiveConnections();
                //
                _needsDispose = false;
                BtSdkError ret = _api.Btsdk_Done();
                BluesoleilUtils.Assert(ret, "Btsdk_Done");
            }

            //----
            internal void SdkInit()
            {
                bool init = _api.Btsdk_IsSDKInitialized();
                bool conn = _api.Btsdk_IsServerConnected(); // Ok to call if '!init'?
                if (init && conn) {
                    return;
                }
                BtSdkError ret = _api.Btsdk_Init();
                //Debug.WriteLine("BlueSoleil Btsdk_Init: " + BluesoleilUtils.BtSdkErrorToString(ret));
                BluesoleilUtils.CheckAndThrow(ret, "Btsdk_Init");
#if DEBUG
                // When Radio is Off, Init succeeds when called but IsSDKInitialized
                // still returns false!  That persists until the radio's turned on.
                bool initAfter = _api.Btsdk_IsSDKInitialized();
                bool connAfter = _api.Btsdk_IsServerConnected(); // Ok to call if '!init'?
                if (!initAfter || !connAfter) {
                    bool ready = _api.Btsdk_IsBluetoothReady();
                    Debug.Assert(!ready, "Not init&&conn, but NOT ready...");
                    Debug.Assert(!init, "Not init&&conn, but NOT !init...");
                    Debug.Assert(conn, "Not init&&conn, but NOT conn...");
                }
#endif
                _needsDispose = true;
            }

            //----
            internal void HandleConnectionEventInd(UInt32 conn_hdl, StackConsts.ConnectionEventType eventType, IntPtr arg)
            {
                var props = (Structs.BtSdkConnectionPropertyStru)Marshal.PtrToStructure(
                    arg, typeof(Structs.BtSdkConnectionPropertyStru));
#if !CODE_ANALYSIS
                var msg = $"HandleConnectionEventInd event: {eventType}, conn_hdl: 0x{conn_hdl:X}, arg.hDev: 0x{props.device_handle:X} role_AND_result: 0x{props.role_AND_result:X}.";
                //Console.WriteLine(msg);
                Debug.WriteLine(msg);
#endif
                int liveCount;
                switch (eventType) {
                    case StackConsts.ConnectionEventType.CONN_IND:
                    case StackConsts.ConnectionEventType.CONN_CFM:
                        liveCount = -2;
                        break;
                    case StackConsts.ConnectionEventType.DISC_IND:
                        liveCount = UseNetworkDisconnectEvent(conn_hdl);
                        goto case StackConsts.ConnectionEventType.DISC_CFM;
                    case StackConsts.ConnectionEventType.DISC_CFM:
                        liveCount = RemoveConnection(conn_hdl);
                        break;
                    default:
                        Debug.Fail($"Unknown ConnectionEventInd: {eventType}=0x{(int)eventType:X}");
                        liveCount = -1;
                        break;
                }
                Debug.WriteLine($"BlueSoleil LiveConns count: {liveCount}.");
            }

            internal bool Contains(BTCONNHDL conn_hdl)
            {
                lock (_liveConns) {
                    var contains = _liveConns.ContainsKey(conn_hdl);
                    return contains;
                }
            }

            internal int AddConnection(BTCONNHDL conn_hdl, IBluesoleilConnection newConn)
            {
                lock (_liveConns) {
                    if (_liveConns.ContainsKey(conn_hdl)) {
                        Debug.Fail("AddDisconnect: already contains hConn! 0x" + conn_hdl.ToString("X"));
                    } else {
                        _liveConns.Add(conn_hdl, newConn);
                    }
                    return _liveConns.Count;
                }
            }

            private int RemoveConnection(BTCONNHDL conn_hdl)
            {
                lock (_liveConns) {
                    if (!_liveConns.ContainsKey(conn_hdl)) {
                        //Debug.Fail("RemoveConnection: unknown connection.  OK, maybe opened by other program or BlueSoleil UI.");
                        Debug.WriteLine("RemoveConnection: unknown connection. OK, maybe third-party connection. connId: 0x" + conn_hdl.ToString("X"));
                    } else {
                        bool ret = _liveConns.Remove(conn_hdl);
                        Debug.Assert(ret, "NOT found in BS.RemoveConnection");
                    }
                    return _liveConns.Count;
                }
            }

            private int UseNetworkDisconnectEvent(BTCONNHDL conn_hdl)
            {
                lock (_liveConns) {
                    if (!_liveConns.ContainsKey(conn_hdl)) {
                        Debug.Fail("FireDisconnect");
                        //Debug.Fail("UseNetworkDisconnectEvent: unknown connection.  OK, maybe opened by other program or BlueSoleil UI.");
                        Debug.WriteLine("UseNetworkDisconnectEvent: unknown connection. OK, maybe third-party connection. connId: 0x" + conn_hdl.ToString("X"));
                    } else {
                        // Note: we don't remove it here.
                        IBluesoleilConnection conn = _liveConns[conn_hdl];
                        ThreadPool.QueueUserWorkItem(UseNetworkDisconnect_Runner, conn);
                    }
                    return _liveConns.Count;
                }
            }

            void UseNetworkDisconnect_Runner(object state)
            {
                var conn = (IBluesoleilConnection)state;
                conn.CloseNetworkOrInternal();
            }

            internal void CloseAnyLiveConnections()
            {
                ICollection<BTCONNHDL> hConnList;
                lock (_liveConns) {
                    hConnList = _liveConns.Keys;
                }
                int count = 0, okCount = 0; ;
                foreach (var hConn in hConnList) {
                    var ret = _api.Btsdk_Disconnect(hConn);
                    Debug.WriteLineIf(ret != BtSdkError.OK, "KillAllConnections: Disconnect: "
                        + BluesoleilUtils.BtSdkErrorToString(ret));
                    ++count;
                    if (ret == BtSdkError.OK) { ++okCount; }
                }//for
                try {
                    Debug.WriteLine($"BlueSoleil CloseAnyLiveConnections: count: {count}, okCount: {okCount}");
                } catch (ObjectDisposedException) {
                }
            }

        }
    }
}

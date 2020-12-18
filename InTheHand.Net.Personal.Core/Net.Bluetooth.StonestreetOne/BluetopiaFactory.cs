// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaFactory
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Sockets;
using Utils;


[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "InTheHand.Net.Bluetooth.StonestreetOne")]


namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaFactory : BluetoothFactory
    {
        readonly IBluetopiaApi _api;
        readonly IBluetopiaSecurity _sec;
        //
        uint _stackId;
        readonly BluetopiaInquiry _inquiryHandler;
        readonly NativeMethods.GAP_Event_Callback _HandleNameLookup;
        //
        // For TEST access only!
#if DEBUG
        internal //...
#endif
        NativeMethods.GAP_Event_Callback _inquiryEventCallback;
        //
        object _lockDevices = new object();
        Dictionary<BluetoothAddress, List<BluetopiaDeviceInfo>> _nameQueryList
            = new Dictionary<BluetoothAddress, List<BluetopiaDeviceInfo>>();
        Dictionary<BluetoothAddress, BluetopiaDeviceInfo> _knownDevices
            = new Dictionary<BluetoothAddress, BluetopiaDeviceInfo>();
        BluetopiaPacketDebug _pdebug;

        // From SS1PMAPN.pdf:
        // ( HCI_DRIVER_SET_EXTENDED_COMM_INFORMATION_DELAY_DRIVER_NAME_BAUD_RATE(&HCI_DriverInformation,
        //    6, 57600, cpUART_RTS_CTS, 500, 115200, TEXT("BTS"));
        // { COMPortNumber=6, BaudRate=57600, Protocol=cpUART_RTS_CTS,
        //    InitializationDelay=500, InitializationBaudRate=115200,
        //    COMMDriverInformation.DriverName="BTS" }
        //
        //readonly byte[] InitData = {
        //    0x00, 0x00, 0x00, 0x00, // DriverType = hdtCOMM -- COM/Serial Port HCI Connection Type.
        //    0x20, 0x00, 0x00, 0x00, //    DriverInformationSize
        //    0x06, 0x00, 0x00, 0x00, //    COMPortNumber
        //    0x00, 0xe1, 0x00, 0x00, //    BaudRate = 0x000e100 = 57600
        //    0x01, 0x00, 0x00, 0x00, //    Protocol = 1 = cpUART_RTS_CTS
        //    0xf4, 0x01, 0x00, 0x00, //    InitializationDelay    = 0x000001f4 = 500
        //    0x00, 0xc2, 0x01, 0x00, //    InitializationBaudRate = 0x0001c200 = 115200
        //    0x42, 0x00, 0x54, 0x00, //    DriverName = L"BTS"   B.T.
        //    0x53, 0x00, 0x00, 0x00  //                          S.\0
        //};
        //
        Structs.HCI_DriverInformation__HCI_COMMDriverInformation InitData2
            = Structs.HCI_DriverInformation__HCI_COMMDriverInformation.FromRegistry(); //new Structs.HCI_DriverInformation_t__HCI_COMMDriverInformation_t(
        // 6, 57600, StackConsts.HCI_COMM_Protocol_t.cpUART_RTS_CTS, 500, 115200, "BTS");

        //--------
        public BluetopiaFactory()
            : this(new BluetopiaRealApi())
        {
        }

        internal BluetopiaFactory(IBluetopiaApi api)
            : this(api, null)
        {
        }

        internal BluetopiaFactory(IBluetopiaApi api, IBluetopiaSecurity optionalSecurityInstance)
        {
            if (api == null) throw new ArgumentNullException("api");
            _api = api;
            //
            _inquiryHandler = new BluetopiaInquiry(this);
            _inquiryEventCallback = _inquiryHandler.HandleInquiryEvent;
            _HandleNameLookup = HandleNameLookup;
            //
            //int handle = _api.BSC_Initialize((byte[])InitData.Clone(), 0);
            Utils.MiscUtils.Trace_WriteLine(
                "Calling BSC_Initialize with:" + InitData2.ToString());
            int handle = _api.BSC_Initialize(ref InitData2, 0);
            //TEST--CheckStructBytes(ref InitData2, (byte[])InitData.Clone());
            var ret = (BluetopiaError)handle;
            if (!BluetopiaUtils.IsSuccessZeroIsIllegal(ret)) {
                KillBtExplorerExe();  // Since one app at a time!
                // _Quickly_ try to init, as BTExplorer.exe restarts automatically!
                handle = _api.BSC_Initialize(ref InitData2, 0);
                ret = (BluetopiaError)handle;
            }
            if (!BluetopiaUtils.IsSuccessZeroIsIllegal(ret)) {
                Utils.MiscUtils.Trace_WriteLine("Stonestreet One Bluetopia failed to init: "
                    + BluetopiaUtils.ErrorToString(ret));
            }
            BluetopiaUtils.CheckAndThrowZeroIsIllegal(ret, "BSC_Initialize");
            _stackId = checked((uint)handle);
            //
            if (optionalSecurityInstance != null)
                _sec = optionalSecurityInstance;
            else
                _sec = new BluetopiaSecurity(this);
            _sec.InitStack();
            //
            //
            var packetDebug = false;
            var eap = BluetoothFactoryConfig.GetEntryAssemblyPath();
            if (eap != null) { // null if we're under unit-test or...
                var dir = Path.GetDirectoryName(eap);
                var filename = Path.Combine(dir, "DoPacketDebug.txt");
                if (File.Exists(filename)) {
                    packetDebug = true;
                }
            }
            if (packetDebug) {
                _pdebug = new BluetopiaPacketDebug(this);
            }
            //
            InitApiVersion();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        void InitApiVersion()
        {
            try {
                var retFake = _api.GAP_Query_Local_Out_Of_Band_Data(StackId, IntPtr.Zero, IntPtr.Zero);
                Debug.Assert(retFake == BluetopiaError.INVALID_PARAMETER, "INFO: Expect to get error due to null pointers...");
                ApiVersion = NativeMethods.ApiVersion.Ssp;
            } catch (MissingMethodException) { // What NETCF P/Invoke throws.
                ApiVersion = NativeMethods.ApiVersion.PreSsp;
            } catch (Exception ex) {
                var msg = "Check API version (SSP) failed: " + ex;
                Debug.Fail(msg);
                Utils.MiscUtils.Trace_WriteLine(msg);
            }
        }

        [Conditional("DEBUG")]
        [Conditional("CODE_ANALYSIS")]
        private static void CheckStructBytes<T>(ref T stru, byte[] expected) where T : struct
        {
#if DEBUG
            int len = Marshal.SizeOf(stru);
            IntPtr p = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(stru, p, false);
            byte[] struBytes = new byte[len];
            Marshal.Copy(p, struBytes, 0, struBytes.Length);
            var bldr = new System.Text.StringBuilder();
            if (expected.Length != struBytes.Length) {
                bldr.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                    "Different lengths: expected: {0}, but is: {1}.",
                    expected.Length, struBytes.Length);
                bldr.Append("\r\n");
            }
            for (int i = 0; i < Math.Min(expected.Length, struBytes.Length); ++i) {
                if (expected[i] != struBytes[i]) {
                    bldr.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                        "Different bytes at {0}: expected: 0x{1:X2}, but is: 0x{2:X2}.",
                        i, expected[i], struBytes[i]);
                    bldr.Append("\r\n");
                }
            }//for
            if (bldr.Length != 0) {
#if ! NO_WINFORMS
                System.Windows.Forms.MessageBox.Show(bldr.ToString());
#else
                Debug.Fail(bldr.ToString());
#endif
            }
#endif
        }

        internal uint StackId
        {
            [DebuggerStepThrough]
            get { return _stackId; }
        }

        internal NativeMethods.ApiVersion ApiVersion { get; set; }

        //--
        protected override void Dispose(bool disposing)
        {
            if (_api != null && _stackId != 0) {
                Debug.WriteLine("BSC_Shutdown(" + _stackId + ")");
                _api.BSC_Shutdown(_stackId);
                if (_pdebug != null) _pdebug.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BluetopiaFactory()
        {
            Dispose(false);
        }

        //----
        internal IBluetopiaApi Api
        {
            [DebuggerStepThrough]
            get
            {
                //SdkInit();
                return _api;
            }
        }

        //----
        #region Factory Methods
        protected override IBluetoothRadio GetPrimaryRadio()
        {
            return new BluetopiaRadio(this);
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            return new IBluetoothRadio[] { GetPrimaryRadio() };
        }

        protected override IBluetoothClient GetBluetoothClient()
        {
            return new BluetopiaClient(this);
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            throw new NotSupportedException("This stack does not use Sockets.");
        }
        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStrm)
        {
            return new BluetopiaClient(this, acceptedStrm);
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            return new BluetopiaClient(this, localEP);
        }

        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            return BluetopiaDeviceInfo.CreateFromGivenAddress(address, this);
        }

        protected override IBluetoothListener GetBluetoothListener()
        {
            return new BluetopiaListener(this);
        }

        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            return _sec;
        }
        #endregion

        //----------------------------------------------------------------------
        class BluetopiaInquiry : CommonBluetoothInquiry<Structs.GAP_Inquiry_Entry_Event_Data>
        {
            readonly BluetopiaFactory _fcty;

            //----
            internal BluetopiaInquiry(BluetopiaFactory fcty)
            {
                _fcty = fcty;
            }

            //----
            protected override IBluetoothDeviceInfo CreateDeviceInfo(Structs.GAP_Inquiry_Entry_Event_Data item)
            {
                BluetopiaDeviceInfo bdi = BluetopiaDeviceInfo.CreateFromInquiry(item, _fcty);
                return bdi;
            }

            //----
            internal void HandleInquiryEvent(uint BluetoothStackID,
                ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter)
            {
                Debug.Assert(GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Inquiry_Entry_Result
                    || GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Inquiry_Result,
                    "Unexpected event type: " + GAP_Event_Data.Event_Data_Type);
                //
                Structs.GAP_Inquiry_Entry_Event_Data gapInquiryEntryEventData;
                Structs.GAP_Inquiry_Event_Data gapInquiryEventData;
                //
                if (GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Inquiry_Entry_Result) {
                    gapInquiryEntryEventData = (Structs.GAP_Inquiry_Entry_Event_Data)Marshal.PtrToStructure(
                        GAP_Event_Data.pData, typeof(Structs.GAP_Inquiry_Entry_Event_Data));
                    // TODO ThreadPool??? -- need to copy first if so.
                    HandleInquiryResultInd(gapInquiryEntryEventData);
                } else {
                    gapInquiryEventData = (Structs.GAP_Inquiry_Event_Data)Marshal.PtrToStructure(
                        GAP_Event_Data.pData, typeof(Structs.GAP_Inquiry_Event_Data));
                    // TODO ThreadPool??? -- need to copy first if so.
                    HandleInquiryCompleteInd(gapInquiryEventData);
                }
            }

            void HandleInquiryCompleteInd(Structs.GAP_Inquiry_Event_Data data)
            {
                base.HandleInquiryComplete(data.Number_Devices);
            }

        }//class

        internal IAsyncResult BeginInquiry(int maxDevices, TimeSpan inquiryLength,
            AsyncCallback callback, object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            CancelAllQueryNames();
            return _inquiryHandler.BeginInquiry(maxDevices, inquiryLength,
                callback, state,
                liveDiscoHandler, liveDiscoState,
                delegate() {
                    BluetopiaError ret = Api.GAP_Perform_Inquiry(StackId,
                        StackConsts.GAP_Inquiry_Type.GeneralInquiry,
                        0, 0, checked((uint)inquiryLength.Seconds),
                        checked((uint)maxDevices),
                        _inquiryEventCallback, 0);
                    BluetopiaUtils.CheckAndThrow(ret, "Btsdk_StartDeviceDiscovery");
                }, args);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<IBluetoothDeviceInfo> EndInquiry(IAsyncResult ar)
        {
            var list = _inquiryHandler.EndInquiry(ar);
            // We find on the M3Sky that if we initiate a Query_Remote_Device_Name
            // it kills Inquiry (see Inquiry_Result straightaway) so we block
            // its use (for inquired-devices) until now: release that and start query.
            list.ForEach(x => ((BluetopiaDeviceInfo)x).SetDiscoDComplete());
            // Wait some(!) time for Remote-Name to complete.  However this is
            // nowhere near long enough *if* there are many devices and one/more
            // do not respond to the name query.  So if one tries to starts a
            // Inquiry soon sometimes it will fail...
            Thread.Sleep(2500);
            Debug.WriteLine("EndInquiry after wait");
            return list;
        }

        //----------------------------------------------------------------------
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<IBluetoothDeviceInfo> GetRememberedDevices(bool authenticated, bool remembered)
        {
            List<IBluetoothDeviceInfo> clonedForLockSafety; // init below
            lock (_lockDevices) {
                clonedForLockSafety = new List<IBluetoothDeviceInfo>(_knownDevices.Count);
                foreach (var cur in _knownDevices) {
                    clonedForLockSafety.Add(cur.Value);
                }
            }//lock
            return clonedForLockSafety;
        }

        private void AddNamedKnownDevice(BluetoothAddress addr, string name)
        {
            var bdiNew = BluetopiaDeviceInfo.CreateFromGivenAddress(addr, this);
            bdiNew.SetRemembered();
            bdiNew.SetName(name);
            lock (_lockDevices) {
                BluetopiaDeviceInfo prev;
                var got = _knownDevices.TryGetValue(addr, out prev);
                if (prev != null) {
                    prev.SetName(name);
                } else {
                    _knownDevices.Add(bdiNew.DeviceAddress, bdiNew);

                }
            }//lock
        }


        //----------------------------------------------------------------------
        DateTime _lastCancelAllQueryNames;

        internal void CancelAllQueryNames()
        {
            ICollection<BluetoothAddress> addrList;
            lock (_lockDevices) {
                _lastCancelAllQueryNames = DateTime.UtcNow;
                addrList = _nameQueryList.Keys;
                _nameQueryList.Clear();
            }
            Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "CancelAllQueryNames: {0} devices.", addrList.Count));
            foreach (var cur in addrList) {
                var ret = NativeMethods.GAP_Cancel_Query_Remote_Device_Name(
                    this.StackId, BluetopiaUtils.BluetoothAddressAsInteger(cur));
                BluetopiaUtils.Assert(ret, "GAP_Cancel_Query_Remote_Device_Name");
            }
            if (addrList.Count > 0) {
                // Help wait for cancellation to complete?
                Thread.Sleep(250);
            }
        }

        internal BluetopiaError QueryName(BluetopiaDeviceInfo device,
            bool mayUseCached, bool mayQueryName)
        {
            const int CancelBlockingTimeSeconds = 2;
            lock (_lockDevices) {
                if (mayUseCached) {
                    BluetopiaDeviceInfo prev;
                    var got = _knownDevices.TryGetValue(device.DeviceAddress, out prev);
                    if (prev != null && prev.HasDeviceName) {
                        device.SetName(prev.DeviceName);
                        return BluetopiaError.OK;
                    }
                }
                //
                if (!mayQueryName) {
                    return BluetopiaError.OK;
                }
                var since = DateTime.UtcNow - _lastCancelAllQueryNames;
                if (since.TotalSeconds < CancelBlockingTimeSeconds) {
                    Debug.WriteLine("QueryName blocked");
                    return BluetopiaError.OK;
                } else { //DEBUG
                }
                List<BluetopiaDeviceInfo> instList;
                var exists = _nameQueryList.TryGetValue(device.DeviceAddress, out instList);
                if (instList == null) {
                    Debug.Assert(!exists, "Null Value!?!");
                    instList = new List<BluetopiaDeviceInfo>();
                    _nameQueryList.Add(device.DeviceAddress, instList);
                }
                if (instList.Contains(device)) {
                    // Already querying...
                    return BluetopiaError.OK;
                }
                instList.Add(device);
            }//lock
            var ret = _api.GAP_Query_Remote_Device_Name(this.StackId,
                BluetopiaUtils.BluetoothAddressAsInteger(device.DeviceAddress),
                _HandleNameLookup, 0);
            BluetopiaUtils.WriteLineIfError(ret, "GAP_Query_Remote_Device_Name");
            return ret;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Mustn't let exception return to stack.")]
        void HandleNameLookup(uint BluetoothStackID,
            ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter)
        {
            try {
                HandleNameLookup2(BluetoothStackID,
                    ref GAP_Event_Data, CallbackParameter);
            } catch (Exception ex) {
                Utils.MiscUtils.Trace_WriteLine("Exception from our HandleNameLookup2!!!: " + ex);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "BluetoothStackID")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "CallbackParameter")]
        void HandleNameLookup2(uint BluetoothStackID,
            ref Structs.GAP_Event_Data GAP_Event_Data, uint CallbackParameter)
        {
            Debug.Assert(GAP_Event_Data.Event_Data_Type == StackConsts.GAP_Event_Type.Remote_Name_Result,
                "Unexpected event type: " + GAP_Event_Data.Event_Data_Type);
            //
            Structs.GAP_Remote_Name_Event_Data data;
            //
            data = (Structs.GAP_Remote_Name_Event_Data)Marshal.PtrToStructure(
                GAP_Event_Data.pData, typeof(Structs.GAP_Remote_Name_Event_Data));
            var addr = BluetopiaUtils.ToBluetoothAddress(data._Remote_Device);
            Debug.WriteLine("GAP_Remote_Name_Event_Data: status: " + data._Remote_Name_Status
                + ", addr: " + addr);
            if (data._Remote_Name_Status != 0) {
                return;
            }
            // TODO ThreadPool??? but need to marshal data._Remote_Name first.
            List<BluetopiaDeviceInfo> queryList;
            lock (_lockDevices) {
                var got = _nameQueryList.TryGetValue(addr, out queryList);
                var gotR = _nameQueryList.Remove(addr);
            }
            // TO-DO  if (list == null) return;
            var arr = Widcomm.WidcommUtils.GetByteArrayNullTerminated(data._Remote_Name, 250);
            var name = BluetopiaUtils.FromNameString(arr);
            Debug.WriteLine("  name: " + name);
            if (queryList == null) return; // duplicate above exit.
            ThreadPool.QueueUserWorkItem(
                delegate {
                    foreach (var cur in queryList) {
                        cur.SetName(name);
                    }
                    //_inquiryHandler.GotNameManually(addr, name);
                    AddNamedKnownDevice(addr, name);
                });
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// The following named event may be used to determine when events
        /// occur having to do with Bluetooth Device Power.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The SYMB_Get_Power_Event() function may be used to determined that
        /// last event that caused the setting of this event.
        /// </para>
        /// <para>When creating
        /// this named event create it with ManualReset set to FALSE and
        /// InitialState set to FALSE.
        /// </para>
        /// </remarks>
        const string SYMB_POWER_INFORMATION_EVENT = "Symbol/Bluetooth/Utility/PowerInformationEvent";

        //----------------------------------------------------------------------
        private static void KillBtExplorerExe()
        {
            var list = Process2.GetProcessesByName("BTExplorer");
            Debug.Assert(list.Length == 0 || list.Length == 1);
            foreach (var cur in list) {
                cur.Kill();
                Debug.WriteLine("Kill BTExplorer.exe");
            }
        }

    }
}

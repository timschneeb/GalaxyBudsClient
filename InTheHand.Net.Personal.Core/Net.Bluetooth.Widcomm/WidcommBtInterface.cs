// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2013 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2013 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using InTheHand.Net.Sockets;
using System.Diagnostics;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using InTheHand.Net.Bluetooth.Factory;
using List_IBluetoothDeviceInfo = System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>;
using AR_Inquiry = InTheHand.Net.AsyncResult<System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>>;
using System.Threading;
using AsyncResultDD = InTheHand.Net.AsyncResult<
    System.Collections.Generic.List<InTheHand.Net.Bluetooth.Factory.IBluetoothDeviceInfo>,
    InTheHand.Net.Bluetooth.Factory.DiscoDevsParams>;
using System.Globalization;
using System.IO;


namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed class WidcommBtInterface : IDisposable
    {
        volatile bool _disposed;
        readonly WidcommBluetoothFactoryBase m_factory;
        readonly IBtIf m_btIf;
        readonly WidcommInquiry _inquiryHandler;


        internal WidcommBtInterface(IBtIf btIf, WidcommBluetoothFactoryBase factory)
        {
            m_factory = factory;
            _inquiryHandler = new WidcommInquiry(m_factory, StopInquiry);
            bool created = false;
            try {
                m_btIf = btIf;
                m_btIf.SetParent(this);
                // "An object of this class must be instantiated before any other DK classes are used"
                m_btIf.Create();
                created = true;
            } finally {
                if (!created) { GC.SuppressFinalize(this); }
            }
        }

        private void EnsureLoaded()
        {
            if (!_disposed) // All is good.
                return;
            m_factory.EnsureLoaded();
        }

        //----
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WidcommBtInterface()
        {
            Dispose(false);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
        void Dispose(bool disposing)
        {
            _disposed = true;
            m_btIf.Destroy(disposing);
        }

        //-----------------------------

        //----------
        internal IAsyncResult BeginInquiry(int maxDevices, TimeSpan inquiryLength,
            AsyncCallback asyncCallback, Object state,
            BluetoothClient.LiveDiscoveryCallback liveDiscoHandler, object liveDiscoState,
            DiscoDevsParams args)
        {
            return _inquiryHandler.BeginInquiry(maxDevices, inquiryLength,
                asyncCallback, state,
                liveDiscoHandler, liveDiscoState,
                StartInquiry, args);
        }

        private void StartInquiry() // We are inside the lock.
        {
            // BTW InquiryLength is set-up in BeginInquiry.
            Debug.WriteLine(WidcommUtils.GetTime4Log() + ": calling StartInquiry.");
            bool success = m_btIf.StartInquiry();
            Debug.WriteLine(WidcommUtils.GetTime4Log() + ": StartInquiry ret: " + success);
            if (!success)
                throw CommonSocketExceptions.Create_StartInquiry("StartInquiry");
        }

        void StopInquiry()
        {
            Utils.MiscUtils.Trace_WriteLine("StopInquiry");
            Debug.WriteLine(WidcommUtils.GetTime4Log() + ": StopInquiry done");
            m_btIf.StopInquiry();
        }


        class WidcommInquiry : CommonBluetoothInquiry<IBluetoothDeviceInfo>
        {
            readonly WidcommBluetoothFactoryBase m_factory;
            ThreadStart _stopInquiry;

            //----
            internal WidcommInquiry(WidcommBluetoothFactoryBase factory, ThreadStart stopInquiry)
            {
                m_factory = factory;
                _stopInquiry = stopInquiry;
            }

            //----
            protected override IBluetoothDeviceInfo CreateDeviceInfo(IBluetoothDeviceInfo item)
            {
                return item;
            }
            //----
            internal void HandleDeviceResponded(byte[] bdAddr, byte[] devClass,
                byte[] deviceName, bool connected)
            {
                Utils.MiscUtils.Trace_WriteLine("HandleDeviceResponded");
                var bdi = WidcommBluetoothDeviceInfo.CreateFromHandleDeviceResponded(
                    bdAddr, deviceName, devClass, connected, m_factory);
                Utils.MiscUtils.Trace_WriteLine("HDR: {0} {1} {2} {3}",
                    ToStringQuotedOrNull(bdAddr), ToStringQuotedOrNull(devClass),
                    ToStringQuotedOrNull(deviceName), connected);
                HandleInquiryResultInd(bdi);
                Utils.MiscUtils.Trace_WriteLine("exit HDR");
            }

            private static string ToStringQuotedOrNull(byte[] array)
            {
                if (array == null)
                    return "(null)";
                else
                    return "\"" + BitConverter.ToString(array) + "\"";
            }

            internal void HandleInquiryComplete(bool success, UInt16 numResponses)
            {
                Utils.MiscUtils.Trace_WriteLine("HandleInquiryComplete");
                HandleInquiryComplete(numResponses);
                Utils.MiscUtils.Trace_WriteLine("exit HandleInquiryComplete");
            }

            protected override void StopInquiry()
            {
                _stopInquiry();
            }
        }//class

        internal List_IBluetoothDeviceInfo EndInquiry(IAsyncResult ar)
        {
            // (Can't lock here as that would block the callback methods).
            // Check is one of queued ar.  However this function is only called from 
            // inside BluetoothClient.DiscoverDevices so we can be less careful/helpful!!
            AR_Inquiry ar2 = (AR_Inquiry)ar;
            return ar2.EndInvoke();
        }

        internal void HandleDeviceResponded(byte[] bdAddr, byte[] devClass,
            byte[] deviceName, bool connected)
        {
            _inquiryHandler.HandleDeviceResponded(bdAddr, devClass, deviceName, connected);
        }

        internal void HandleInquiryComplete(bool success, UInt16 numResponses)
        {
            _inquiryHandler.HandleInquiryComplete(success, numResponses);
        }

        //----------
        const int MaxNumberSdpRecords = 10;
        //
        object lockServiceDiscovery = new object();
        AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> m_arServiceDiscovery;

        public IAsyncResult BeginServiceDiscovery(BluetoothAddress address, Guid serviceGuid, SdpSearchScope searchScope,
            AsyncCallback asyncCallback, Object state)
        {
            BeginServiceDiscoveryKillInquiry();
            // Just in case the user modifies the original address!!!
            BluetoothAddress addr2 = (BluetoothAddress)address.Clone();
            AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> ar
                = new AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams>(asyncCallback, state,
                    new ServiceDiscoveryParams(addr2, serviceGuid, searchScope));
            lock (lockServiceDiscovery) {
                if (m_arServiceDiscovery != null)
                    throw new NotSupportedException("Currently support only one concurrent Service Lookup operation.");
                bool success = false;
                try {
                    m_arServiceDiscovery = ar;
                    bool ret = m_btIf.StartDiscovery(addr2, serviceGuid);
                    Debug.WriteLine(WidcommUtils.GetTime4Log() + ": StartDiscovery ret: " + ret);
                    if (!ret) {
                        WBtRc ee = GetExtendedError();
                        throw WidcommSocketExceptions.Create_StartDiscovery(ee);
                    }
                    success = true;
                } finally {
                    if (!success)
                        m_arServiceDiscovery = null;
                }
            }
            return ar;
        }

        private void BeginServiceDiscoveryKillInquiry()
        {
            Utils.MiscUtils.Trace_WriteLine("BeginServiceDiscovery gonna call StopInquiry.");
            //IAsyncResult arDD = m_arInquiry;
            //var whDD = arDD == null ? null : arDD.AsyncWaitHandle;
            StopInquiry();
            //if (whDD != null) {
            // We NEVER saw the Inquiry Complete event come from Widcomm no
            // matter how long we waited, so don't bother with this wait.
            // Leave the StopInquiry in case it is doing some good...
            //const int KillInquiryWaitTimeout = 5 * 1000;
            //bool completed = whDD.WaitOne(KillInquiryWaitTimeout, false);
            //Utils.MiscUtils.Trace_WriteLine("BeginServiceDiscovery saw _arInquiry complete: {0}.", completed);
            //}
        }

        public ISdpDiscoveryRecordsBuffer EndServiceDiscovery(IAsyncResult asyncResult)
        {
            AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> checkTypeMatches = m_arServiceDiscovery;
            AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> ar2
                = (AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams>)asyncResult;
            return ar2.EndInvoke();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "will rethrow")]
        internal void HandleDiscoveryComplete()
        {
            Utils.MiscUtils.Trace_WriteLine("HandleDiscoveryComplete");
            AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> sacAr = null;
            ISdpDiscoveryRecordsBuffer recBuf = null;
            Exception sacEx = null;
            try {
                lock (lockServiceDiscovery) {
                    Debug.Assert(m_arServiceDiscovery != null, "NOT m_arServiceDiscovery != null");
                    if (m_arServiceDiscovery == null) { return; } // Nothing we can do then!
                    sacAr = m_arServiceDiscovery;
                    m_arServiceDiscovery = null;
                    BluetoothAddress addr;
                    ushort numRecords0;
                    DISCOVERY_RESULT result = m_btIf.GetLastDiscoveryResult(out addr, out numRecords0);
                    if (result != DISCOVERY_RESULT.SUCCESS) {
                        sacEx = WidcommSocketExceptions.Create(result, "ServiceRecordsGetResult");
                        return;
                    }
                    if (!addr.Equals(sacAr.BeginParameters.address)) {
                        sacEx = new InvalidOperationException("Internal error -- different DiscoveryComplete address.");
                        return;
                    }
                    // Get the records
                    recBuf = m_btIf.ReadDiscoveryRecords(addr, MaxNumberSdpRecords,
                        sacAr.BeginParameters);
                }//lock
            } catch (Exception ex) {
                sacEx = ex;
            } finally {
                Debug.Assert(sacAr != null, "out: NOT sacAr != null");
                Debug.Assert(m_arServiceDiscovery == null, "out: NOT m_arServiceDiscovery == null");
                WaitCallback dlgt = delegate {
                    RaiseDiscoveryComplete(sacAr, recBuf, sacEx);
                };
                ThreadPool.QueueUserWorkItem(dlgt);
            }
        }

        static void RaiseDiscoveryComplete(
            AsyncResult<ISdpDiscoveryRecordsBuffer, ServiceDiscoveryParams> sacAr,
            ISdpDiscoveryRecordsBuffer recBuf, Exception sacEx)
        {
            if (sacAr != null) { // will always be true!
                if (sacEx != null) {
                    sacAr.SetAsCompleted(sacEx, false);
                } else {
                    sacAr.SetAsCompleted(recBuf, false);
                }
            }
        }

        //----------
        public List_IBluetoothDeviceInfo GetKnownRemoteDeviceEntries()
        {
            List<REM_DEV_INFO> list = new List<REM_DEV_INFO>();
            REM_DEV_INFO info = new REM_DEV_INFO();
            int cb = System.Runtime.InteropServices.Marshal.SizeOf(typeof(REM_DEV_INFO));
            IntPtr pBuf = System.Runtime.InteropServices.Marshal.AllocHGlobal(cb);
            try {
                REM_DEV_INFO_RETURN_CODE ret = m_btIf.GetRemoteDeviceInfo(ref info, pBuf, cb);
                Utils.MiscUtils.Trace_WriteLine("GRDI: ret: {0}=0x{0:X}", ret);
                while (ret == REM_DEV_INFO_RETURN_CODE.SUCCESS) {
                    list.Add(info); // COPY it into the list
                    ret = m_btIf.GetNextRemoteDeviceInfo(ref info, pBuf, cb);
                    Utils.MiscUtils.Trace_WriteLine("GnRDI: ret: {0}=0x{0:X}", ret);
                }//while
                if (ret != REM_DEV_INFO_RETURN_CODE.EOF)
                    throw WidcommSocketExceptions.Create(ret, "Get[Next]RemoteDeviceInfo");
                //
                List_IBluetoothDeviceInfo bdiList = new List_IBluetoothDeviceInfo(list.Count);
                foreach (REM_DEV_INFO cur in list) {
                    IBluetoothDeviceInfo bdi = WidcommBluetoothDeviceInfo.CreateFromStoredRemoteDeviceInfo(cur, m_factory);
                    bdiList.Add(bdi);
                }
                return bdiList;
            } finally {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(pBuf);
            }
        }

        //----------
        const string DevicesRegPath = @"Software\WIDCOMM\BTConfig\Devices\"; // "HKEY_LOCAL_MACHINE\..."

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static string GetWidcommDeviceKeyName(BluetoothAddress device)
        {
            // See ReadDeviceFromRegistryAndCheckAndSetIfPaired_
            return device.ToString("C").ToLower(CultureInfo.InvariantCulture);
        }

        public List_IBluetoothDeviceInfo ReadKnownDevicesFromRegistry()
        {
            // Multiple keys, one per device, named with address e.g. 00:11:22:33:44:55
            // Each with values: BRCMStack DWORD, Code DWORD, DevClass DWORD, etc etc
            //
            List_IBluetoothDeviceInfo devices = new List_IBluetoothDeviceInfo();
            using (RegistryKey rkDevices = Registry.LocalMachine.OpenSubKey(DevicesRegPath)) {
                if (rkDevices == null) {
                    // The Registry key is created when the first device is stored, 
                    // so on a new device it doesn't exist. So return an empty list.
                    return devices;
                    // IOException is what GetValueKind throws.
                    //throw new System.IO.IOException("Widcomm 'Devices' key not found in the Registry.");
                }
                foreach (string itemName in rkDevices.GetSubKeyNames()) {
                    using (RegistryKey rkItem = rkDevices.OpenSubKey(itemName)) {
                        WidcommBluetoothDeviceInfo bdi = ReadDeviceFromRegistryAndCheckAndSetIfPaired_(
                            itemName, rkItem, m_factory);
                        devices.Add(bdi);
                    }
                }//for
            }
            return devices;
        }

        private WidcommBluetoothDeviceInfo ReadDeviceFromRegistryAndCheckAndSetIfPaired_(
            string itemName, RegistryKey rkItem, WidcommBluetoothFactoryBase factory)
        {
            BluetoothAddress address = BluetoothAddress.Parse(itemName);
            Debug.Assert(GetWidcommDeviceKeyName(address).Equals(itemName, StringComparison.OrdinalIgnoreCase),
                "itemName not colons?: " + itemName);
            //
            byte[] devName, devClass ;
            try {
                devName = Registry_ReadBinaryValue(rkItem, "Name");
                devClass = Registry_ReadBinaryValue(rkItem, "DevClass");
            } catch (IOException) { // "The specified registry key does not exist."
                Debug.WriteLine("Partial device info in Registry for: {0}.", itemName);
                return null;
            }
            Int32? trusted = Registry_ReadDwordValue_Optional(rkItem, "TrustedMask");
            WidcommBluetoothDeviceInfo bdi = CreateFromStoredRemoteDeviceInfo(address, devName, devClass, factory);
            WidcommBluetoothDeviceInfo.CheckAndSetIfPaired(bdi, factory);
            return bdi;
        }

        internal WidcommBluetoothDeviceInfo ReadDeviceFromRegistryAndCheckAndSetIfPaired(BluetoothAddress address,
            WidcommBluetoothFactoryBase factory)
        {
            using (RegistryKey rkDevices = Registry.LocalMachine.OpenSubKey(DevicesRegPath)) {
                if (rkDevices == null) {
                    // The Registry key is created when the first device is stored, 
                    // so on a new device it doesn't exist. So return an empty list.
                    return null;
                }
                string itemName = address.ToString("C");
                using (RegistryKey rkItem = rkDevices.OpenSubKey(itemName)) {
                    if (rkItem == null) {
                        return null;
                    }
                    WidcommBluetoothDeviceInfo bdi = ReadDeviceFromRegistryAndCheckAndSetIfPaired_(itemName, rkItem, factory);
                    return bdi;
                }
            }
        }

        private static WidcommBluetoothDeviceInfo CreateFromStoredRemoteDeviceInfo(
            BluetoothAddress devAddress, byte[] devName, byte[] devClass,
            WidcommBluetoothFactoryBase factory)
        {
            REM_DEV_INFO rdi = new REM_DEV_INFO();
            rdi.bda = WidcommUtils.FromBluetoothAddress(devAddress);
            rdi.bd_name = devName;
            rdi.dev_class = devClass;
            // rdi.b_connected = ...
            // rdi.b_paired = ...
            WidcommBluetoothDeviceInfo bdi = WidcommBluetoothDeviceInfo.CreateFromStoredRemoteDeviceInfo(rdi, factory);
            string nameStr = bdi.DeviceName;
            Debug.Assert(nameStr.Length == 0 || nameStr[nameStr.Length - 1] != 0, "null terminator!!");
            int idxDbg;
            Debug.Assert((idxDbg = nameStr.IndexOf((char)0)) == -1, "null terminator!! at: " + idxDbg);
            return bdi;
        }

        private static byte[] Registry_ReadBinaryValue(RegistryKey rkItem, string name)
        {
            Registry_CheckIsKind(rkItem, name, RegistryValueKind.Binary);
            byte[] raw = (byte[])rkItem.GetValue(name);
            return raw;
        }

        private static Int32? Registry_ReadDwordValue_Optional(RegistryKey rkItem, string name)
        {
            object val = rkItem.GetValue(name);
            if (val == null)
                return null;
            Registry_CheckIsKind(rkItem, name, RegistryValueKind.DWord);
            return (Int32)val;
        }

        private static void Registry_CheckIsKind(RegistryKey rkItem, string name, RegistryValueKind expectedKind)
        {
            if (PlatformVerification.IsMonoRuntime) {
                Utils.MiscUtils.Trace_WriteLine("Skipping Registry_CheckIsKind check on Mono as it's not supported.");
                return;
            }
            RegistryValueKind kind = rkItem.GetValueKind(name);
            if (kind != expectedKind) {
                string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Expected '{0}':'{1}', to be '{2}' but was '{3}'.",
                    rkItem.Name, name, expectedKind, kind);
                throw new FormatException(msg);
            }
        }

        /// <summary>
        /// Remove the device by deleting it from the Registry.
        /// </summary>
        /// <param name="device">The device address.</param>
        /// <returns>Whether the device is deleted -- it is no longer a remembered device.
        /// </returns>
        internal static bool DeleteKnownDevice(BluetoothAddress device)
        {
            string itemName = GetWidcommDeviceKeyName(device);
            using (RegistryKey rkDevices = Registry.LocalMachine.OpenSubKey(DevicesRegPath, true)) {
                using (RegistryKey theOne = rkDevices.OpenSubKey(itemName, false)) {
                    if (theOne == null) // Isn't present.
                        return true;
                }
                try {
                    rkDevices.DeleteSubKeyTree(itemName);
                    return true;
                } catch (System.Security.SecurityException ex) {
                    Utils.MiscUtils.Trace_WriteLine("DeleteKnownDevice DeleteSubKeyTree(" + itemName + "): "
                        + ExceptionExtension.ToStringNoStackTrace(ex));
                } catch (UnauthorizedAccessException ex) {
                    Utils.MiscUtils.Trace_WriteLine("DeleteKnownDevice DeleteSubKeyTree(" + itemName + "): "
                        + ExceptionExtension.ToStringNoStackTrace(ex));
                }
                return false;
            }
        }

        //----------
        internal bool GetLocalDeviceVersionInfo(ref DEV_VER_INFO m_dvi)
        {
            return m_btIf.GetLocalDeviceVersionInfo(ref m_dvi);
        }

        internal bool GetLocalDeviceInfoBdAddr(byte[] bdAddr)
        {
            return m_btIf.GetLocalDeviceInfoBdAddr(bdAddr);
        }

        internal bool GetLocalDeviceName(byte[] bdName)
        {
            return m_btIf.GetLocalDeviceName(bdName);
        }

        internal void IsStackUpAndRadioReady(out bool stackServerUp, out bool deviceReady)
        {
            m_btIf.IsStackUpAndRadioReady(out stackServerUp, out deviceReady);
        }

        internal void IsDeviceConnectableDiscoverable(out bool conno, out bool disco)
        {
            m_btIf.IsDeviceConnectableDiscoverable(out conno, out disco);
        }

        internal void SetDeviceConnectableDiscoverable(bool connectable, bool forPairedOnly, bool discoverable)
        {
            if (connectable || discoverable) {
                EnsureLoaded();
            }
            m_btIf.SetDeviceConnectableDiscoverable(connectable, forPairedOnly, discoverable);
        }

        internal int GetRssi(byte[] bd_addr)
        {
            return m_btIf.GetRssi(bd_addr);
        }

        internal bool BondQuery(byte[] bd_addr)
        {
            return m_btIf.BondQuery(bd_addr);
        }

        internal BOND_RETURN_CODE Bond(BluetoothAddress address, string passphrase)
        {
            return m_btIf.Bond(address, passphrase);
        }

        internal bool UnBond(BluetoothAddress address)
        {
            return m_btIf.UnBond(address);
        }

        //----------
        /// <summary>
        /// Call CBtIf::GetExtendedError.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>Is not currently used anywhere...
        /// </para>
        /// <para>Not supported on Widcomm WCE WM/WinCE, we (natively) return -1.
        /// </para>
        /// </remarks>
        /// -
        /// <returns>A <see cref="T:InTheHand.Net.Bluetooth.Widcomm.WBtRc"/> value.</returns>
        private WBtRc GetExtendedError()
        {
            return m_btIf.GetExtendedError();
        }

        /// <summary>
        /// CBtIf::IsRemoteDevicePresent
        /// </summary>
        /// -
        /// <remarks>
        /// <note>"added BTW and SDK 5.0.1.1000"</note>
        /// <note>"added BTW-CE and SDK 1.7.1.2700"</note>
        /// </remarks>
        internal SDK_RETURN_CODE IsRemoteDevicePresent(byte[] bd_addr)
        {
            return m_btIf.IsRemoteDevicePresent(bd_addr);
        }

        /// <summary>
        /// CBtIf::IsRemoteDeviceConnected
        /// </summary>
        /// -
        /// <remarks>
        /// <note>"added BTW 5.0.1.300, SDK 5.0"</note>
        /// <note>"added BTW-CE and SDK 1.7.1.2700"</note>
        /// </remarks>
        internal bool IsRemoteDeviceConnected(byte[] bd_addr)
        {
            return m_btIf.IsRemoteDeviceConnected(bd_addr);
        }

        //----------
        [DebuggerStepThrough]
        public static bool IsWidcommSingleThread(WidcommPortSingleThreader st)
        {
            return WidcommPortSingleThreader.IsWidcommSingleThread(st);
        }

        static T GetStaticData<T>(LocalDataStoreSlot slot) where T : struct
        {
            object o = Thread.GetData(slot);
            T v = o == null ? default(T) : (T)o;
            return v;
        }

    }//class


    internal enum SdpSearchScope
    {
        Anywhere,
        ServiceClassOnly
    }


    sealed class ServiceDiscoveryParams
    {
        readonly internal BluetoothAddress address;
        readonly internal Guid serviceGuid;
        readonly internal SdpSearchScope searchScope;

        internal ServiceDiscoveryParams(BluetoothAddress address, Guid serviceGuid, SdpSearchScope searchScope)
        {
            this.address = address;
            this.serviceGuid = serviceGuid;
            if (searchScope != SdpSearchScope.Anywhere
                    && searchScope != SdpSearchScope.ServiceClassOnly)
                throw new ArgumentException("Unrecognized value for SdpSearchScope enum.", "searchScope");
            this.searchScope = searchScope;
        }
    }

}

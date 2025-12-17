// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBtIf
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class WidcommBtIf : IBtIf
    {
        private static class NativeMethods
        {
            internal delegate void OnDeviceResponded(IntPtr/*byte[]*/ bdAddr,
                IntPtr/*byte[]*/ devClass, IntPtr/*byte[]*/ deviceName, bool connected);
            internal delegate void OnInquiryComplete(bool success, UInt16 numResponses);
            internal delegate void OnDiscoveryComplete();
            internal delegate void OnStackStatusChange(int new_status);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_Create(out IntPtr ppBtIf,
                OnDeviceResponded deviceResponded, OnInquiryComplete handleInquiryCompleted,
                OnDiscoveryComplete handleDiscoveryComplete);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern MY32FEET_AutoReconnectErrors BtIf_SetAutoReconnect(IntPtr pObj,
                [MarshalAs(UnmanagedType.Bool)] bool autoReconnect);
            internal enum MY32FEET_AutoReconnectErrors
            {
                SUCCESS = 0,
                NO_FUNCTION,
                CALL_FAILED
            }

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int BtIf_SetCallback2(IntPtr pBtIf,
                int num, OnStackStatusChange stackStatusChange);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_StartInquiry(IntPtr pBtIf);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern void BtIf_StopInquiry(IntPtr pBtIf);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_StartDiscovery(IntPtr pObj,
                byte[] p_bda, ref Guid p_service_guid,
                out int sizeofSdpDiscoveryRec);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern DISCOVERY_RESULT BtIf_GetLastDiscoveryResult(IntPtr pObj,
                [Out]byte[] p_bda, out UInt16 p_num_recs);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int BtIf_ReadDiscoveryRecords(IntPtr pObj,
                byte[] p_bda, int max_size, out IntPtr pSdpDiscoveryRecArray);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int BtIf_ReadDiscoveryRecordsServiceClassOnly(IntPtr pObj,
                byte[] p_bda, int max_size, out IntPtr pSdpDiscoveryRecArray,
                ref Guid p_guid_filter);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern REM_DEV_INFO_RETURN_CODE BtIf_GetRemoteDeviceInfo(IntPtr pObj,
                IntPtr p_rem_dev_info, int cb);
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern REM_DEV_INFO_RETURN_CODE BtIf_GetNextRemoteDeviceInfo(IntPtr pObj,
                IntPtr p_rem_dev_info, int cb);

            //----
            [DllImport(WidcommNativeBits.WidcommDll
                //, EntryPoint="TEST_MISSING_BtIf_GetLocalDeviceVersionInfo"
                )]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_GetLocalDeviceVersionInfo(IntPtr pObj,
                ref DEV_VER_INFO pBuf, int cb);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_GetLocalDeviceInfoBdAddr(IntPtr m_pBtIf,
                [Out]byte[] bdAddr, int cb);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_GetLocalDeviceName(IntPtr pObj,
                [Out]byte[] pBdName, int cb);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_IsStackUpAndRadioReady(IntPtr pObj,
                [MarshalAs(UnmanagedType.Bool)] out bool stackServerUp,
                [MarshalAs(UnmanagedType.Bool)] out bool deviceReady);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_IsDeviceConnectableDiscoverable(IntPtr pObj,
                [MarshalAs(UnmanagedType.Bool)] out bool conno,
                [MarshalAs(UnmanagedType.Bool)] out bool disco);

#if !WinXP
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_SetDeviceConnectableDiscoverable(IntPtr pObj,
                [MarshalAs(UnmanagedType.Bool)] bool connectable,
                [MarshalAs(UnmanagedType.Bool)] bool forPairedOnly,
                [MarshalAs(UnmanagedType.Bool)] bool discoverable);
#endif

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_GetConnectionStats(IntPtr pObj,
                byte[] bdAddr, out tBT_CONN_STATS pStats, int cb);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_BondQuery(IntPtr pObj,
                byte[] bdAddr);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int BtIf_Bond(IntPtr pObj,
                byte[] bdAddr, byte[] pin_code);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_UnBond(IntPtr pObj,
                byte[] bdAddr);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern uint BtIf_GetExtendedError(IntPtr pBtIf);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern int BtIf_IsRemoteDevicePresent(IntPtr pObj, byte[] bdAddr);

            [DllImport(WidcommNativeBits.WidcommDll)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool BtIf_IsRemoteDeviceConnected(IntPtr pObj, byte[] bdAddr);

            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_IsRemoteDevicePresentConnected(IntPtr pObj, byte[] bdAddr,
                out int pPresent, [MarshalAs(UnmanagedType.Bool)] out bool pConnected);

            //----
            [DllImport(WidcommNativeBits.WidcommDll)]
            internal static extern void BtIf_Destroy(IntPtr pBtIf);

            //--------
#if NETCF
            const string KernelCoreLibrary = "coredll.dll";
#else
            const string KernelCoreLibrary = "kernel32.dll";
#endif
            //[DllImport(KernelCoreLibrary, CharSet = CharSet.Unicode)]
            //internal static extern IntPtr LoadLibrary(string fileName);

            [DllImport(KernelCoreLibrary, CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr LoadLibraryEx(string fileName, IntPtr hFile, LoadLibraryExFlags dwFlags);

            [DllImport(KernelCoreLibrary, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool FreeLibrary(IntPtr hModule);

            [DllImport(KernelCoreLibrary, CharSet = CharSet.Unicode)]
            internal static extern IntPtr GetModuleHandle(string moduleName);

            [Flags]
            internal enum LoadLibraryExFlags
            {
                DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
                LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
                LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,
                LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
                //
                VISTA_AND_LATER__LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            }

        }

        // From the Widcomm documentation:
        //   An object of this class must be instantiated before any other DK 
        //   classes are used (typically at application startup). An object
        //   of this class should not be deleted until the application has 
        //   finished all interactions with the stack (typically at application
        //   exit). *** Only one object of this class should be instantiated by 
        //   the application. ***
        static volatile bool s_alreadyExists;
        IntPtr m_pBtIf;
        // So do we need a list of parents (as WeakReferences) and notify each on 
        // each event?
        WidcommBtInterface m_parent;
        readonly WidcommBluetoothFactory _factory;
        //
        // Stop the delegates being GC'd, as the native code is calling their thunks.
        NativeMethods.OnDeviceResponded m_handleDeviceResponded;
        NativeMethods.OnInquiryComplete m_handleInquiryComplete;
        NativeMethods.OnDiscoveryComplete m_handleDiscoveryComplete;
        NativeMethods.OnStackStatusChange m_handleStackStatusChange;


        internal WidcommBtIf(WidcommBluetoothFactory fcty)
        {
            if (s_alreadyExists)
                throw new InvalidOperationException("May only be one instance of WidcommBtIf.");
            s_alreadyExists = true;
            _factory = fcty;
        }

        public void SetParent(WidcommBtInterface parent)
        {
            if (m_parent != null)
                throw new InvalidOperationException("Can only have one parent.");
            m_parent = parent;
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Is rethrown by method call CheckDependencies.")]
        public void Create()
        {
            if (m_pBtIf != IntPtr.Zero)// Singleton!
                throw new InvalidOperationException("Create already called.");
            m_handleDeviceResponded = HandleDeviceResponded;
            m_handleInquiryComplete = HandleInquiryComplete;
            m_handleDiscoveryComplete = HandleDiscoveryComplete;
            m_handleStackStatusChange = HandleStackStatusChange;
            bool success = false;
            try {
                NativeMethods.BtIf_Create(out m_pBtIf, m_handleDeviceResponded, m_handleInquiryComplete,
                    m_handleDiscoveryComplete);
                if (m_pBtIf == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to initialise CBtIf.");
                success = true;
            } catch (Exception ex) {
                CheckDependencies(ex);
            } finally {
                if (!success) {
                    Debug.Assert(m_pBtIf == IntPtr.Zero, "but failed!");
                    s_alreadyExists = false;
                }
            }
            SetCallback2();
            //SetAutoReconnect();
#if DEBUG
            CheckDependencies(null);
#endif
        }

#if false
        private void SetAutoReconnect()
        {
            const bool AssertForMissingSetAutoReconnect = false;
            try {
                NativeMethods.MY32FEET_AutoReconnectErrors ret
                    = NativeMethods.BtIf_SetAutoReconnect(m_pBtIf, true);
                Debug.WriteLine("BtIf_SetAutoReconnect ret: " + ret);
#if NETCF
                Debug.Assert(ret == NativeMethods.MY32FEET_AutoReconnectErrors.SUCCESS,
                    "BtIf_SetAutoReconnect ret: " + ret);
#else
                Debug.Assert(ret == NativeMethods.MY32FEET_AutoReconnectErrors.NO_FUNCTION,
                    "BtIf_SetAutoReconnect ret: " + ret + " expected NO_FUNCTION");
#endif
            } catch (EntryPointNotFoundException ex) {
                ReportNeedNeedNativeDllUpgrade(ex, AssertForMissingSetAutoReconnect);
            } catch (MissingMethodException ex) {
                ReportNeedNeedNativeDllUpgrade(ex, AssertForMissingSetAutoReconnect);
            }
        }
#endif

        private void SetCallback2()
        {
            const bool AssertForSetCallback2 = false;
            try {
                const int numSetting = 1;
                var ptrF = m_handleStackStatusChange;
                Debug.Assert(ptrF != null, "Delegate m_handleStackStatusChange is not set.");
#if !NETCF
                // Do NOT use on desktop for now, wierd things happen!
                // (Can do more testing in the future if the feature's required,
                // is it the Widcomm thread calling-into the CLR that causes bad
                // things?)
                ptrF = null;
#endif
                //Debug.WriteLine("Will NOT register for OnStackStatusChange.");
                //ptrF = null;
                if (ptrF != null) {
                    int numDid = NativeMethods.BtIf_SetCallback2(m_pBtIf, numSetting, ptrF);
                    if (numDid < numSetting)
                        ReportNeedNeedNativeDllUpgrade(
                            new ArgumentException("BtIf_SetCallback2 returned " + numSetting + ", but wanted: " + numSetting + "."),
                            true);
                    // If fail, then just will get no StackStatusChange events.
                    Debug.WriteLine("Did register for OnStackStatusChange.");
                }
            } catch (EntryPointNotFoundException ex) {
                ReportNeedNeedNativeDllUpgrade(ex, AssertForSetCallback2);
            } catch (MissingMethodException ex) {
                ReportNeedNeedNativeDllUpgrade(ex, AssertForSetCallback2);
            }
        }

        public void Destroy(bool disposing)
        {
            Utils.MiscUtils.Trace_WriteLine("WidcommBtIf.Destroy");
            Debug.Assert(m_pBtIf != IntPtr.Zero, "WidcommBtIf Already Destroyed");
            if (m_pBtIf != IntPtr.Zero) {
                NativeMethods.BtIf_Destroy(m_pBtIf);
                m_pBtIf = IntPtr.Zero;
            }
            s_alreadyExists = false;
        }

        //-------------
        const string ModuleName32feet = "32feetWidcomm";
#if WinXP
        const string ModuleNameWidcomm1 = "btwapi";
        const string ModuleNameWidcomm0 = "wbtapi";
#else
        const string ModuleNameWidcommWM = "BtSdkCe50";
        const string ModuleNameWidcommPPC = "BtSdkCe30";
#endif

        private static string GetWidcommInstalledModuleName()
        {
            string moduleNameWidcomm;
#if WinXP
            moduleNameWidcomm = ModuleNameWidcomm0;
#else
            OperatingSystem ver = Environment.OSVersion;
            if (ver.Version.Major < 5) // Think this is correct...
                moduleNameWidcomm = ModuleNameWidcommPPC;
            else
                moduleNameWidcomm = ModuleNameWidcommWM;
#endif
            return moduleNameWidcomm;
        }

        // This one!
        /// <summary>
        /// Check whether all the of dependencies are correct.
        /// </summary>
        /// -
        /// <param name="wrapException">The original exception we got on trying
        /// to load Widcomm.  Or <c>null</c> if Widcomm loaded successfully and
        /// we're just doing a check of the dependencies.
        /// </param>
        /// -
        /// <returns>Does not return if <paramref name="wrapException"/> is non-null,
        /// instead will throw it, or a more explanatory exception (with it as
        /// an inner exception).
        /// If <paramref name="wrapException"/> is null,
        /// </returns>
        private static LibraryStatus CheckDependencies(Exception wrapException)
        {
            string[] nameList = { ModuleName32feet,
#if WinXP
                                    ModuleNameWidcomm0, 
                                    ModuleNameWidcomm1
#else
                                    GetWidcommInstalledModuleName()
#endif
                                };
            foreach (string moduleName in nameList) {
                LibraryStatus status;
                try {
                    status = CheckLibraryDependency(moduleName);
                } catch (Exception ex) {
                    string msg1 = $"'{moduleName}' status exception: {ex}!";
                    Utils.MiscUtils.Trace_WriteLine(msg1);
                    Debug.Fail(msg1);
                    if (wrapException != null)
                        throw new PlatformNotSupportedException(msg1, ex);
                    status = LibraryStatus.Exception;
                    Debug.Assert(false, msg1);
                    //return status; Or continue to check next dependency
                }
                string msg = $"Dependency DLL '{moduleName}' status: {status}.";
                Utils.MiscUtils.Trace_WriteLine(msg);
                if (wrapException != null) {
                    if (!IsFound(status))
                        throw new PlatformNotSupportedException(msg, wrapException);
                } else {
                    if (status != LibraryStatus.ModuleLoaded) {
                        // Being called when start-up was ok, but we're apparently
                        // looking for the wrong dependencies.
                        Debug.Assert(false, "INFO: Unexpected exists but *not* ModuleLoaded: " + msg);
                        //
                        // We're here only in DEBUG mode so keep going to check
                        // the next if the file is at least present.
                        if (status != LibraryStatus.LoadLibraryAccessible) {
                            return status;
                        }
                    }
                }
            }
            if (wrapException != null)
                throw new PlatformNotSupportedException(wrapException.Message, wrapException);
            else
                return LibraryStatus.AggregateOk;
        }

        // This one!
        public static Exception IsWidcommStackPresentButNotInterfaceDll()
        {
            const Exception Success = null;
            string moduleNameWidcomm = GetWidcommInstalledModuleName();
            LibraryStatus wcDll = CheckLibraryDependency(moduleNameWidcomm);
            if (wcDll == LibraryStatus.NotFound) {
                // Widcomm stack not installed.
                return Success;
            }
            Debug.Assert(wcDll == LibraryStatus.ModuleLoaded || wcDll == LibraryStatus.LoadLibraryAccessible,
                "wcDll unexpected: " + wcDll);
            LibraryStatus ifaceDll = CheckLibraryDependency(ModuleName32feet);
            if (ifaceDll == LibraryStatus.NotFound) {
                // Ignore on Vista, no 'real' Widcomm support there.
                OperatingSystem os = Environment.OSVersion;
                Version osv = os.Version;
                const int VistaMajor = 6;
                if (osv.Major >= VistaMajor && os.Platform == PlatformID.Win32NT) {
                    bool forceCheckOnAllPlatforms = BluetoothFactoryConfig.WidcommICheckIgnorePlatform;
                    if (!forceCheckOnAllPlatforms)
                        return Success;
                }
                //
                // But not our interface DLL.
                return new PlatformNotSupportedException("Widcomm stack seems to be present, need to install 32feetWidcomm.dll alongside.");
            }
            // Both installed.
            return Success;
        }

        enum LibraryStatus
        {
            ModuleLoaded,
            LoadLibraryAccessible,
            AggregateOk,
            NotFound,
            Exception,
        }

        static bool IsFound(LibraryStatus status)
        {
            if (status == LibraryStatus.ModuleLoaded
                    || status == LibraryStatus.LoadLibraryAccessible)
                return true;
            return false;
        }

        static LibraryStatus CheckLibraryDependency(string moduleName)
        {
            IntPtr hG = NativeMethods.GetModuleHandle(moduleName);
            if (hG != IntPtr.Zero) {
                // (Must NOT free the handle).
                return LibraryStatus.ModuleLoaded;
            }
            //
            NativeMethods.LoadLibraryExFlags loadFlags = NativeMethods.LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE;
            IntPtr hL = NativeMethods.LoadLibraryEx(moduleName, IntPtr.Zero, loadFlags);
            if (hL != IntPtr.Zero) {
                bool success = NativeMethods.FreeLibrary(hL); // Must free the handle.
                Debug.Assert(success);
                return LibraryStatus.LoadLibraryAccessible;
            } else { // COVERAGE
            }
            //
            return LibraryStatus.NotFound;
        }

        //-------------
        void HandleStackStatusChange(int new_status)
        {
            var newStatus = (STACK_STATUS)new_status;
            Utils.MiscUtils.Trace_WriteLine(
                WidcommUtils.GetTime4Log() + ": "
                + "StackStatusChange: {0}", newStatus);
            //
            switch (newStatus) {
                case STACK_STATUS.Down:
                case STACK_STATUS.Error:
                case STACK_STATUS.Unloaded:
                    _factory._seenStackDownEvent = true;
                    ThreadPool.QueueUserWorkItem(UnloadedKill_Runner);
                    break;
                default:
                    Debug.Assert(newStatus == STACK_STATUS.Reloaded || newStatus == STACK_STATUS.Up,
                        "Unknown state: " + newStatus);
                    break;
            }
        }

        void UnloadedKill_Runner(object state)
        {
            Utils.MiscUtils.Trace_WriteLine(
                WidcommUtils.GetTime4Log() + ": "
                + "PortKill_Runner");
            var livePorts = _factory.GetPortList();
            foreach (var cur in livePorts) {
                cur.CloseInternalAndAbort_willLock();
            }
            Utils.MiscUtils.Trace_WriteLine(
                "PortKill_Runner done ({0}).", livePorts.Length);
        }



        //-------------
        public bool StartInquiry()
        {
            return NativeMethods.BtIf_StartInquiry(m_pBtIf);
        }

        public void StopInquiry()
        {
            NativeMethods.BtIf_StopInquiry(m_pBtIf);
        }

        void HandleDeviceResponded(IntPtr bdAddr, IntPtr devClass, IntPtr deviceName, bool connected)
        {
            byte[] bdAddrArr;
            byte[] devClassArr;
            byte[] deviceNameArr;
            WidcommUtils.GetBluetoothCallbackValues(bdAddr, devClass, deviceName,
                out bdAddrArr, out devClassArr, out deviceNameArr);
            m_parent.HandleDeviceResponded(bdAddrArr, devClassArr, deviceNameArr, connected);
        }

        void HandleInquiryComplete(bool success, UInt16 numResponses)
        {
            m_parent.HandleInquiryComplete(success, numResponses);
        }

        //-------------
        public bool StartDiscovery(BluetoothAddress address, Guid serviceGuid)
        {
            Utils.MiscUtils.Trace_WriteLine("WidcommBtIf.StartDiscovery");
            byte[] bdaddr = WidcommUtils.FromBluetoothAddress(address);
            int sizeofSdpDiscoveryRec; // just for interests sake
            bool ret = NativeMethods.BtIf_StartDiscovery(m_pBtIf, bdaddr, ref serviceGuid,
                out sizeofSdpDiscoveryRec);
            return ret;
        }

        public DISCOVERY_RESULT GetLastDiscoveryResult(out BluetoothAddress address, out UInt16 p_num_recs)
        {
            byte[] bdaddr = WidcommUtils.FromBluetoothAddress(BluetoothAddress.None);
            DISCOVERY_RESULT ret = NativeMethods.BtIf_GetLastDiscoveryResult(m_pBtIf, bdaddr, out p_num_recs);
            address = WidcommUtils.ToBluetoothAddress(bdaddr);
            return ret;
        }

        public ISdpDiscoveryRecordsBuffer ReadDiscoveryRecords(BluetoothAddress address, int maxRecords, ServiceDiscoveryParams args)
        {
            byte[] bdaddr = WidcommUtils.FromBluetoothAddress(address);
            IntPtr pList;
            Guid filter = args.serviceGuid;
            int count;
            if (args.searchScope == SdpSearchScope.Anywhere) {
                count = NativeMethods.BtIf_ReadDiscoveryRecords(m_pBtIf, bdaddr,
                    maxRecords, out pList);
            } else {
                Debug.Assert(args.searchScope == SdpSearchScope.ServiceClassOnly, "the other enum");
                try {
                    count = NativeMethods.BtIf_ReadDiscoveryRecordsServiceClassOnly(m_pBtIf, bdaddr,
                        maxRecords, out pList, ref filter);
                } catch (EntryPointNotFoundException ex) {
                    ReportNeedNeedNativeDllUpgrade(ex, true);
                    count = NativeMethods.BtIf_ReadDiscoveryRecords(m_pBtIf, bdaddr,
                        maxRecords, out pList);
                } catch (MissingMethodException ex) { // for NETCF
                    ReportNeedNeedNativeDllUpgrade(ex, true);
                    count = NativeMethods.BtIf_ReadDiscoveryRecords(m_pBtIf, bdaddr,
                        maxRecords, out pList);
                }
            }
            ISdpDiscoveryRecordsBuffer recBuf = new SdpDiscoveryRecordsBuffer(
                _factory, pList, count, args);
            return recBuf;
        }

        const string ErrorMsgNeedNativeDllUpgrade = "Need to upgrade your 32feetWidcomm.dll!";

        /// <summary>
        /// ReportNeedNeedNativeDllUpgrade, call from pair of catch:
        /// EntryPointNotFoundException and MissingMethodException.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="mayAssert">Whether we may put up an (Debug.)Assert dialog box.
        /// </param>
        internal static void ReportNeedNeedNativeDllUpgrade(Exception ex, bool mayAssert)
        {
            if (mayAssert) {
                Debug.Fail(ErrorMsgNeedNativeDllUpgrade);
            }
            Utils.MiscUtils.Trace_WriteLine(ErrorMsgNeedNativeDllUpgrade + "\n" + ex);
        }

        void HandleDiscoveryComplete()
        {
            m_parent.HandleDiscoveryComplete();
        }


        //-------------


        //-------------

        public REM_DEV_INFO_RETURN_CODE GetRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb)
        {
#if NETCF    // Not supported on my test device
            return REM_DEV_INFO_RETURN_CODE.ERROR;
#else
            Utils.MiscUtils.Trace_WriteLine("enter GetRemoteDeviceInfo, pBuf=0x{0:X}", p_rem_dev_info.ToInt64());
            REM_DEV_INFO_RETURN_CODE ret = NativeMethods.BtIf_GetRemoteDeviceInfo(m_pBtIf, p_rem_dev_info, cb);
            Utils.MiscUtils.Trace_WriteLine("exit GetRemoteDeviceInfo");
            remDevInfo = (REM_DEV_INFO)Marshal.PtrToStructure(p_rem_dev_info, typeof(REM_DEV_INFO));
            return ret;
#endif
        }

        public REM_DEV_INFO_RETURN_CODE GetNextRemoteDeviceInfo(ref REM_DEV_INFO remDevInfo, IntPtr p_rem_dev_info, int cb)
        {
            Utils.MiscUtils.Trace_WriteLine("enter GetNextRemoteDeviceInfo, pBuf=0x{0:X}", p_rem_dev_info.ToInt64());
            REM_DEV_INFO_RETURN_CODE ret = NativeMethods.BtIf_GetNextRemoteDeviceInfo(m_pBtIf, p_rem_dev_info, cb);
            Utils.MiscUtils.Trace_WriteLine("exit GetNextRemoteDeviceInfo");
            remDevInfo = (REM_DEV_INFO)Marshal.PtrToStructure(p_rem_dev_info, typeof(REM_DEV_INFO));
            return ret;
        }

        //-------------
        public bool GetLocalDeviceVersionInfo(ref DEV_VER_INFO devVerInfo)
        {
            int cb = Marshal.SizeOf(devVerInfo);
            bool success = NativeMethods.BtIf_GetLocalDeviceVersionInfo(m_pBtIf,
                ref devVerInfo, cb);
            return success;
        }

        public bool GetLocalDeviceName(byte[] bdName)
        {
            bool success = NativeMethods.BtIf_GetLocalDeviceName(m_pBtIf, bdName, bdName.Length);
            return success;
        }

        public bool GetLocalDeviceInfoBdAddr(byte[] bdAddr)
        {
            return NativeMethods.BtIf_GetLocalDeviceInfoBdAddr(m_pBtIf, bdAddr, bdAddr.Length);
        }

        public void IsStackUpAndRadioReady(out bool stackServerUp, out bool deviceReady)
        {
            try {
                NativeMethods.BtIf_IsStackUpAndRadioReady(m_pBtIf, out stackServerUp, out deviceReady);
            } catch (EntryPointNotFoundException ex) {
                string msg = "Need to upgrade your 32feetWidcomm.dll!";
                Debug.Fail(msg);
                Utils.MiscUtils.Trace_WriteLine(msg + "\n" + ex);
                stackServerUp = deviceReady = true;
            } catch (MissingMethodException ex) { // for NETCF
                string msg = "Need to upgrade your 32feetWidcomm.dll!";
                Debug.Fail(msg);
                Utils.MiscUtils.Trace_WriteLine(msg + "\n" + ex);
                stackServerUp = deviceReady = true;
            }
        }

        public void IsDeviceConnectableDiscoverable(out bool conno, out bool disco)
        {
            NativeMethods.BtIf_IsDeviceConnectableDiscoverable(m_pBtIf, out conno, out disco);
        }

        public void SetDeviceConnectableDiscoverable(bool connectable, bool forPairedOnly, bool discoverable)
        {
#if WinXP
            throw new NotSupportedException("No Widcomm API support.");
#else
            try {
                NativeMethods.BtIf_SetDeviceConnectableDiscoverable(m_pBtIf, connectable, forPairedOnly, discoverable);
                return;
            } catch (EntryPointNotFoundException ex) {
                string msg = "Need to upgrade your 32feetWidcomm.dll!";
                Debug.Fail(msg);
                Utils.MiscUtils.Trace_WriteLine(msg + "\n" + ex);
            } catch (MissingMethodException ex) { // for NETCF
                string msg = "Need to upgrade your 32feetWidcomm.dll!";
                Debug.Fail(msg);
                Utils.MiscUtils.Trace_WriteLine(msg + "\n" + ex);
            }
#endif
        }

        public int GetRssi(byte[] bd_addr)
        {
            Debug.Assert(bd_addr.Length == WidcommStructs.BD_ADDR_LEN, "bd_addr.Length: " + bd_addr.Length);
            tBT_CONN_STATS stats = new tBT_CONN_STATS();
            bool success = NativeMethods.BtIf_GetConnectionStats(m_pBtIf, bd_addr,
                out stats, Marshal.SizeOf(stats));
            if (!success) {
                // Occurs mostly.  Apparently a connection must exist for Rssi to 
                // be read, both from observation and from the Widcomm docs:
                //   "TRUE, if a connection attempt has been initiated; FALSE, if 
                //    a connection attempt has not been initiated"
                return int.MinValue;
            } else {
                return stats.rssi;
            }
        }

        public bool BondQuery(byte[] bd_addr)
        {
            bool arePaired = NativeMethods.BtIf_BondQuery(m_pBtIf, bd_addr);
            return arePaired;
        }

        public BOND_RETURN_CODE Bond(BluetoothAddress address, string passphrase)
        {
            byte[] bdaddr = WidcommUtils.FromBluetoothAddress(address);
            byte[] pinUtf8;
            byte[] pinUtf16; // As seen used in WM sample with CString::GetBuffer().
            if (passphrase != null) {
                // TODO should be ASCII??
                pinUtf8 = Encoding.UTF8.GetBytes(passphrase + "\0");
                pinUtf16 = Encoding.Unicode.GetBytes(passphrase + "\0");
            } else {
                // ?????????
                pinUtf8 = null;
                pinUtf16 = null;
            }
            //
            BOND_RETURN_CODE ret2;
#if !NETCF
            int retBtw = NativeMethods.BtIf_Bond(m_pBtIf, bdaddr, pinUtf8);
            ret2 = (BOND_RETURN_CODE)retBtw;
#else
            int retWce = NativeMethods.BtIf_Bond(m_pBtIf, bdaddr, pinUtf16);
            ret2 = Convert((BOND_RETURN_CODE__WCE)retWce);
#endif
            return ret2;
        }

        static BOND_RETURN_CODE Convert(BOND_RETURN_CODE__WCE wce)
        {
            BOND_RETURN_CODE ret;
            switch (wce) {
                case BOND_RETURN_CODE__WCE.ALREADY_BONDED:
                    ret = BOND_RETURN_CODE.ALREADY_BONDED;
                    break;
                case BOND_RETURN_CODE__WCE.BAD_PARAMETER:
                    ret = BOND_RETURN_CODE.BAD_PARAMETER;
                    break;
                case BOND_RETURN_CODE__WCE.FAIL:
                    ret = BOND_RETURN_CODE.FAIL;
                    break;
                case BOND_RETURN_CODE__WCE.SUCCESS:
                    ret = BOND_RETURN_CODE.SUCCESS;
                    break;
                default:
                    Debug.Fail($"Unknown BOND_RETURN_CODE__WCE code: 0x{(int)wce:X}");
                    ret = (BOND_RETURN_CODE)99;
                    break;
            }
            return ret;
        }

        public bool UnBond(BluetoothAddress address)
        {
            byte[] bdaddr = WidcommUtils.FromBluetoothAddress(address);
            bool ret = NativeMethods.BtIf_UnBond(m_pBtIf, bdaddr);
            return ret;
        }

        //-------------
        public WBtRc GetExtendedError()
        {
            uint ret = NativeMethods.BtIf_GetExtendedError(m_pBtIf);
            return checked((WBtRc)ret);
        }

        //----
        public SDK_RETURN_CODE IsRemoteDevicePresent(byte[] bd_addr)
        {
            int ret0 = NativeMethods.BtIf_IsRemoteDevicePresent(m_pBtIf, bd_addr);
            SDK_RETURN_CODE ret = (SDK_RETURN_CODE)ret0;
            return ret;
        }

        public bool IsRemoteDeviceConnected(byte[] bd_addr)
        {
            bool ret = NativeMethods.BtIf_IsRemoteDeviceConnected(m_pBtIf, bd_addr);
            return ret;
        }

    }


    enum DISCOVERY_RESULT
    {
        SUCCESS,
        /// <summary>
        /// Could not connect to remote device 
        /// </summary>
        CONNECT_ERR,
        /// <summary>
        /// Remote device rejected the connection 
        /// </summary>
        CONNECT_REJ,
        /// <summary>
        /// Security failed 
        /// </summary>
        SECURITY,
        /// <summary>
        /// Remote Service Record Error 
        /// </summary>
        BAD_RECORD,
        /// <summary>
        /// Other error
        /// </summary>
        OTHER_ERROR
    }

    enum REM_DEV_INFO_RETURN_CODE
    {
        /// <summary>
        /// success response
        /// </summary>
        SUCCESS,
        /// <summary>
        /// no more devices found
        /// </summary>
        EOF,
        /// <summary>
        /// can not find exsiting entry for bda provided as input
        /// </summary>
        ERROR,
        /// <summary>
        /// out of memory
        /// </summary>
        MEM_ERROR
    };

#if TEST_EARLY
    public
#endif
    enum BOND_RETURN_CODE
    {
        SUCCESS,
        BAD_PARAMETER,
        NO_BT_SERVER,
        ALREADY_BONDED,     // maintained for compatibility, BTW stack allows rebonding
        FAIL,
        REPEATED_ATTEMPTS   // pairing has failed repeatedly, and further attempts will
        // continue to return this code until after the device security
        // timeout                  - added BTW 5.0.1.700, SDK 5.0
    };

    enum BOND_RETURN_CODE__BTW
    {
        SUCCESS,
        BAD_PARAMETER,
        NO_BT_SERVER,
        ALREADY_BONDED,     // maintained for compatibility, BTW stack allows rebonding
        FAIL,
        REPEATED_ATTEMPTS   // pairing has failed repeatedly, and further attempts will
        // continue to return this code until after the device security
        // timeout                  - added BTW 5.0.1.700, SDK 5.0
    };

    enum BOND_RETURN_CODE__WCE
    {
        SUCCESS,
        ALREADY_BONDED,
        BAD_PARAMETER,
        FAIL
    };

    /// <summary>
    /// Used by OnStackChanges virtual method.
    /// </summary>
    /// <remarks>
    /// <para>1000-WCE-PG100-RCD.pdf (03/20/06) says:
    /// "... no longer used: DEVST_UP and DEVST_ERROR."
    /// and:
    /// "Values defined in BtIfClasses.h are:
    /// <code lang="none">
    /// � DEVST_DOWN � The stack is down and no longer available.
    /// � DEVST_UNLOADED � The stack is down, but should be available again after DEVST_RELOADED.
    /// � DEVST_RELOADED � The stack has been successfully reloaded."
    /// </code>
    /// </para>
    /// </remarks>
    enum STACK_STATUS
    {
        /// <summary>
        /// Device is present, but down [Seen (on BTW)]
        /// </summary>
        Down,
        /// <summary>
        /// Device is present and UP [Doc'd as obsolete, but I see it (on BTW)]
        /// </summary>
        Up,
        /// <summary>
        /// Device is in error (maybe being removed) [Doc'd as obsolete]
        /// </summary>
        Error,
        /// <summary>
        /// Stack is being unloaded
        /// </summary>
        Unloaded,
        /// <summary>
        /// Stack reloaded after being unloaded
        /// </summary>
        Reloaded
    }

}

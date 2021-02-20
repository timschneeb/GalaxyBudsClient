// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.WindowsBluetoothRadio
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.Factory;

#if WinXP
using InTheHand.Win32;
using System.Diagnostics.CodeAnalysis;

[assembly: InternalsVisibleTo("GalaxyBudsClient.Bluetooth.Windows")]

namespace InTheHand.Net.Bluetooth.Msft
{
    sealed class WindowsBluetoothRadio : IBluetoothRadio //: IDisposable
    {

        public string Remote { get { return null; } }

        #region IsPlatformSupported
        /// <summary>
        /// Gets a value that indicates whether the 32feet.NET library can be used with the current device.
        /// </summary>
        internal static bool IsPlatformSupported
        {
            get
            {
                return (AllRadios.Length > 0);
            }
        }
        #endregion

        private BLUETOOTH_RADIO_INFO radio;
        private readonly WindowsRadioHandle _handle;
        readonly LmpVersion _lmpV = LmpVersion.Unknown;
        readonly int _lmpSubv;
        readonly HciVersion _hciV = HciVersion.Unknown;
        readonly int _hciRev;
        readonly LmpFeatures _lmpFeatures;


        #region Constructor
        internal WindowsBluetoothRadio(WindowsRadioHandle handleW)
        {
            this._handle = handleW;

            //Debug.WriteLine("WindowsBluetoothRadio..ctor h=" + Handle.ToString("X"));
            radio = new BLUETOOTH_RADIO_INFO();
            radio.dwSize = 520;
            System.Diagnostics.Debug.Assert(System.Runtime.InteropServices.Marshal.SizeOf(radio) == radio.dwSize, "BLUETOOTH_RADIO_INFO SizeOf == dwSize");

            int hresult = NativeMethods.BluetoothGetRadioInfo(_handle, ref radio);
            if (hresult != 0) {
                throw new System.ComponentModel.Win32Exception(hresult, "Error retrieving Radio information.");
            }

            ReadVersionInfo(_handle, ref _lmpV, ref _lmpSubv, ref _hciV, ref _hciRev, ref _lmpFeatures);
        }
        #endregion

        #region ReadVersionInfo
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
            Justification = "DEBUG accesses instance members")]
        void ReadVersionInfo(WindowsRadioHandle handle,
            ref LmpVersion lmpV, ref int lmpSubv, ref HciVersion hciV, ref int hciRev, ref LmpFeatures lmpFeatures)
        {
            // Windows 7 IOCTL
            var buf = new byte[300];
            int bytesReturned;
            var success = NativeMethods.DeviceIoControl(handle,
                NativeMethods.MsftWin32BthIOCTL.IOCTL_BTH_GET_LOCAL_INFO,
                IntPtr.Zero, 0,
                buf, buf.Length, out bytesReturned, IntPtr.Zero);
            if (!success) {
                int gle = Marshal.GetLastWin32Error();
                //Debug.WriteLine(string.Format(CultureInfo.InvariantCulture, "INFO: IOCTL_BTH_GET_LOCAL_INFO failure: {0} = 0x{0:X}.", gle));
            } else {
                const int OffsetOf_flags = 272;
                const int OffsetOf_hciRevision = OffsetOf_flags + 4;
                const int OffsetOf_hciVersion = OffsetOf_hciRevision + 2;
                const int OffsetOf_radioInfo = OffsetOf_hciVersion + 1; //????? pad??
                const int OffsetOf_lmpFeatures = OffsetOf_radioInfo;
                const int OffsetOf_mfg = OffsetOf_radioInfo + 8;
                const int OffsetOf_lmpSubversion = OffsetOf_mfg + 2;
                const int OffsetOf_lmpVersion = OffsetOf_lmpSubversion + 2;
                const int OffsetOf_END = OffsetOf_lmpVersion + 1;
                const int ExpectedSize = 292;
                Debug.Assert(OffsetOf_END == ExpectedSize,
                    "OffsetOf_END: " + OffsetOf_END + ", ExpectedSize: " + ExpectedSize);
                hciRev = BitConverter.ToUInt16(buf, OffsetOf_hciRevision);
                hciV = (HciVersion)buf[OffsetOf_hciVersion];
                lmpSubv = BitConverter.ToUInt16(buf, OffsetOf_lmpSubversion);
                lmpV = (LmpVersion)buf[OffsetOf_lmpVersion];
                var dbg_mfg = BitConverter.ToUInt16(buf, OffsetOf_mfg);
                Debug.Assert(lmpSubv == radio.lmpSubversion,
                    "_lmpSubv: " + lmpSubv + ", radio.lmpSubversion: " + radio.lmpSubversion);
                Debug.Assert(dbg_mfg == unchecked((ushort)radio.manufacturer),
                    "dbg_mfg: " + dbg_mfg + ", radio.manufacturer: " + radio.manufacturer);
                //Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,"hciSubvers: {0}, hciVersion: {1}, lmpSubversion : {2}, lmpVersion: {3}", hciRev, hciV, lmpSubv, lmpV));
                var flags = (LOCAL_FLAGS)BitConverter.ToUInt32(buf, OffsetOf_flags);
                lmpFeatures = (LmpFeatures)BitConverter.ToInt64(buf, OffsetOf_lmpFeatures);
#if DEBUG
                var msgFl = "IOCTL_BTH_GET_LOCAL_INFO flags: '"
                     + flags + "' 0x" + flags.ToString("X");
                //Debug.WriteLine(msgFl);
                //Console.WriteLine(msgFl);
                //
                var msgFe = "IOCTL_BTH_GET_LOCAL_INFO lmpFeatures: '"
                     + lmpFeatures + "' 0x" + lmpFeatures.ToString("X");
                //Debug.WriteLine(msgFe);
                //Console.WriteLine(msgFe);
                LmpFeaturesUtils.FindUndefinedValues(lmpFeatures);
                //LmpFeaturesUtils.FindUnsetValues(lmpFeatures);
#endif
            }
        }

        [Flags]
        enum LOCAL_FLAGS
        {
            NonDiscoverableNonConnectable = 0,
            Discoverable = 1,
            Connectable = 2,
        }

        /*struct _BTH_LOCAL_RADIO_INFO
        {
            BTH_DEVICE_INFO localInfo;
            UInt32 flags;
            UInt16 hciRevision;
            Byte hciVersion;
            BTH_RADIO_INFO radioInfo;
        }

        struct BTH_RADIO_INFO
        {
            UInt64 lmpSupportedFeatures;
            UInt16 mfg;
            UInt16 lmpSubversion;
            Byte lmpVersion;
        }*/
        #endregion

        #region Primary Radio
        internal static IBluetoothRadio GetPrimaryRadio()
        {
            //get a single radio
            IntPtr handle = IntPtr.Zero;
            IntPtr findhandle = IntPtr.Zero;

            BLUETOOTH_FIND_RADIO_PARAMS bfrp;
            bfrp.dwSize = 4;
            System.Diagnostics.Debug.Assert(System.Runtime.InteropServices.Marshal.SizeOf(bfrp) == bfrp.dwSize, "BLUETOOTH_FIND_RADIO_PARAMS SizeOf == dwSize");

            findhandle = NativeMethods.BluetoothFindFirstRadio(ref bfrp, out handle);

            if (findhandle != IntPtr.Zero) {
                NativeMethods.BluetoothFindRadioClose(findhandle);
            }
            if (handle != IntPtr.Zero) {
                var hw = new WindowsRadioHandle(handle);
                return new WindowsBluetoothRadio(hw);
            }
            throw new PlatformNotSupportedException("No Radio.");
        }
        #endregion

        #region All Radios

        internal static IBluetoothRadio[] AllRadios
        {
            get
            {
                IntPtr handle = IntPtr.Zero;
                IntPtr findhandle = IntPtr.Zero;
                WindowsRadioHandle hw;

                BLUETOOTH_FIND_RADIO_PARAMS bfrp;
                bfrp.dwSize = 4;
                System.Diagnostics.Debug.Assert(System.Runtime.InteropServices.Marshal.SizeOf(bfrp) == bfrp.dwSize, "BLUETOOTH_FIND_RADIO_PARAMS SizeOf == dwSize");

                List<WindowsRadioHandle> radiocollection = new List<WindowsRadioHandle>();

                findhandle = NativeMethods.BluetoothFindFirstRadio(ref bfrp, out handle);

                if (findhandle != IntPtr.Zero) {
                    //add first handle
                    hw = new WindowsRadioHandle(handle);
                    radiocollection.Add(hw);

                    while (NativeMethods.BluetoothFindNextRadio(findhandle, out handle)) {
                        //add subsequent handle
                        hw = new WindowsRadioHandle(handle);
                        radiocollection.Add(hw);
                    }

                    //close findhandle
                    NativeMethods.BluetoothFindRadioClose(findhandle);
                }

                //populate results array
                IBluetoothRadio[] results = new IBluetoothRadio[radiocollection.Count];
                for (int radioindex = 0; radioindex < results.Length; radioindex++) {
                    results[radioindex] = new WindowsBluetoothRadio(radiocollection[radioindex]);
                }

                return results;
            }
        }
        #endregion



        #region Handle
        public IntPtr Handle
        {
            get
            {
                if (this._handle.IsClosed) {
                    throw new InvalidOperationException("Radio Handle is closed.");
                }
                return this._handle.DangerousGetHandle();
            }
        }
        #endregion

        #region Hardware Status
        public HardwareStatus HardwareStatus
        {
            get
            {
                var tmp = new BLUETOOTH_RADIO_INFO();
                tmp.dwSize = 520;
                System.Diagnostics.Debug.Assert(System.Runtime.InteropServices.Marshal.SizeOf(tmp) == radio.dwSize, "BLUETOOTH_RADIO_INFO SizeOf == dwSize");

                int hresult = NativeMethods.BluetoothGetRadioInfo(_handle, ref tmp);
                if (hresult != 0) {
                    //throw new System.ComponentModel.Win32Exception(hresult, "Error retrieving Radio information.");
                    return HardwareStatus.NotPresent;
                }
                return HardwareStatus.Running;
            }
        }
        #endregion

        #region Mode
        RadioModes IBluetoothRadio.Modes
        {
            get
            {
                RadioModes modes = 0;
                if (HardwareStatus == HardwareStatus.Running) {
                    modes |= RadioModes.PowerOn;
                } else {
                    modes |= RadioModes.PowerOff;
                }
                //
                if (NativeMethods.BluetoothIsDiscoverable(_handle)) {
                    modes |= RadioModes.Discoverable;
                }
                if (NativeMethods.BluetoothIsConnectable(_handle)) {
                    modes |= RadioModes.Connectable;
                }
                return modes;
            }
        }

        public void SetMode(bool? connectable, bool? discoverable)
        {
            // http://msdn.microsoft.com/en-us/library/windows/desktop/aa362778(v=vs.85).aspx
            // "The radio must be made non-discoverable prior to making a radio non-connectable."
            // Thus Discoverable needs to be set BEFORE if false and AFTER if true.
            if (discoverable.HasValue && !discoverable.Value) {
                // "Returns TRUE if the discovery state was successfully changed."
                var changedState = NativeMethods.BluetoothEnableDiscovery(_handle, discoverable.Value);
            }
            if (connectable.HasValue) {
                // "Returns TRUE if the incoming connection state was successfully changed."
                var changedState = NativeMethods.BluetoothEnableIncomingConnections(_handle, connectable.Value);
            }
            if (discoverable.HasValue && discoverable.Value) {
                // "Returns TRUE if the incoming connection state was successfully changed."
                var changedState = NativeMethods.BluetoothEnableDiscovery(_handle, discoverable.Value);
            }
        }

        public RadioMode Mode
        {
            get
            {

                if (NativeMethods.BluetoothIsDiscoverable(_handle)) {
                    return RadioMode.Discoverable;
                }
                if (NativeMethods.BluetoothIsConnectable(_handle)) {
                    return RadioMode.Connectable;
                }
                return RadioMode.PowerOff;
            }
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Nullable`1<System.Boolean>")]
            set
            {

                bool? okDisco = null, okConno = null;
                switch (value) {
                    case RadioMode.Discoverable:
                        if (Mode == RadioMode.PowerOff) {
                            okConno = NativeMethods.BluetoothEnableIncomingConnections(_handle, true);
                        }
                        okDisco = NativeMethods.BluetoothEnableDiscovery(_handle, true);
                        break;
                    case RadioMode.Connectable:
                        if (Mode == RadioMode.Discoverable) {
                            okDisco = NativeMethods.BluetoothEnableDiscovery(_handle, false);
                        } else {
                            okConno = NativeMethods.BluetoothEnableIncomingConnections(_handle, true);
                        }
                        break;
                    case RadioMode.PowerOff:
                        if (Mode == RadioMode.Discoverable) {
                            okDisco = NativeMethods.BluetoothEnableDiscovery(_handle, false);
                        }
                        okConno = NativeMethods.BluetoothEnableIncomingConnections(_handle, false);
                        break;
                }
                Debug.WriteLine("SetMode(" + value + "): conno: " + okConno + ", disco: " + okDisco);
            }
        }
        #endregion

        #region Local Address
        public BluetoothAddress LocalAddress
        {
            get
            {
                return new BluetoothAddress(radio.address);
            }
        }

        #endregion

        #region Name
        public string Name
        {
            get
            {
                return radio.szName;
            }
            set
            {
                // Based on code submission by Todd M Stafney

                // figure out what subkey in the registry the name lives in
                string regKey = GetRadioRegKey(this.LocalAddress);
                if (null == regKey) return; // should only get this if there are no active BT devices?
                string fullRegKey = String.Format("SYSTEM\\CurrentControlSet\\Enum\\{0}\\Device Parameters", regKey);

                // now try and slam dunk our new name in there - this requires the app to be running as Administrator or equivalent
                System.Security.Permissions.RegistryPermission rp = new System.Security.Permissions.RegistryPermission(System.Security.Permissions.PermissionState.Unrestricted);
                rp.Demand();
                Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(fullRegKey, true);
                if (null == rk) {
                    Debug.WriteLine("!! set_Name failed to open key");
                    return; // ??? this shouldn't happen!
                }
                // let's provide a handy way to "reset" the name...
                if (null == value) value = System.Environment.MachineName;
                // the name is stored as a binary... of ascii chars...
                System.Collections.Generic.List<byte> bits = new System.Collections.Generic.List<byte>(System.Text.ASCIIEncoding.ASCII.GetBytes(value));
                // but we need to null terminate him
                bits.Add(0);
                // do it...
                rk.SetValue("Local Name", bits.ToArray(), Microsoft.Win32.RegistryValueKind.Binary);
                rk.Close();

                // This check decides what IO control code to use based on if we're in XP or Vista.
                // The XP one I had thanks to the Mark article.  But that wasn't valid on Vista.  So? 
                // I had to unpack these CTL_CODE generated values from the old driver kit (thank you
                // dusty old CD's in my office) and then figure out what it was and then go searching 
                // through the new driver kit to match it up. Ok so for those of you following along at home
                // 0x411008 in Vista CTL_CODE() parlance is:
                //   DeviceType ==  0x41 == FILE_DEVICE_BLUETOOTH
                //   Access     ==  0x00 == FILE_ANY_ACCESS
                //   Function   == 0x402 == some sort of device control function
                //   Method     ==  0x00 == METHOD_BUFFERED
                uint ctlCode = (uint)(6 > System.Environment.OSVersion.Version.Major ? 0x220fd4 : 0x411008);
                long foo = 4;  // tells the control function to reset or reload or similar...
                int bytes = 0; // merely a placeholder
                if (!NativeMethods.DeviceIoControl(this.Handle, ctlCode, ref foo, 4, IntPtr.Zero, 0, out bytes, IntPtr.Zero)) {
                    throw new Win32Exception();
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "InTheHand.Net.BluetoothAddress.TryParse(System.String,InTheHand.Net.BluetoothAddress@)")]
        private static string GetRadioRegKey(InTheHand.Net.BluetoothAddress radioAddress)
        {
            Guid btguid = NativeMethods.GUID_DEVCLASS_BLUETOOTH;
            IntPtr hDevInfo = NativeMethods.SetupDiGetClassDevs(ref btguid, null, IntPtr.Zero, NativeMethods.DIGCF.PRESENT | NativeMethods.DIGCF.PROFILE);

            if (IntPtr.Zero.Equals(hDevInfo)) throw new Win32Exception();

            try {
                NativeMethods.SP_DEVINFO_DATA data = new NativeMethods.SP_DEVINFO_DATA();
                // On 32bit platforms, all SetupApi structures are 1-Byte packed. 
                // On 64bit platforms the SetupApi structures are 8-byte packed. 
                // i.e. for 32 bit SP_DEVINFO_DATA.cbSize=28, for 64Bit SP_DEVINFO_DATA.cbSize=(28+4)=32.
                data.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(data);
                uint idx = 0;

                while (NativeMethods.SetupDiEnumDeviceInfo(hDevInfo, idx++, ref data)) {
                    // grab the instance id
                    string instanceId = DevInstanceId(hDevInfo, data);
                    //Debug.WriteLine("WindowsBluetoothRadio.GetRadioRegKey item: " + instanceId);

                    //the last part of the instanceId will be the unique bluetooth address of the radio
                    int lastSlashIndex = instanceId.LastIndexOf("\\");
                    BluetoothAddress address;
                    InTheHand.Net.BluetoothAddress.TryParse(instanceId.Substring(lastSlashIndex + 1, instanceId.Length - lastSlashIndex - 1), out address);
                    //InTheHand.Net.BluetoothAddress address = InTheHand.Net.BluetoothAddress.Parse(instanceId.Substring(lastSlashIndex + 1, instanceId.Length - lastSlashIndex - 1));

                    if (address != null && radioAddress == address) {
                        return instanceId;
                    }
                }
            } finally {
                // make sure we clean up after ourselves!
                NativeMethods.SetupDiDestroyDeviceInfoList(hDevInfo);
            }
            // hmmm, we didn't find any BT radios?
            Debug.WriteLine("!! WindowsBluetoothRadio.GetRadioRegKey, radio NOT found!");
            return null;
        }

        private static string DevInstanceId(IntPtr hDevInfo, NativeMethods.SP_DEVINFO_DATA data)
        {
            Win32Error rc = Win32Error.ERROR_SUCCESS;
            int requiredSize = 0;
            System.Text.StringBuilder sb = null;

            // we call twice, first to get the required buffer size...
            for (int ii = 0; ii < 2; ii++) {
                rc = Win32Error.ERROR_SUCCESS;
                if (!NativeMethods.SetupDiGetDeviceInstanceId(hDevInfo, ref data, sb, requiredSize, out requiredSize)) {
                    rc = (Win32Error)System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    if (rc != Win32Error.ERROR_INSUFFICIENT_BUFFER) break;
                    sb = new System.Text.StringBuilder(requiredSize + 2);
                }
            }
            if (Win32Error.ERROR_SUCCESS != rc) throw new Win32Exception((int)rc);

            return sb.ToString();
        }

        #endregion

        #region Class Of Device
        public ClassOfDevice ClassOfDevice
        {
            get
            {
                return new ClassOfDevice(radio.ulClassofDevice);
            }
        }

        #endregion

        #region Manufacturer
        public Manufacturer Manufacturer
        {
            get
            {
                return radio.manufacturer;
            }
        }
        #endregion

        #region Lmp Subversion
        LmpVersion IBluetoothRadio.LmpVersion { get { return _lmpV; } }

        public int LmpSubversion { get { return radio.lmpSubversion; } }

        HciVersion IBluetoothRadio.HciVersion { get { return _hciV; } }

        int IBluetoothRadio.HciRevision { get { return _hciRev; } }

        public LmpFeatures LmpFeatures { get { return _lmpFeatures; } }
        #endregion

        #region Stack
        public Manufacturer SoftwareManufacturer
        {
            get
            {
                return Manufacturer.Microsoft;
            }
        }
        #endregion

        /*private static NativeMethods.BluetoothMessageFilter bmf = new NativeMethods.BluetoothMessageFilter();

        public event EventHandler RadioInRange
        {
            add
            {
                NativeMethods.DEV_BROADCAST_HANDLE dbh = new NativeMethods.DEV_BROADCAST_HANDLE();
                dbh.dbch_size = System.Runtime.InteropServices.Marshal.SizeOf(dbh);
                dbh.dbch_devicetype = NativeMethods.DBT_DEVTYP_HANDLE;
                dbh.dbch_handle = this.handle;
                dbh.dbch_eventguid = NativeMethods.GUID_BLUETOOTH_RADIO_IN_RANGE;
                System.Windows.Forms.Application.AddMessageFilter(bmf);
                int result = NativeMethods.RegisterDeviceNotification(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, ref dbh, 0);
            }
            remove
            {
            }
        }*/

    }
}
#endif

// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2008-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Sockets;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    internal sealed class WidcommBluetoothDeviceInfo : IBluetoothDeviceInfo
    {
        readonly WidcommBluetoothFactoryBase m_factory;
        REM_DEV_INFO m_remDevInfo;
        bool m_remembered;
        string m_cachedName;
        DateTime m_lastSeen;

        private WidcommBluetoothDeviceInfo(REM_DEV_INFO remDevInfo, WidcommBluetoothFactoryBase factory)
        {
            m_factory = factory;
            m_remDevInfo = remDevInfo;
        }

        //--------
        internal static WidcommBluetoothDeviceInfo CreateFromGivenAddressNoLookup(BluetoothAddress address, WidcommBluetoothFactoryBase factory)
        {
            REM_DEV_INFO rdi = new REM_DEV_INFO();
            rdi.bda = WidcommUtils.FromBluetoothAddress(address);
            WidcommBluetoothDeviceInfo bdi = new WidcommBluetoothDeviceInfo(rdi, factory);
            return bdi;
        }

        internal static WidcommBluetoothDeviceInfo CreateFromGivenAddress(BluetoothAddress address, WidcommBluetoothFactoryBase factory)
        {
            REM_DEV_INFO rdi = new REM_DEV_INFO();
            rdi.bda = WidcommUtils.FromBluetoothAddress(address);
            WidcommBluetoothDeviceInfo bdi = factory.GetWidcommBtInterface()
                .ReadDeviceFromRegistryAndCheckAndSetIfPaired(address, factory);
            if (bdi == null) {
                bdi = CreateFromGivenAddressNoLookup(address, factory);
            }
            return bdi;
        }

        /// <summary>
        /// Used when loading a stack stored/remembered/maybe-paired device.
        /// </summary>
        internal static WidcommBluetoothDeviceInfo CreateFromStoredRemoteDeviceInfo(REM_DEV_INFO rdi, WidcommBluetoothFactoryBase factory)
        {
            WidcommBluetoothDeviceInfo bdi = new WidcommBluetoothDeviceInfo(rdi, factory);
            bdi.m_remembered = true;
            return bdi;
        }

        /// <summary>
        /// Used when a device is discovered during Inquiry.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>When the result of Inquiry and get-stack-stored-devices are merged,
        /// the remembered/authenticated flags may get set then (with <see cref="M:SetAuthenticated"/>).
        /// </para>
        /// </remarks>
        internal static WidcommBluetoothDeviceInfo CreateFromHandleDeviceResponded(byte[] bdAddr,
            byte[] deviceName, byte[] devClass, bool connected, WidcommBluetoothFactoryBase factory)
        {
            REM_DEV_INFO rdi = new REM_DEV_INFO();
            rdi.bda = bdAddr;
            rdi.bd_name = deviceName;
            rdi.b_connected = connected;
            rdi.dev_class = devClass;
            WidcommBluetoothDeviceInfo bdi = new WidcommBluetoothDeviceInfo(rdi, factory);
            //bdi.m_inquiryTime = DateTime.UtcNow;
            return bdi;
        }

        //--------------------------------------------------------------
        /// <summary>
        /// Called after reading the device from the Registry, to find if it is paired.
        /// </summary>
        internal static void CheckAndSetIfPaired(WidcommBluetoothDeviceInfo bdi, WidcommBluetoothFactoryBase factory)
        {
            Debug.Assert(bdi.m_remembered == true, "should be already marked as rembered");
            Debug.Assert(!bdi.m_remDevInfo.b_paired, "why rechecking?");
            if (!bdi.m_remDevInfo.b_paired) {
                bool paired = factory.GetWidcommBtInterface().BondQuery(bdi.m_remDevInfo.bda);
                bdi.m_remDevInfo.b_paired = paired;
            }
        }

        /// <summary>
        /// For use when the results of Inquiry and get-stack-stored-devices are merged.
        /// </summary>
        public void Merge(IBluetoothDeviceInfo other)
        {
            m_remembered = other.Remembered;
            m_remDevInfo.b_paired = other.Authenticated;
        }

        public void SetDiscoveryTime(DateTime dt)
        {
            if (m_lastSeen != DateTime.MinValue)
                throw new InvalidOperationException("LastSeen is already set.");
            m_lastSeen = dt;
        }

        //--------------------------------------------------------------

        public BluetoothAddress DeviceAddress
        {
            get { return WidcommUtils.ToBluetoothAddress(m_remDevInfo.bda); }
        }

        public string DeviceName
        {
            get
            {
                if (m_cachedName == null) {
                    if (m_remDevInfo.bd_name != null)
                        m_cachedName = WidcommUtils.BdNameToString(m_remDevInfo.bd_name);
                    if (m_cachedName == null)
                        m_cachedName = DeviceAddress.ToString("C", System.Globalization.CultureInfo.InvariantCulture);
                }
                return m_cachedName;
            }
            set
            {
                m_cachedName = value;
                m_remDevInfo.bd_name = Encoding.UTF8.GetBytes(m_cachedName + "\0");
            }
        }

        public bool Remembered
        {
            get { return m_remembered; }
        }

        public bool Authenticated
        {
            get { return m_remDevInfo.b_paired; }
        }

        public ClassOfDevice ClassOfDevice
        {
            get
            {
                // Is currently only set in the 
                if (m_remDevInfo.dev_class == null) {
                    Utils.MiscUtils.Trace_WriteLine("NOT m_remDevInfo.dev_class");
                    return new ClassOfDevice(0);
                }
                return WidcommUtils.ToClassOfDevice(m_remDevInfo.dev_class);
            }
        }

        public bool Connected
        {
            get { return m_remDevInfo.b_connected; }
        }

        public DateTime LastSeen
        {
            get { return m_lastSeen; }
        }

        public DateTime LastUsed
        {
            get { return DateTime.MinValue; }
        }

        public int Rssi
        {
            get
            {
                WidcommBtInterface iface = m_factory.GetWidcommBtInterface();
                int rssi = iface.GetRssi(m_remDevInfo.bda);
                return rssi;
            }
        }

        public void Refresh()
        {
            //m_valid = false;
            // Seems no way to read just the device name.  Read from Registry at least?
            m_cachedName = null;
            // TODO Can use CBtIf::IsConnected to update this.
            // m_remDevInfo.b_connected = ...
        }

        public ServiceRecord[] GetServiceRecords(Guid service)
        {
            IAsyncResult ar = BeginGetServiceRecords(service, null, null);
            return EndGetServiceRecords(ar);
        }

        public IAsyncResult BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            return BeginGetServiceRecordsUnparsedWidcomm(service, callback, state);
        }

        public ServiceRecord[] EndGetServiceRecords(IAsyncResult asyncResult)
        {
            using (ISdpDiscoveryRecordsBuffer wrec = EndGetServiceRecordsUnparsedWidcomm(asyncResult)) {
                ServiceRecord[] records = wrec.GetServiceRecords();
                return records;
            }
        }

        private IAsyncResult BeginGetServiceRecordsUnparsedWidcomm(Guid service, AsyncCallback callback, object state)
        {
            WidcommBtInterface iface = m_factory.GetWidcommBtInterface();
            IAsyncResult ar = iface.BeginServiceDiscovery(DeviceAddress, service, SdpSearchScope.Anywhere, callback, state);
            return ar;
        }

        private ISdpDiscoveryRecordsBuffer EndGetServiceRecordsUnparsedWidcomm(IAsyncResult ar)
        {
            WidcommBtInterface iface = m_factory.GetWidcommBtInterface();
            ISdpDiscoveryRecordsBuffer wrec = iface.EndServiceDiscovery(ar);
            return wrec;
        }

        public byte[][] GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotSupportedException("Can't get the raw record from the Widcomm stack.");
        }


        public Guid[] InstalledServices
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public void SetServiceState(Guid service, bool state, bool throwOnError)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public void SetServiceState(Guid service, bool state)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            throw new NotImplementedException("GetVersions not currently supported on this stack.");
        }

        public void ShowDialog()
        {
            // Copied from WinCE BDI version.
#if ! NO_WINFORMS
            System.Windows.Forms.MessageBox.Show(
                "Name: " + this.DeviceName
                + "\r\nAddress: " + this.DeviceAddress.ToString("C", System.Globalization.CultureInfo.InvariantCulture),
                this.DeviceName + " Properties",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.None,
                System.Windows.Forms.MessageBoxDefaultButton.Button1
#if !NETCF
                , (System.Windows.Forms.MessageBoxOptions)0 //For FxCop
#endif
                );
#endif
        }

        public void Update()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("Who's calling Equals on IBdi?!");
            //TODO throw new NotSupportedException("Should be called on BDI wrapper class.");
            return base.Equals(obj);
        }

        // SHOULD override this if overriding Equals
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}

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
using System.Runtime.InteropServices;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class WidcommBluetoothRadio : IBluetoothRadio
    {
        readonly WidcommBluetoothFactoryBase _factory;
        readonly DEV_VER_INFO m_dvi;
        readonly string m_name;

        internal WidcommBluetoothRadio(WidcommBluetoothFactoryBase factory)
        {
            System.Threading.ThreadStart handleError = delegate { };
#if !NETCF
            // On my old iPAQ the radio info functions fail sometimes even though 
            // the stack is working fine so we ignored the errors in the first 
            // release.  On Win32 this is a problem when the stack is installed 
            // but no radio is attached, so fail in that case.
            handleError = delegate {
                throw new PlatformNotSupportedException(
                    "Widcomm Bluetooth stack not supported (Radio).");
            };
#endif
            //
            Debug.Assert(factory != null);
            _factory = factory;
            var tmp = BtIf;
            bool ret;
            ret = BtIf.GetLocalDeviceVersionInfo(ref m_dvi);
            Debug.Assert(ret, "GetLocalDeviceVersionInfo failed");
            if (!ret) {
                handleError();
                // Call handleError first so that one Win32 we don't get the Assert if there's no stack present.
                Debug.Assert(ret, "GetLocalDeviceVersionInfo failed");
                m_dvi = new DEV_VER_INFO(HciVersion.Unknown); // Reset to overwrite any garbage returned by GetLocalDeviceVersionInfo.
            }
            byte[] bdName = new byte[WidcommStructs.BD_NAME_LEN];
            ret = BtIf.GetLocalDeviceName(bdName);
            Debug.Assert(ret, "GetLocalDeviceName failed");
            if (ret)
                m_name = WidcommUtils.BdNameToString(bdName);
            else {
                bdName = null;
                handleError();
            }
            //
            // Did GetLocalDeviceVersionInfo get the address?  It doesn't work on 
            // my iPAQ, but then again this way doesn't work either!
            if (LocalAddress == null || LocalAddress.ToInt64() == 0) {
                Utils.MiscUtils.Trace_WriteLine("GetLocalDeviceVersionInfo's bd_addr is empty, trying GetLocalDeviceInfoBdAddr...");
                if (m_dvi.bd_addr == null)
                    m_dvi.bd_addr = new byte[WidcommStructs.BD_ADDR_LEN];
                ret = BtIf.GetLocalDeviceInfoBdAddr(m_dvi.bd_addr);
                Debug.Assert(ret, "GetLocalDeviceInfoBdAddr failed");
                if (!ret) {
                    m_dvi.bd_addr = new byte[WidcommStructs.BD_ADDR_LEN];
                    handleError();
                }
            }
        }

        private WidcommBtInterface BtIf
        {
            [DebuggerStepThrough]
            get { return _factory.GetWidcommBtInterface(); }
        }

        //--------
        public string Remote { get { return null; } }

        public ClassOfDevice ClassOfDevice
        {
            get
            {
                // Could return DesktopComputer/PdaComputer on the Win32/WM, but would 
                // it just make the caller think this was supported, when we know no 
                // way of getting the ServiceClass bits!
                // Note also "MinorClass" value in Registry at HKEY_LOCAL_MACHINE\SOFTWARE\Widcomm\BTConfig\General\,
                // but no ServiceClass entry as far as I can see.
                return new ClassOfDevice(0);
            }
        }

        public IntPtr Handle
        {
            get { throw new NotSupportedException("WidcommBluetoothRadio.Handle"); }
        }

        public HardwareStatus HardwareStatus
        {
            get
            {
#if true //- HAVE_NEW_VERSION_OF_THE_NATIVE_DLL
                bool stackUp, radioReady;
                BtIf.IsStackUpAndRadioReady(out stackUp, out radioReady);
                if (!stackUp)
                    return HardwareStatus.Shutdown;
                if (!radioReady)
                    return HardwareStatus.Shutdown;
#endif
                return HardwareStatus.Running;
            }
        }

        LmpVersion IBluetoothRadio.LmpVersion
        {
            get { return (LmpVersion)m_dvi.lmp_version; }
        }

        public int LmpSubversion
        {
            get { return m_dvi.lmp_sub_version; }
        }

        LmpFeatures IBluetoothRadio.LmpFeatures { get { return LmpFeatures.None; } }

        HciVersion IBluetoothRadio.HciVersion
        {
            get { return (HciVersion)m_dvi.hci_version; }
        }

        public int HciRevision
        {
            get { return m_dvi.hci_revision; }
        }

        public BluetoothAddress LocalAddress
        {
            get
            {
                if (m_dvi.bd_addr == null)
                    return null;
                else
                    return WidcommUtils.ToBluetoothAddress(m_dvi.bd_addr);
            }
        }

        public Manufacturer Manufacturer
        {
            get
            {
                // [enum Manufacturer : short] -vs- [ushort manufacturer]
                short i16 = unchecked((short)m_dvi.manufacturer);
                return (Manufacturer)i16;
            }
        }

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
                bool conno, disco;
                // This returns true/true on Win32.
                BtIf.IsDeviceConnectableDiscoverable(out conno, out disco);
                if (disco) { modes |= RadioModes.Discoverable; }
                if (conno) { modes |= RadioModes.Connectable; }
                // check PowerOff/On
                return modes;
            }
        }

        public void SetMode(bool? connectable, bool? discoverable)
        {
            bool conno, disco;
            if (connectable.HasValue && discoverable.HasValue) {
                // Will set both bits so do NOT need to know their current value.
                conno = disco = false;
            } else {
                // This returns true/true on Win32.
                BtIf.IsDeviceConnectableDiscoverable(out conno, out disco);
            }
            if (connectable.HasValue) {
                conno = connectable.Value;
            }
            if (discoverable.HasValue) {
                disco = discoverable.Value;
            }
            BtIf.SetDeviceConnectableDiscoverable(conno, AllowPairedOnly, disco);
        }

        const bool AllowPairedOnly = false;

        public RadioMode Mode
        {
            get
            {
                if (HardwareStatus != HardwareStatus.Running)
                    return RadioMode.PowerOff;
                //--
                bool conno, disco;
                // This returns true/true on Win32.
                BtIf.IsDeviceConnectableDiscoverable(out conno, out disco);
                if (disco) {
                    Debug.Assert(conno, "disco but not conno!");
                    return RadioMode.Discoverable;
                } else if (conno) {
                    return RadioMode.Connectable;
                } else {
                    return RadioMode.PowerOff;
                }
            }
            set
            {
#if false && WinXP
                throw new NotSupportedException("No Widcomm API support.");
#else
                bool conno;
                bool disco;
                switch (value) {
                    case RadioMode.PowerOff:
                        conno = false;
                        disco = false; // Native won't change it.  Off so not disco.
                        break;
                    case RadioMode.Connectable:
                        conno = true;
                        disco = false;
                        break;
                    case RadioMode.Discoverable:
                        conno = true;
                        disco = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
                BtIf.SetDeviceConnectableDiscoverable(conno, AllowPairedOnly, disco);
#endif
            }
        }

        public string Name
        {
            get { return m_name; }
            set { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        public Manufacturer SoftwareManufacturer
        {
            get { return Manufacturer.Broadcom; /* .Widcomm?? */ }
        }

    }//class
}

// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using Android.Bluetooth;
using System.Diagnostics;

namespace InTheHand.Net.Bluetooth.Droid
{
    class AndroidBthRadio : IBluetoothRadio
    {
        readonly RadioVersions _fakeVers = new RadioVersions(
            LmpVersion.Unknown, 0, LmpFeatures.None, Manufacturer.Unknown);
        //
        readonly BluetoothAdapter _adapter;
        readonly BluetoothAddress _addr;

        #region ctor
        internal AndroidBthRadio(Android.Bluetooth.BluetoothAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException("adapter");
            _adapter = adapter;
            string addrS = _adapter.Address;
            _addr = AndroidBthUtils.ToBluetoothAddress(addrS);
        }
        #endregion

        #region IBluetoothRadio Members

        BluetoothAddress IBluetoothRadio.LocalAddress
        {
            get { return _addr; }
        }

        string IBluetoothRadio.Name
        {
            get { return _adapter.Name; }
            set
            {
                bool ok = _adapter.SetName(value);
                if (!ok)
                    throw new InvalidOperationException("Failed to set radio name.");
            }
        }

        ClassOfDevice IBluetoothRadio.ClassOfDevice
        {
            get { return new ClassOfDevice(0); }
        }

        RadioModes IBluetoothRadio.Modes
        {
            get
            {
                RadioModes m = 0;
                var s = _adapter.State;
                var sm = _adapter.ScanMode;
                // Power etc
                if (s == State.On) {
                    m |= RadioModes.PowerOn;
                } else {
                    m |= RadioModes.PowerOff;
                }
                // Conno/Disco
                if (sm == ScanMode.ConnectableDiscoverable) {
                    m |= RadioModes.Discoverable | RadioModes.Connectable;
                } else if (sm == ScanMode.Connectable) {
                    m |= RadioModes.Connectable;
                } else if (sm == ScanMode.None) {
                }
                //
                return m;
            }
        }

        HardwareStatus IBluetoothRadio.HardwareStatus
        {
            get
            {
                HardwareStatus hws;
                switch (_adapter.State) {
#if false // API Level 14
                    // Thise four are not Radio state enums they are Profile state enums.
                    // Mono has combined the two into one enum.
                    case State.Disconnected:
                    case State.Connecting:
                    case State.Connected:
                    case State.Disconnecting:
                        break;
#endif
                    //
                    case State.Off:
                        hws = HardwareStatus.Shutdown;
                        break;
                    case State.TurningOn:
                        hws = HardwareStatus.Initializing;
                        break;
                    case State.On:
                        hws = HardwareStatus.Running;
                        break;
                    case State.TurningOff:
                        hws = HardwareStatus.Shutdown;
                        break;
                    default:
                        if (!TestUtilities.IsUnderTestHarness()) {
                            Debug.Fail("Unknown radio state: '" + _adapter.State
                                + "' (0x" + ((int)_adapter.State).ToString("X") + ").");
                        }
                        hws = HardwareStatus.Unknown;
                        break;
                }
                return hws;
            }
        }

        RadioMode IBluetoothRadio.Mode
        {
            get
            {
                var modes = ((IBluetoothRadio)this).Modes;
                if ((modes & RadioModes.PowerOff) != 0) {
                    return RadioMode.PowerOff;
                }
                if ((modes & RadioModes.Discoverable) != 0) {
                    if (!TestUtilities.IsUnderTestHarness()) {
                        Debug.Assert((modes & RadioModes.Connectable) != 0,
                            "Is discoverable is it always connectable?");
                    }
                    return RadioMode.Discoverable;
                }
                if (!TestUtilities.IsUnderTestHarness()) {
                    Debug.Assert((modes & RadioModes.Connectable) != 0,
                        "Is it always connectable?");
                }
                if ((modes & RadioModes.Connectable) != 0) {
                    return RadioMode.Connectable;
                }
                return RadioMode.PowerOff;// Not true but better that saying Connectable.
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        void IBluetoothRadio.SetMode(bool? connectable, bool? discoverable)
        {
            throw new NotImplementedException();
        }

        IntPtr IBluetoothRadio.Handle
        {
            get { return IntPtr.Zero; }
        }

        string IBluetoothRadio.Remote
        {
            get { return null; }
        }

        Manufacturer IBluetoothRadio.SoftwareManufacturer
        {
            // warning CS0618: 'InTheHand.Net.Bluetooth.Manufacturer.AndroidXxxx' is obsolete: 
#pragma warning disable 618
            get { return Manufacturer.AndroidXxxx; }
#pragma warning restore 618
        }

        HciVersion IBluetoothRadio.HciVersion
        {
            get { return HciVersion.Unknown; }
        }

        int IBluetoothRadio.HciRevision
        {
            get { return 0; }
        }

        LmpVersion IBluetoothRadio.LmpVersion
        {
            get { return _fakeVers.LmpVersion; }
        }

        int IBluetoothRadio.LmpSubversion
        {
            get { return _fakeVers.LmpSubversion; }
        }

        LmpFeatures IBluetoothRadio.LmpFeatures
        {
            get { return _fakeVers.LmpSupportedFeatures; }
        }

        Manufacturer IBluetoothRadio.Manufacturer
        {
            get { return _fakeVers.Manufacturer; }
        }

        #endregion

        internal BluetoothAdapter Adapter
        {
            get { return _adapter; }
        }

    }
}
#endif

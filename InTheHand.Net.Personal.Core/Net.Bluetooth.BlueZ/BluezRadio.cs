//TODO ---------------------------------------------REMOVE ALL OTHER CHANGES AND COMMIT Name PROPERTY CHANGES.
// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezRadio
// 
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License
#if BlueZ
using System;
using System.Collections.Generic;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics.CodeAnalysis;
using NDesk.DBus;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    sealed class BluezRadio : IBluetoothRadio
    {
        readonly BluezFactory _fcty;
        readonly int _dd;
        readonly BluetoothAddress _addr;
        readonly byte[] _nameTmp;
        Structs.hci_version _versions = new Structs.hci_version(HciVersion.Unknown);
        LmpFeatures _lmpFeatures;
        bool _doneVersions;
        readonly ObjectPath _objectPath;
        readonly BluezDbusInterface.Adapter _adapter;

        //----
        internal BluezRadio(BluezFactory fcty, int dd)
        {
            _dd = dd;
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
            BluezError ret;
            var bdaddr = BluezUtils.FromBluetoothAddress(BluetoothAddress.None);
            ret = NativeMethods.hci_read_bd_addr(_dd, bdaddr, _fcty.StackTimeout);
            //TODO BluezUtils.CheckAndThrow(ret, "hci_read_bd_addr");
            BluezUtils.Assert(ret, "hci_read_bd_addr");
            if (BluezUtils.IsSuccess(ret)) {
                _addr = BluezUtils.ToBluetoothAddress(bdaddr);
                Console.WriteLine("Radio SUCCESS, addr: " + _addr);
            } else {
                // NEVER used EXCEPT in the debugger if we skip the CheckandThrow above.
                _addr = BluetoothAddress.None;
                Console.WriteLine("Radio FAIL, addr: " + _addr);
            }
            _nameTmp = new byte[250];
            //
            // First find _objectPath. In the future we'll be passed this.
            var ax = _fcty.BluezDbus.FindAdapter(_addr, out _objectPath);
            Debug.Assert(_objectPath != null, "BluezRadio..ctor NOT _objectPath!=null");
            //--
            // Set Adapter.
            _adapter = GetAdapter(_objectPath);
            Console.WriteLine("Got adapter at .ctor.3.");
            //
            var prop = GetProperties();
            string addrDt = (string)prop[PropertyName.Address];
            var addrD = BluetoothAddress.Parse(addrDt);
            Utils.MiscUtils.AssertEquals(addrD, _addr);
            Console.WriteLine("Check DONE Radio..ctor. " + addrD + " vs " + _addr);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Bahh  D-Bus library uses Exception.")]
        BluezDbusInterface.Adapter GetAdapter(ObjectPath path)
        {
            try {
                var a = _fcty.BluezDbus.GetAdapter(path);
                return a;
            } catch (Exception ex) {
                Debug.Assert(ex.Message.StartsWith("org.bluez.Error.NoSuchAdapter"),
                    "Unexpected exception type: " + ex);
                if (ex.Message.StartsWith("org.bluez.Error.NoSuchAdapter", StringComparison.OrdinalIgnoreCase)) {
                    throw new InvalidOperationException("Radio does not exist.", ex);
                } else {
                    throw;
                }
            }
        }

        private IDictionary<string, object> GetProperties()
        {
            var prop = _adapter.GetProperties();
            return prop;
        }

        private void SetMode(RadioMode value)
        {
            var a = _adapter;
            if (value == RadioMode.PowerOff) {
                a.SetProperty(BluezRadio.PropertyName.Powered, false);
            } else if (value == RadioMode.Connectable) {
                a.SetProperty(BluezRadio.PropertyName.Powered, true);
            } else if (value == RadioMode.Discoverable) {
                a.SetProperty(BluezRadio.PropertyName.Powered, true);
                a.SetProperty(BluezRadio.PropertyName.Discoverable, true);
            } else {
                throw new ArgumentException("Unknown enum value: " + value);
            }
            Console.WriteLine("DONE SetAdapterMode.");
        }

        //----
        BluetoothAddress IBluetoothRadio.LocalAddress
        {
            get { return _addr; }
        }

        string IBluetoothRadio.Name
        {
            get
            {
                var pd = GetProperties();
                var name = (string)pd[PropertyName.Name];
                return name;
            }
            set
            {
                var a = _fcty.BluezDbus.GetAdapter(_objectPath);
                a.SetProperty(PropertyName.Name, value);
            }
        }

        //--
        ClassOfDevice IBluetoothRadio.ClassOfDevice
        {
            get
            {
                ClassOfDevice cod;
                var codArr = new byte[3 + 3];
                var ret = NativeMethods.hci_read_class_of_dev(_dd, codArr, _fcty.StackTimeout);
                BluezUtils.Assert(ret, "hci_read_class_of_dev");
                if (BluezUtils.IsSuccess(ret)) {
                    cod = BluezUtils.ToClassOfDevice(codArr);
                } else {
                    cod = new ClassOfDevice(0);
                }
                //
                ClassOfDevice codP;
                var pd = GetProperties();
                //BluezUtils.DumpKeys(pd);
                if (pd == null) {
                    Debug.Assert(!BluezUtils.IsSuccess(ret), "fail but they worked!");
                    codP = new ClassOfDevice(uint.MaxValue);
                } else {
                    Debug.Assert(BluezUtils.IsSuccess(ret), "success but they failed!");
                    var codIntP = (UInt32?)pd[PropertyName.Class];
                    if (!codIntP.HasValue) {
                        Debug.Fail("No value but they worked(?)!");
                        codP = new ClassOfDevice(uint.MaxValue - 1);
                    } else {
                        codP = new ClassOfDevice(codIntP.Value);
                        //Utils.MiscUtils.AssertEquals(cod, codP);
                        if (!(cod == codP)) { Debug.WriteLine("Not equals Radio CoD vs DBus"); }
                    }
                }
                // The D-Bus API is not working for me on Ubuntu..............
                Console.WriteLine("Done CHECK DBus Radio CoD: " + cod + " vs " + codP);
                //
                return cod;
            }
        }

        //--
        void ReadVersions()
        {
            if (_doneVersions)
                return;
            var vers = new Structs.hci_version(HciVersion.Unknown);
            var ret = NativeMethods.hci_read_local_version(_dd, ref vers, _fcty.StackTimeout);
            BluezUtils.Assert(ret, "hci_read_local_version");
            if (BluezUtils.IsSuccess(ret)) {
                _versions = vers;
            }
            _doneVersions = true; // Always set, as unlikely to work second time if failed first time.
            //
            var arr = new byte[8];
            ret = NativeMethods.hci_read_local_features(_dd, arr, _fcty.StackTimeout);
            if (BluezUtils.IsSuccess(ret)) {
                _lmpFeatures = (LmpFeatures)BitConverter.ToInt64(arr, 0);
            }
        }

        Manufacturer IBluetoothRadio.Manufacturer
        {
            get
            {
                ReadVersions();
                return (Manufacturer)_versions.manufacturer;
            }
        }

        LmpVersion IBluetoothRadio.LmpVersion
        {
            get
            {
                ReadVersions();
                return (LmpVersion)_versions.lmp_ver;
            }
        }

        int IBluetoothRadio.LmpSubversion
        {
            get
            {
                ReadVersions();
                return _versions.lmp_subver;
            }
        }

        LmpFeatures IBluetoothRadio.LmpFeatures { get { return _lmpFeatures; } }

        HciVersion IBluetoothRadio.HciVersion
        {
            get
            {
                ReadVersions();
                return (HciVersion)_versions.hci_ver;
            }
        }

        int IBluetoothRadio.HciRevision
        {
            get
            {
                ReadVersions();
                return _versions.hci_rev;
            }
        }

        Manufacturer IBluetoothRadio.SoftwareManufacturer
        {
#pragma warning disable 618 // '...' is obsolete: '...'
            get { return Manufacturer.BlueZXxxx; }
#pragma warning restore 618
        }

        //----
        HardwareStatus IBluetoothRadio.HardwareStatus
        {
            get
            {
                var prop = GetProperties();
                if (prop == null)
                    return HardwareStatus.Shutdown;
                var power = (bool)prop[PropertyName.Powered];
                if (power)
                    return HardwareStatus.Running;
                return HardwareStatus.Shutdown;
            }
        }

        internal static class PropertyName
        {
            internal const string Address = "Address";
            internal const string Name = "Name";
            internal const string Class = "Class";
            internal const string Powered = "Powered";
            internal const string Discoverable = "Discoverable";
            internal const string Pairable = "Pairable";
            internal const string PaireableTimeout = "PaireableTimeout";
            internal const string DiscoverableTimeout = "DiscoverableTimeout";
            internal const string Discovering = "Discovering";
            internal const string Devices = "Devices";
            internal const string UUIDs = "UUIDs";
        }

        RadioModes IBluetoothRadio.Modes
        {
            get
            {
                RadioModes modes = 0;
                var prop = GetProperties();
                if (prop == null)
                    return RadioModes.PowerOff;
                var power = (bool)prop[PropertyName.Powered];
                if (power) {
                    modes |= RadioModes.PowerOn;
                    // Assume always connectable.
                    modes |= RadioModes.Connectable;
                } else {
                    modes |= RadioModes.PowerOff;
                }
                var disco = (bool)prop[PropertyName.Discoverable];
                if (disco) { modes |= RadioModes.Discoverable; }
                return modes;
            }
        }

        public void SetMode(bool? connectable, bool? discoverable)
        {
            if (connectable == false) // HasValue && ==false
                throw new ArgumentException("BlueZ appears not to support non-connectable mode.");
            switch (discoverable) {
                case true:
                    SetMode(RadioMode.Discoverable);
                    break;
                case false:
                    SetMode(RadioMode.Connectable);
                    break;
                // null NOP
            }
        }

        RadioMode IBluetoothRadio.Mode
        {
            get
            {
                var prop = GetProperties();
                if (prop == null)
                    return RadioMode.PowerOff;
                var power = (bool)prop[PropertyName.Powered];
                if (!power)
                    return RadioMode.PowerOff;
                var disco = (bool)prop[PropertyName.Discoverable];
                if (disco)
                    return RadioMode.Discoverable;
                return RadioMode.Connectable;
            }
            set { SetMode(value); }
        }

        IntPtr IBluetoothRadio.Handle
        {
            get { throw new NotImplementedException(); }
        }

        //--
        string IBluetoothRadio.Remote
        {
            get { return null; }
        }

    }
}
#endif

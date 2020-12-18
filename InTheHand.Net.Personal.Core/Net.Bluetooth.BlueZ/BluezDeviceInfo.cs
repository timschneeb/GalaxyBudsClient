// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezDeviceInfo
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License
#if BlueZ
using System;
using System.Collections.Generic;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using NDesk.DBus;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    internal sealed class BluezDeviceInfo : IBluetoothDeviceInfo
    {
        static bool _propDumped;
        //
        readonly BluezFactory _fcty;
        readonly BluetoothAddress _addr;
        ClassOfDevice _cod;
        string _name;
        bool _hasDeviceName;
        BluezSdpQuery _sdpQuery;
        bool _rmbd, _authd, _connd;
        ObjectPath _dbusPath;
        DateTime _discoTime;
        int? _rssiAtInquiry;

        //----
        // Use factory method to create.
        private BluezDeviceInfo(BluezFactory fcty, BluetoothAddress addr)
        {
            Debug.Assert(fcty != null, "ArgNull");
            _fcty = fcty;
            Debug.Assert(addr != null, "ArgNull");
            _addr = addr;
            _cod = new ClassOfDevice(0);
        }

        //----
        internal static BluezDeviceInfo CreateFromGivenAddress(BluezFactory fcty, BluetoothAddress address)
        {
            var bdi = new BluezDeviceInfo(fcty, address);
            ObjectPath objectPath;
            var dict = fcty.BluezDbus.FindDeviceProperties_OnDefaultAdapter(address, out objectPath);
            if (dict != null) {
                SetProperties(objectPath, bdi, dict);
            } else {
                Console.WriteLine("No dbus device properties.");
            }
            return bdi;
        }

        internal static BluezDeviceInfo CreateFromInquiry(BluezFactory fcty, Structs.inquiry_info cur)
        {
            var addr = BluezUtils.ToBluetoothAddress(cur.bdaddr);
            var bdi = new BluezDeviceInfo(fcty, addr);
            bdi._cod = BluezUtils.ToClassOfDevice(cur.dev_class);
            // Note the devices here aren't 'created' by BlueZ so no ObjectPath.
            // We'll lookup/create the device later if required.
            //
            return bdi;
        }

        internal static BluezDeviceInfo CreateFromInquiryLive(BluezFactory fcty,
            IDictionary<string, object> dbusDeviceDict)
        {
            var addr = BluetoothAddress.Parse((string)dbusDeviceDict["Address"]);
            var bdi = new BluezDeviceInfo(fcty, addr);
            // "keys: Address, Class, Icon, RSSI, Name, Alias, LegacyPairing, Paired"
            // Note the devices here aren't 'created' by BlueZ so no ObjectPath.
            // We'll lookup/create the device later if required.
            SetProperties(null, bdi, dbusDeviceDict);
            object value;
            if (dbusDeviceDict.TryGetValue("RSSI", out value)) {
                // Int16--Console.WriteLine("RSSI type: " + value.GetType().Name);
                bdi._rssiAtInquiry = (int?)Convert.ToInt32(value);
            } else {
                Console.WriteLine("Where's the 'RSSI' property?!?");
            }
            //
            return bdi;
        }

        internal static BluezDeviceInfo CreateFromStored(BluezFactory fcty,
            ObjectPath objectPath,
            BluetoothAddress address,
            IDictionary<string, object> dbusDeviceDict)
        {
            var bdi = new BluezDeviceInfo(fcty, address);
            if (!_propDumped) {
                _propDumped = true;
                BluezUtils.DumpKeys(dbusDeviceDict);
                // "keys: Address, Name, Alias, Class, Icon, Paired, Trusted, Connected, UUIDs, Adapter"
            }
            SetProperties(objectPath, bdi, dbusDeviceDict);
            return bdi;
        }

        private static void SetProperties(ObjectPath objectPath,
            BluezDeviceInfo bdi, IDictionary<string, object> dbusDeviceDict)
        {
            // Stored:
            //   "keys: Address, Name, Alias, Class, Icon, Paired, Trusted, Connected, UUIDs, Adapter"
            // D-Bus Inquiry event:
            //   "keys: Address, Name, Alias, Class, Icon, Paired; RSSI, LegacyPairing"
            //   ("keys: Address, Class, Icon, RSSI, Name, Alias, LegacyPairing, Paired")
#if DEBUG
            var checkAddr = BluetoothAddress.Parse((string)dbusDeviceDict["Address"]);
            Debug.Assert(bdi._addr == checkAddr, "NOT EQUAL address: " + bdi._addr + " checkAddr: " + checkAddr);
#endif
            bdi._rmbd = true;
            bdi._dbusPath = objectPath;
            BluezUtils.DebugKeyExists(dbusDeviceDict, "Paired");
            bdi._authd = (bool)dbusDeviceDict["Paired"];
            BluezUtils.DebugKeyExists(dbusDeviceDict, "Connected");
            object value;
            if (dbusDeviceDict.TryGetValue("Connected", out value)) {
                bdi._connd = (bool)value;
            }
            BluezUtils.DebugKeyExists(dbusDeviceDict, "Alias");
            bdi._name = (string)dbusDeviceDict["Alias"];
            // Seen not present when callled from CreateFromGivenAddress from BtCli.GetRemoteMachineName.
            BluezUtils.DebugKeyExists(dbusDeviceDict, "Class");
            if (dbusDeviceDict.ContainsKey("Class")) {
                bdi._cod = new ClassOfDevice((UInt32)dbusDeviceDict["Class"]);
            } else {
                bdi._cod = new ClassOfDevice(0);
            }
            Console.WriteLine("After SetProperties, _name: " + bdi._name);
        }

        private static void EnsureHasObjectPath(BluezDeviceInfo bdi)
        {
            // A device from discovery (D-Bus or HCI) isn't 'created' by BlueZ 
            // so no ObjectPath. So we have to lookup/create the device now.
            if (bdi._dbusPath != null)
                return;
            var pd = bdi._fcty.BluezDbus.FindDeviceProperties_OnDefaultAdapter(bdi._addr, out bdi._dbusPath);
            Debug.Assert((pd == null) == (bdi._dbusPath == null), "Xor! "
                + Utils.MiscUtils.ToStringQuotedOrNull(pd) + " vs "
                + Utils.MiscUtils.ToStringQuotedOrNull(bdi._dbusPath));
            if (pd == null) {
                // Doesn't exist, so have to create the device now.
                var a = bdi._fcty.BluezDbus.GetDefaultAdapter();
                bdi._dbusPath = a.CreateDevice(BluezUtils.FromBluetoothAddressToDbus(bdi._addr));
            }
            Debug.Assert(bdi._dbusPath != null, "NOT _dbusPath!=null after EnsureHasObjectPath.");
            Console.WriteLine("EnsureHasObjectPath after: "
                + Utils.MiscUtils.ToStringQuotedOrNull(bdi._dbusPath));
        }

        //----
        BluetoothAddress IBluetoothDeviceInfo.DeviceAddress
        {
            get { return _addr; }
        }

        string IBluetoothDeviceInfo.DeviceName
        {
            get
            {
                if (_name == null) {
                    Console.WriteLine("_name is null");
                    //
                    EnsureHasObjectPath(this);
                    Debug.Assert(_dbusPath != null, "(2) NOT _dbusPath!=null");
                    var d = _fcty.BluezDbus.GetDevice((NDesk.DBus.ObjectPath)_dbusPath);
                    var pd = d.GetProperties();
                    if (pd.ContainsKey("Alias")) {
                        _name = (string)pd["Alias"];
                        _hasDeviceName = true;
                    } else {
                        SetDefaultDeviceName();
                        Debug.Assert(!_hasDeviceName, "NOT !_hasDeviceName");
                    }
                    //
#if TESTING_OBSOLETING
                    byte[] bdaddr = BluezUtils.FromBluetoothAddress(_addr);
                    byte[] nameBuf = new byte[250];
                    var ret = NativeMethods.hci_read_remote_name(_fcty.DevDescr, bdaddr,
                        nameBuf.Length, nameBuf, _fcty.StackTimeout);
                    Debug.WriteLine("hci_read_remote_name ret: " + ret);
                    Console.WriteLine("hci_read_remote_name ret: {0}.", ret);
                    string nameHci;
                    if (BluezUtils.IsSuccess(ret)) {
                        nameHci = BluezUtils.FromNameString(nameBuf);
                        Utils.MiscUtils.AssertEquals(_name, nameHci);
                        Debug.Assert(_hasDeviceName, "NOT _hasDeviceName==true");
                    } else {
                        Debug.Assert(!_hasDeviceName, "NOT _hasDeviceName==false");
                        nameHci = null;
                    }
                    Console.WriteLine("Name D-Bus vs HCI: " + _name + " vs " + nameHci);
#endif
                }
                return _name;
            }
            set { _name = value; }
        }

        public bool HasDeviceName { get { return _hasDeviceName; } }

        private void SetDefaultDeviceName()
        {
            _name = _addr.ToString("C");
        }


        #region IBluetoothDeviceInfo Members

        void IBluetoothDeviceInfo.Merge(IBluetoothDeviceInfo other)
        {
            _authd = other.Authenticated;
            Debug.Assert(this._cod.Equals(other.ClassOfDevice), "ClassOfDevice " + this._cod + " <> " + other.ClassOfDevice);
            Debug.Assert(this._connd == other.Connected, "Connected " + this._connd + " <> " + other.Connected);
            Debug.Assert(this._addr == other.DeviceAddress, "DeviceAddress " + this._addr + " <> " + other.DeviceAddress);
            //Debug.Assert(this._cachedName == other.DeviceName, "DeviceName '" + this._cachedName + "' <> '" + other.DeviceName + "'");
            if (this._name == null || !_hasDeviceName) {
                this._name = other.DeviceName;
                //TODO this._hasDeviceName = other.HasDeviceName;
            }
            _rmbd = other.Remembered;
        }

        void IBluetoothDeviceInfo.SetDiscoveryTime(DateTime dt)
        {
            _discoTime = dt;
        }

        bool IBluetoothDeviceInfo.Remembered
        {
            get { return _rmbd; }
        }

        bool IBluetoothDeviceInfo.Authenticated
        {
            get { return _authd; }
        }

        bool IBluetoothDeviceInfo.Connected
        {
            get { return _connd; }
        }

        //
        DateTime IBluetoothDeviceInfo.LastSeen
        {
            get { return _discoTime; }
        }

        DateTime IBluetoothDeviceInfo.LastUsed
        {
            get { return DateTime.MinValue; }
        }

        //
        ClassOfDevice IBluetoothDeviceInfo.ClassOfDevice
        {
            get { return _cod; }
        }

        int IBluetoothDeviceInfo.Rssi
        {
            get { return int.MinValue; }
        }

        //----
        void IBluetoothDeviceInfo.Refresh()
        {
            _name = null;
            _hasDeviceName = false;
        }

        //----
        byte[][] IBluetoothDeviceInfo.GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotSupportedException("Can't get the raw record from this stack (or tell me how!).");
        }

        List<ServiceRecord> DoGetServiceRecords(Guid service)
        {
            if (_sdpQuery == null) {
                _sdpQuery = new BluezSdpQuery(_fcty);
            }
            var list = _sdpQuery.DoSdpQueryWithConnect(_addr, service, false);
            return list;
        }

        ServiceRecord[] IBluetoothDeviceInfo.GetServiceRecords(Guid service)
        {
            var list = DoGetServiceRecords(service);
            return list.ToArray();
        }

        IAsyncResult IBluetoothDeviceInfo.BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            if (_sdpQuery == null) {
                _sdpQuery = new BluezSdpQuery(_fcty);
            }
            var ar = _sdpQuery.BeginQuery(_addr, service, false, callback, state);
            return ar;
        }

        ServiceRecord[] IBluetoothDeviceInfo.EndGetServiceRecords(IAsyncResult asyncResult)
        {
            if (_sdpQuery == null) {
                throw new InvalidOperationException("Need a previous call to BeginGetServiceRecords.");
            }
            var list = _sdpQuery.EndQuery(asyncResult);
            return list.ToArray();
        }

        //----
        Guid[] IBluetoothDeviceInfo.InstalledServices
        {
            get { throw new NotImplementedException(); }
        }

        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state)
        {
            throw new NotImplementedException();
        }

        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            throw new NotImplementedException("GetVersions not currently supported by this stack.");
        }

        void IBluetoothDeviceInfo.ShowDialog()
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.Update()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
#endif
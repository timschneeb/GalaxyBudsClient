// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezDbus
// 
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License
#if BlueZ
//
//
using NDesk.DBus;
using DbusPropertyDictionary = System.Collections.Generic.IDictionary<string, object>;
//
using System;
using System.Collections.Generic;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    class MyDbusRunner
    {
        Bus _fctyBus;
        Exception _startupException;
        ManualResetEvent _started = new ManualResetEvent(false);

        //----
        public MyDbusRunner()
        {
            // Run a message loop for DBus on a new thread.
            var t = new Thread(Loop_Runner);
            t.IsBackground = true;
            t.Start();
            _started.WaitOne(60 * 1000);
            _started.Close();
            if (_startupException != null)
                throw _startupException;
        }

        //----
        protected Bus FactoryBus { get { return _fctyBus; } }

        //----
        public T GetObject<T>(string busName, ObjectPath path)
        {
            var obj = FactoryBus.GetObject<T>(busName, path);
            return obj;
        }

        //----
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Rethrown on main thread.")]
        void Loop_Runner()
        {
            try {
                // Open our own copy of the System bus, as a consumer
                // application could have opened it already and we would
                // then be relying on their threading.
                var tA = typeof(NDesk.DBus.Bus).Assembly
                    .GetType("NDesk.DBus.Address", true);
                string name = (string)tA.InvokeMember("System",
                    System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.GetProperty,
                    null, null, null,
                    CultureInfo.InvariantCulture);
                _fctyBus = new Bus(name); // Bus.System
            } catch (Exception ex) {
                _startupException = ex;
                return;
            } finally {
                _started.Set();
            }
            Console.WriteLine("Dbus loop running");
            //
            while (true) {
                _fctyBus.Iterate();
            }
        }
    }

    class BluezDbus : MyDbusRunner
    {
        internal const string Service = "org.bluez";
        //
        readonly BluezFactory _fcty;

        //----
        internal BluezDbus(BluezFactory fcty)
        {
            _fcty = fcty;
            //
            KeepDefaultAdapterAndRegisterForSignals();
        }

        //----
        public T GetObject<T>(ObjectPath path)
        {
            var obj = FactoryBus.GetObject<T>(Service, ObjectPath.Root);
            return obj;
        }

        //----
        public List<IBluetoothDeviceInfo> GetDeviceList_OnDefaultAdapter()
        {
            var a = GetDefaultAdapter();
            var inList = GetDevices(a);
            Console.WriteLine("got Devices[]");
            //Console.WriteLine("Devices is type: " + inList.GetType().FullName);
            var outList = new List<IBluetoothDeviceInfo>();
            foreach (var curDevicePath in inList) {
                var d = GetDevice(curDevicePath);
                var deviceDict = d.GetProperties();
                var addrStr = (string)deviceDict["Address"];
                Console.WriteLine("  GDL got addrStr: " + addrStr);
                var addr = BluetoothAddress.Parse(addrStr);
                var bdi = BluezDeviceInfo.CreateFromStored(_fcty, curDevicePath, addr, deviceDict);
                outList.Add(bdi);
            }
            return outList;
        }

        internal BluezDbusInterface.Device GetDevice(ObjectPath devicePath)
        {
            var d = FactoryBus.GetObject<BluezDbusInterface.Device>(Service, devicePath);
            return d;
        }

        private BluezDbusInterface.Device FindDevice_OnDefaultAdapter(
            BluetoothAddress address, out ObjectPath objectPath)
        {
            BluezDbusInterface.Adapter a;
            var d = FindDevice_OnDefaultAdapter(address, out objectPath, out a);
            return d;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Bahh  D-Bus library uses Exception.")]
        internal BluezDbusInterface.Device FindDevice_OnDefaultAdapter(
            BluetoothAddress address, out ObjectPath objectPath,
            out BluezDbusInterface.Adapter a)
        {
            a = GetDefaultAdapter();
            var addrStrIn = FromBluetoothAddress(address);
            Console.WriteLine("gonna FindDevice");
            ObjectPath devicePath;
            try {
                devicePath = a.FindDevice(addrStrIn);
            } catch (Exception ex) {
                Console.WriteLine("GetDevice_ error: " + ex.Message);
                Debug.Assert(ex.Message.StartsWith("org.bluez.Error.DoesNotExist:"),
                    "Unexected exception type: " + ex);
                if (ex.Message.StartsWith("org.bluez.Error.DoesNotExist:", StringComparison.OrdinalIgnoreCase)) {
                    objectPath = null;
                    return null;
                } else {
                    throw;
                }
            }
            objectPath = devicePath;
            Console.WriteLine("gonna Get {0}", devicePath);
            var d = FactoryBus.GetObject<BluezDbusInterface.Device>(Service, devicePath);
            return d;
        }

        public DbusPropertyDictionary FindDeviceProperties_OnDefaultAdapter(
            BluetoothAddress address, out ObjectPath objectPath)
        {
            BluezDbusInterface.Device d = FindDevice_OnDefaultAdapter(address, out objectPath);
            if (d == null) {
                return null;
            }
            var deviceDict = d.GetProperties();
            var addrStr = (string)deviceDict["Address"];
            Console.WriteLine("  GDP got addrStr: " + addrStr);
            return deviceDict;
        }

        //--------
        public bool RemoveDeviceFind_OnDefaultAdapter(BluetoothAddress device)
        {
            BluezDbusInterface.Adapter a;
            ObjectPath devicePath;
            BluezDbusInterface.Device d = FindDevice_OnDefaultAdapter(device, out devicePath, out a);
            if (d == null) {
                return false;
            }
            a.RemoveDevice(devicePath);
            return true;
        }

        internal bool PairRequest_OnDefaultAdapter(BluetoothAddress device, string pin)
        {
            var a = GetDefaultAdapter();
            ObjectPath agent;
            string capabilityStr;
            if (pin == null) {
                agent = new ObjectPath("/USE/THE/EXISTING/AGENTUI/KKASHKETXCSHDJDAJLDJLDJL");
                capabilityStr = string.Empty;
            } else {
                // TODO 
                agent = new ObjectPath("/OUR/AGENT");
                var capability = BluezDbusInterface.AgentCapability.NoInputNoOutput;
                capabilityStr = capability.ToString();
            }
            try {
                var path = a.CreatePairedDevice(FromBluetoothAddress(device), agent, capabilityStr);
                return true;
            } catch (Exception ex) {
                Console.WriteLine("PairRequest_ error: " + ex.Message);
                // "org.bluez.Error.AlreadyExists: Bonding already exists"
                if (ex.Message.StartsWith("org.bluez.Error.AlreadyExists:", StringComparison.OrdinalIgnoreCase)) {
                    Console.WriteLine("WARNING org.bluez.Error.AlreadyExists");
                    Debug.Fail("WARNING org.bluez.Error.AlreadyExists");
                    return true;
                } else if (ex.Message.StartsWith("org.bluez.Error.ConnectionAttemptFailed:", StringComparison.OrdinalIgnoreCase)) {
                    // "org.bluez.Error.ConnectionAttemptFailed: Network is down"
                    return false;
                } else {
                    throw;
                }
            }
        }

        //--------
        internal ObjectPath GetDefaultAdapterPath()
        {
            var mgr = FactoryBus.GetObject<BluezDbusInterface.Manager>(Service, ObjectPath.Root);
            Console.WriteLine("got Manager");
#if false
            string pathTmp;
            pathTmp = mgr.DefaultAdapter();
            Console.WriteLine("DefaultAdapter : string");
            var adapterPath = new ObjectPath(pathTmp);
#else
            ObjectPath adapterPath = mgr.DefaultAdapter();
            Console.WriteLine("DefaultAdapter : ObjectPath");
#endif
            return adapterPath;
        }

        internal BluezDbusInterface.Adapter GetDefaultAdapter()
        {
            ObjectPath adapterPath = GetDefaultAdapterPath();
            var a = FactoryBus.GetObject<BluezDbusInterface.Adapter>(Service, adapterPath);
            Console.WriteLine("got Adapter");
            return a;
        }

        //[Obsolete("No Find! Use the ObjectPath.")]
        public BluezDbusInterface.Adapter FindAdapter(
            BluetoothAddress localAddress, out ObjectPath objectPath)
        {
            var mgr = FactoryBus.GetObject<BluezDbusInterface.Manager>(Service, ObjectPath.Root);
            Console.WriteLine("got Manager");
            ObjectPath adapterPath = mgr.FindAdapter(FromBluetoothAddress(localAddress));
            objectPath = adapterPath;
            return GetAdapter(adapterPath);
        }

        internal BluezDbusInterface.Adapter GetAdapter(ObjectPath adapterPath)
        {
            var a = FactoryBus.GetObject<BluezDbusInterface.Adapter>(Service, adapterPath);
            Console.WriteLine("got Adapter");
            return a;
        }

        private static ObjectPath[] GetDevices(BluezDbusInterface.Adapter a)
        {
#if false
            var inList = GetProperty<DbusPropertyDictionary>(a, "Devices");
#else
            var dictTmp = a.GetProperties();
            //Console.WriteLine("dictTmp is type: " + dictTmp.GetType().FullName);
            //Console.WriteLine("dictTmp is type: " + dictTmp.GetType().Name);
            var tmp = dictTmp["Devices"];
            //Console.WriteLine("Property Value is type: " + tmp.GetType().Name);
            var inList = (ObjectPath[])tmp;
#endif
            return inList;
        }

        //--------
        static string FromBluetoothAddress(BluetoothAddress address)
        {
            return BluezUtils.FromBluetoothAddressToDbus(address);
        }

        //----
        internal event Action<IBluetoothDeviceInfo> LiveDisco = delegate { };
        BluezDbusInterface.Adapter _liveDiscoAdapterHack;
        static bool _livePropDumped;

        private void KeepDefaultAdapterAndRegisterForSignals()
        {
            _liveDiscoAdapterHack = GetDefaultAdapter();
            _liveDiscoAdapterHack.DeviceFound += adapter_DeviceFound;
            _liveDiscoAdapterHack.PropertyChanged += _registeredAdapter_PropertyChanged;
            Console.WriteLine("Now Registered2");
        }

        void _registeredAdapter_PropertyChanged(string name, object newValue)
        {
            Console.WriteLine("xAdapter_PropertyChanged name: {0}, newValue: {1}",
                name, newValue);
        }

        void adapter_DeviceFound(string address, IDictionary<string, object> properties)
        {
            Console.WriteLine("xAdapter_DeviceFound addr: {0}, prop.Count: {1}",
                address, properties.Count);
            if (!_livePropDumped) {
                _livePropDumped = true;
                BluezUtils.DumpKeys(properties);
                // "keys: Address, Class, Icon, RSSI, Name, Alias, LegacyPairing, Paired"
            }
            var bdi = BluezDeviceInfo.CreateFromInquiryLive(_fcty, properties);
            LiveDisco(bdi);
        }

    }//class
}
#endif

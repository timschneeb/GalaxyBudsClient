// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if FAKE_ANDROID_BTH_API
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;
using Android.Bluetooth;

namespace InTheHand.Net.Bluetooth.Droid
{

    class MockAndroidBth1 : RealProxy
    {
        public static T Create<T>(AndroidMockValues values)
            where T : MarshalByRefObject
        {
            return Create<T>(Convert(values));
        }

        public static T Create<T>(IDictionary<KeyValuePair<string, string>, object> values)
            where T : MarshalByRefObject
        {
            var p = new MockAndroidBth1(typeof(T), values);
            return (T)p.GetTransparentProxy();
        }

        //----
        public static IDictionary<KeyValuePair<string, string>, object> OriginalValues
        {
            get
            {
                if (_originalValues == null) {
                    var values = GetOriginalValuesClass();
                    var d = Convert(values);
                    _originalValues = d;
                }
                return _originalValues;
            }
        }

        private static Dictionary<KeyValuePair<string, string>, object> Convert(AndroidMockValues values)
        {
            Func<IMethodCallMessage, object> dlgt;
            var d = new Dictionary<KeyValuePair<string, string>, object>();
            //----
            const string _ServiceClass = "_ServiceClass";
            dlgt = (msg) =>
            {
                var sc = (Android.Bluetooth.ServiceClass)d[new KeyValuePair<string, string>(_ServiceClass, null)];
                return DoHasServiceFunction(msg, sc);
            };
            d.Add(new KeyValuePair<string, string>("HasService", null), dlgt);
            //
            dlgt = _ => Create<BluetoothDevice>(d);
            d.Add(new KeyValuePair<string, string>("GetRemoteDevice", AdapterTypeName), dlgt);
            //
            dlgt = _ => Create<BluetoothClass>(d);
            d.Add(new KeyValuePair<string, string>("get_BluetoothClass", DeviceTypeName), dlgt);
            //
            dlgt = _ => new List<BluetoothDevice> { Create<BluetoothDevice>(d) };
            d.Add(new KeyValuePair<string, string>("get_BondedDevices", AdapterTypeName), dlgt);
            //----
            d.Add(new KeyValuePair<string, string>("get_Address", AdapterTypeName), values.Radio_Address);
            d.Add(new KeyValuePair<string, string>("get_Name", AdapterTypeName), values.Radio_Name);
            // Adapter
            d.Add(new KeyValuePair<string, string>("get_State", AdapterTypeName), values.Radio_State);
            d.Add(new KeyValuePair<string, string>("get_ScanMode", AdapterTypeName), values.Radio_ScanMode);
            // Device
            d.Add(new KeyValuePair<string, string>("get_Address", DeviceTypeName), values.Device1_Address);
            d.Add(new KeyValuePair<string, string>("get_Name", DeviceTypeName), values.Device1_Name);
            d.Add(new KeyValuePair<string, string>("get_BondState", DeviceTypeName), values.Device1_BondState);
            // Class
            d.Add(new KeyValuePair<string, string>("get_DeviceClass", null), values.Device1_Class_DeviceClass);
            d.Add(new KeyValuePair<string, string>(_ServiceClass, null), values.Device1_Class_ServiceClass);
            //
            return d;
        }

        private static AndroidMockValues GetOriginalValuesClass()
        {
            Android.Bluetooth.ServiceClass scIIII = Android.Bluetooth.ServiceClass.LimitedDiscoverability | Android.Bluetooth.ServiceClass.ObjectTransfer;
            var v = new AndroidMockValues
            {
                Radio_Address = "00:11:22:AA:BB:CC",
                Radio_Name = "MyRadio1",
                // Adapter
                Radio_State = Android.Bluetooth.State.On,
                Radio_ScanMode = Android.Bluetooth.ScanMode.Connectable,
                // Device
                Device1_Address = "00:91:A2:2A:3B:4C",
                Device1_Name = "TheRemoteDeviceName",
                Device1_BondState = Bond.None,
                // Class
                Device1_Class_DeviceClass = Android.Bluetooth.DeviceClass.HealthPulseOximeter,
                Device1_Class_ServiceClass = scIIII,
            };
            return v;
        }

        //----
        private MockAndroidBth1(Type t, IDictionary<KeyValuePair<string, string>, object> values)
            : base(t)
        {
            _values = values;
        }

        const string AdapterTypeNameWithComma = "Android.Bluetooth.BluetoothAdapter,";
        const string DeviceTypeNameWithComma = "Android.Bluetooth.BluetoothDevice,";
        //
        internal const string AdapterTypeName = "Android.Bluetooth.BluetoothAdapter";
        internal const string DeviceTypeName = "Android.Bluetooth.BluetoothDevice";

        IDictionary<KeyValuePair<string, string>, object> _values;
        // As used by manual definitions in the code!
        static IDictionary<KeyValuePair<string, string>, object> _originalValues;

        public override IMessage Invoke(IMessage msg)
        {
            var mcMsg = (IMethodCallMessage)msg;
            object ret;
            string fault = null;
            if (mcMsg.MethodName == null) {
                Debug.Fail("Ehhh no method name!");
                fault = "Ehhh no method name!";
                ret = null;
            } else {
                var typeNameName = TruncateAt(mcMsg.TypeName, ',');
                var k = new KeyValuePair<string, string>(mcMsg.MethodName, typeNameName);
                var kNT = new KeyValuePair<string, string>(mcMsg.MethodName, null);
                if (_values.ContainsKey(k)) {
                    ret = _values[k];
                } else if (_values.ContainsKey(kNT)) {
                    ret = _values[kNT];
                } else if (mcMsg.MethodName == "GetType") {
                    ret = this.GetType();
                } else if (mcMsg.MethodName == "ToString") {
                    ret = this.ToString();
                } else {
                    fault = "Mock doesn't handle method name: '" + mcMsg.MethodName + "'.";
                    ret = null;
                }
            }
            if (fault != null) {
                throw new NotImplementedException(fault);
            }
            //
            if (ret is Delegate) {
                var dlgt = (Func<IMethodCallMessage, object>)ret;
                var retD = dlgt(mcMsg);
                ret = retD;
            }
            //
            var retM = new ReturnMessage(ret, null, 0, null, mcMsg);
            return retM;
        }

        private string TruncateAt(string text, char marker)
        {
            var idx = text.IndexOf(marker);
            if (idx == -1) throw new ArgumentException("Marker doesn't appear in string.");
            var txt2 = text.Substring(0, idx);
            return txt2;
        }

        private static object DoHasServiceFunction(IMethodCallMessage mcMsg, Android.Bluetooth.ServiceClass sc)
        {
            object ret;
            var checkSc = (Android.Bluetooth.ServiceClass)mcMsg.InArgs[0];
            var retB = (sc & checkSc) != 0;
            ret = retB;
#if DEBUG
            var cb = CountBits(checkSc);
            Debug.Assert(cb == 1, "Check bits has more that one bit set, has "
                + cb + "set, it is: " + checkSc + " i.e. 0x" + ((int)checkSc).ToString("X8"));
#endif
            return ret;
        }

        private static int CountBits(Android.Bluetooth.ServiceClass checkSc)
        {
            int v = (int)checkSc;
            int count = 0;
            for (int i = 1; i != 0; ShiftLeftUnchecked(ref i)) {
                if ((v & i) != 0)
                    ++count;
            }
            return count;
        }

        private static int ShiftLeftUnchecked(ref int i)
        {
            unchecked { return i <<= 1; }
        }

    }
}
#endif

// 32feet.NET - Personal Area Networking for .NET
//
// Copyright (c) 2013 Alan J McFarlane, All rights reserved.
// Copyright (c) 2013 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

#if ANDROID_BTH
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using Android.Bluetooth;
using Android.Runtime;
using Java.Util;

namespace InTheHand.Net.Bluetooth.Droid
{
    class AndroidBthDeviceInfo : IBluetoothDeviceInfo
    {
        readonly AndroidBthFactoryBase _fcty;
        readonly BluetoothDevice _dev;
        readonly BluetoothAddress _addr;
        DateTime _discoTime;
        bool _rmbd, _authd;
        ClassOfDevice _cod;
        int? _rssiAtDisco;


        // Use factory methods!!
        private AndroidBthDeviceInfo(AndroidBthFactoryBase fcty, BluetoothDevice dev)
        {
            _fcty = fcty;
            _dev = dev;
            _addr = BluetoothAddress.Parse(dev.Address);
        }

        internal static AndroidBthDeviceInfo CreateFromGivenAddress(
            AndroidBthFactoryBase fcty, BluetoothAddress address,
            bool queryOrInternalOnly)
        {
            BluetoothAdapter a = fcty.GetAdapter();
            var dev = a.GetRemoteDevice(AndroidBthUtils.FromBluetoothAddress(address));
            var bdi = new AndroidBthDeviceInfo(fcty, dev);
            //
            if (queryOrInternalOnly) {
                bdi._rmbd = bdi._authd = (dev.BondState == Bond.Bonded);
                var cod = dev.BluetoothClass;
                if (cod != null) {
                    bdi._cod = AndroidBthUtils.ConvertCoDs(cod);
                }
            }
            return bdi;
        }

        internal static AndroidBthDeviceInfo CreateFromBondedList(AndroidBthFactoryBase fcty, BluetoothDevice dev)
        {
            var bdi = new AndroidBthDeviceInfo(fcty, dev);
            bdi._rmbd = bdi._authd = true;
            return bdi;
        }

        internal static AndroidBthDeviceInfo CreateFromInquiry(
            AndroidBthFactoryBase fcty, BluetoothDevice dev,
            BluetoothClass cod, string nameOpt, short rssiOpt)
        {
            var bdi = new AndroidBthDeviceInfo(fcty, dev);
            bdi._discoTime = DateTime.UtcNow;
            bdi._cod = AndroidBthUtils.ConvertCoDs(cod);
            bdi._rssiAtDisco = rssiOpt;
            Debug.Assert(bdi._dev.Name == nameOpt);
            return bdi;
        }

        #region IBluetoothDeviceInfo Members

        BluetoothAddress IBluetoothDeviceInfo.DeviceAddress
        {
            get { return _addr; }
        }

        string IBluetoothDeviceInfo.DeviceName
        {
            get { return _dev.Name; }
            set { throw new NotSupportedException(); }
        }

        //----
        void IBluetoothDeviceInfo.Merge(IBluetoothDeviceInfo other)
        {
            _authd = other.Authenticated;
            Debug.Assert(this._cod.Equals(other.ClassOfDevice), "ClassOfDevice " + this._cod + " <> " + other.ClassOfDevice);
            //Debug.Assert(this._connd == other.Connected, "Connected " + this._connd + " <> " + other.Connected);
            Debug.Assert(this._addr == other.DeviceAddress, "DeviceAddress " + this._addr + " <> " + other.DeviceAddress);
            //Debug.Assert(this._cachedName == other.DeviceName, "DeviceName '" + this._cachedName + "' <> '" + other.DeviceName + "'");
            //if (this._name == null || !_hasDeviceName) {
            //    this._name = other.DeviceName;
            //    //TODO this._hasDeviceName = other.HasDeviceName;
            //}
            _rmbd = other.Remembered;
        }

        void IBluetoothDeviceInfo.SetDiscoveryTime(DateTime dt)
        {
            _discoTime = dt;
        }

        DateTime IBluetoothDeviceInfo.LastSeen
        {
            get { return _discoTime; }
        }

        DateTime IBluetoothDeviceInfo.LastUsed
        {
            get { return DateTime.MinValue; }
        }

        //----
        void IBluetoothDeviceInfo.Refresh()
        {
            // We don't cache the name.
        }

        void IBluetoothDeviceInfo.Update()
        {
            throw new NotImplementedException();
        }

        //----
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
            get { return false; } // There's no easy way to get if connected.
        }

        //----
        ClassOfDevice IBluetoothDeviceInfo.ClassOfDevice
        {
            get
            {
                if (_cod == null) return new ClassOfDevice(0);
                return _cod;
            }
        }

        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            throw new NotSupportedException("Android does not support getting radio version information.");
        }

        int IBluetoothDeviceInfo.Rssi
        {
            get
            {
                if (_rssiAtDisco.HasValue)
                    return _rssiAtDisco.Value;
                return int.MinValue;
            }
        }

        //----
        byte[][] IBluetoothDeviceInfo.GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotImplementedException();
        }

#if ANDROID_API_LEVEL_15
        bool _doneFetchSdp;
#endif

        ServiceRecord[] IBluetoothDeviceInfo.GetServiceRecords(Guid service)
        {
#if !ANDROID_API_LEVEL_15
            throw new NotSupportedException("A basic GetServiceRecords may be possible at API level 15.");
#else
            // HAC.K Just call FetchUuidsWithSdp and don't wait for the Intent
            // that signals that it has completed. So for now the user MUST
            // call this function, wait a wee while and call it again. The 
            // first will return zero records.
            if (!_doneFetchSdp) {
                bool ok;
                try {
                    ok = _dev.FetchUuidsWithSdp();
                } catch (Java.Lang.NullPointerException npex) {
                    throw new InvalidOperationException("Device Discovery failed; maybe Bluetooth is disabled.", npex);
                } catch (Exception ex) {
                    throw new InvalidOperationException("Device Discovery failed; maybe Bluetooth is disabled.", ex);
                }
                if (!ok)
                    throw new InvalidOperationException("GetServiceRecords failed; maybe Bluetooth is disabled.");
                _doneFetchSdp = true;
            }
            var /*ParcelUuid[]*/ list = _dev.GetUuids();
            if (list == null) // Or in current hack: we've not called FetchUuidsWithSdp previously.
                throw new InvalidOperationException("Device Discovery failed; maybe Bluetooth is disabled.");
            var recList = from pu in list
                          let u = pu.Uuid
                          let g = _fcty.FromJavaUuid(u)
                          select CreateRecordContainingOnlyClassId(g);
            return recList.ToArray();
#endif
        }

#if ANDROID_API_LEVEL_15
        private ServiceRecord CreateRecordContainingOnlyClassId(Guid svcClassId)
        {
            List<ServiceAttribute> attrList = new List<ServiceAttribute>();
            // -- ClassIDs --
            attrList.Add(new ServiceAttribute(InTheHand.Net.Bluetooth.AttributeIds.UniversalAttributeId.ServiceClassIdList,
                new ServiceElement(ElementType.ElementSequence,
                    new ServiceElement(ElementType.Uuid128, svcClassId)
            )));
            return new ServiceRecord(attrList);
        }
#endif

        IAsyncResult IBluetoothDeviceInfo.BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        ServiceRecord[] IBluetoothDeviceInfo.EndGetServiceRecords(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        Guid[] IBluetoothDeviceInfo.InstalledServices
        {
            get { throw new NotImplementedException(); }
        }

        //----
        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state, bool throwOnError)
        {
            throw new NotImplementedException();
        }

        void IBluetoothDeviceInfo.SetServiceState(Guid service, bool state)
        {
            throw new NotImplementedException();
        }

        //----
        void IBluetoothDeviceInfo.ShowDialog()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Connections
        internal BluetoothSocket CreateSocket(BluetoothEndPoint remoteEP, bool auth, bool encr)
        {
            BluetoothSocket s;
            if (remoteEP.HasPort) {
                s = CreateSocketToPort(_dev, remoteEP.Port, auth, encr);
            }
            if (auth || encr) {
                s = _dev.CreateRfcommSocketToServiceRecord(
                    _fcty.ToJavaUuid(remoteEP.Service));
            } else {
                s = _dev.CreateInsecureRfcommSocketToServiceRecord(
                    _fcty.ToJavaUuid(remoteEP.Service));
            }
            return s;
        }

        private BluetoothSocket CreateSocketToPort(BluetoothDevice dev, int port, bool auth, bool encr)
        {
#if !FAKE_ANDROID_BTH_API
            string methodName = (auth || encr) ? "createRfcommSocket" : "createInsecureRfcommSocket";
            // http://mono-for-android.1047100.n5.nabble.com/Bluetooth-Service-Discovery-failed-exception-td5711896.html
            // http://stackoverflow.com/questions/9703779/connecting-to-a-specific-bluetooth-port-on-a-bluetooth-device-using-android
            IntPtr pCreateRfcommSocket = JNIEnv.GetMethodID(dev.Class.Handle, methodName, "(I)Landroid/bluetooth/BluetoothSocket;");
            IntPtr pSocket = JNIEnv.CallObjectMethod(dev.Handle, pCreateRfcommSocket, new Android.Runtime.JValue(port));
            var s = Java.Lang.Object.GetObject<BluetoothSocket>(pSocket, JniHandleOwnership.TransferLocalRef);
            return s;
#else
            throw new NotImplementedException("Reflection Connect to Port not implemented in FAKE_ANDROID_API.");
#endif
        }

        #endregion

        #region Security

        //public bool CreateBond()
        //{
        //    return _dev.CreateBond ();
        //}
        //
        //public bool RemoveBond()
        //{
        //    return _dev.RemoveBond ();
        //}

        #endregion

    }
}
#endif

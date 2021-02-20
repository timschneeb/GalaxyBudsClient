// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaDeviceInfo
// 
// Copyright (c) 2010-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Collections.Generic;
using System.Text;
using InTheHand.Net.Bluetooth.Factory;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Globalization;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaDeviceInfo : IBluetoothDeviceInfo
    {
        readonly BluetopiaFactory _factory;
        readonly BluetoothAddress _addr;
#if DEBUG
        internal
#endif
        string _cachedName;
        bool _hasRealName = false;
        bool _blockFindNameForNow;
        bool _paired, _remembered, _connected;
        ClassOfDevice _cod;
        DateTime _lastSeen;
        BluetopiaSdpQuery _sdpQuery;
        internal ClockOffsetEtc _clockOffsetEtc;

        //NativeMethods.GAP_Event_Callback _gapEventCallback;

        // Access is ONLY throw the following factory methods!!!!!!
        private BluetopiaDeviceInfo(BluetopiaFactory factory, BluetoothAddress device)
        {
            // Access ONLY ONLY throw the following factory methods!!!!!!
            if (factory == null) throw new ArgumentNullException("factory");
            _factory = factory;
            _addr = device;
            //
            // HAC-K
            // TO-DO ?_connected: Use GAP_Query_Connection_Handle
            _connected = false; // shutUpCompiler
            Debug.Assert(_connected == false, "_connected");
        }

        //----
#if DEBUG
        internal BluetopiaSdpQuery Testing_GetSdpQuery()
        {
            // The test needs to access this *before* we call BeginGSR, so create it here...
            if (_sdpQuery == null) {
                _sdpQuery = new BluetopiaSdpQuery(_factory);
            }
            return _sdpQuery;
        }
#endif

        //----
        internal static BluetopiaDeviceInfo CreateFromGivenAddress(BluetoothAddress addr, BluetopiaFactory factory)
        {
            return new BluetopiaDeviceInfo(factory, addr);
        }

        internal static BluetopiaDeviceInfo CreateFromStored(BluetopiaFactory factory,
            BluetoothAddress addr, string name, uint cod, bool paired)
        {
            var bdi = CreateFromGivenAddress(addr, factory);
            bdi._remembered = true;
            bdi._paired = paired;
            if (!string.IsNullOrEmpty(name)) {
#if DEBUG
                BluetoothAddress tmp;
                if (BluetoothAddress.TryParse(name, out tmp)) {
                    Debug.Fail("Name is an address!");
                }
#endif
                bdi.SetName(name);
            }
            bdi._cod = new ClassOfDevice(cod);
            return bdi;
        }

        internal static BluetopiaDeviceInfo CreateFromInquiry(
            Structs.GAP_Inquiry_Entry_Event_Data data, BluetopiaFactory factory)
        {
            var bdi = new BluetopiaDeviceInfo(factory,
                BluetopiaUtils.ToBluetoothAddress(data.BD_ADDR));
            bdi._cod = BluetopiaUtils.ToClassOfDevice(data.Class_of_Device);
            bdi._blockFindNameForNow = true;
            bdi._clockOffsetEtc = new ClockOffsetEtc(data);
            return bdi;
        }

        //----
        internal void SetName(string name)
        {
            _cachedName = name;
            _hasRealName = true;
        }

        internal void SetDiscoDComplete()
        {
            _blockFindNameForNow = false;
            // Since name-lookup causes such problems on this platforms, don't
            // do it unless the user specifically asks for it.
            //var force = DeviceName;
        }

        internal void SetRemembered()
        {
            _remembered = true;
        }

        public void Refresh()
        {
            _cachedName = null;
            _hasRealName = false;
            // TODO re-read paired, connected, etc...
        }

        //----

        public BluetoothAddress DeviceAddress
        {
            get { return _addr; }

        }

        public string DeviceName
        {
            get
            {
                if (_cachedName == null) {
                    // Start async lookup
                    var mayUseCached = true;
                    var mayQueryName = !_blockFindNameForNow;
                    var ret = _factory.QueryName(this, mayUseCached, mayQueryName);
                    var addrString = DeviceAddress.ToString("C", System.Globalization.CultureInfo.InvariantCulture);
                    if (!BluetopiaUtils.IsSuccess(ret)) {
                        // Failed. Don't try again unless the user hits Refresh
                        _cachedName = addrString;
                    } else {
                        // Did FindName know our name and set it on us directly,
                        // or didn't and we're waiting for the lookup so should
                        // return the dummy name for now.
                        if (_cachedName == null) {
                            return addrString;
                        }
                    }
                }
                return _cachedName;
            }
            set { _cachedName = value; _hasRealName = true; }
        }

        public bool HasDeviceName { get { return _hasRealName; } }

        //----
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int Rssi
        {
            get
            {
                try {
                    sbyte rssi;
                    var ret = _factory.Api.BSC_Read_RSSI(_factory.StackId,
                        BluetopiaUtils.BluetoothAddressAsInteger(_addr), out rssi);
                    if (BluetopiaUtils.IsSuccess(ret)) {
                        return rssi;
                    }
                    if (ret != BluetopiaError.DEVICE_NOT_CONNECTED) { //Its the common one
                        Debug.WriteLine("BSC_Read_RSSI error: " + ret);
                    }
                    // And drop out...
                } catch (Exception ex) {
                    Debug.WriteLine("BSC_Read_RSSI exception: " + ex);
                }
                return int.MinValue;
            }
        }

#if PLAY_GETRSSI
        int? IBluetoothDeviceInfo.GetRssi(RssiType type)
        {
            Debug.Assert(type == RssiType.Live || type == RssiType.LiveDoForceConnect || type == RssiType.LiveOrLastGoodValue);
            // TO-DO Bluetopia GetRssi
            return this.Rssi;
        }
#endif

        //----
        public ClassOfDevice ClassOfDevice
        {
            get
            {
                if (_cod == null)
                    return new ClassOfDevice(0);
                return _cod;
            }
        }

        public bool Authenticated
        {
            get { return _paired; }
        }

        public bool Remembered
        {
            get
            {
                if (!_remembered && Authenticated) {
                    Debug.Fail("Authenticated but NOT _remembered huh!.");
                    return true;
                }
                return _remembered;
            }
        }

        public bool Connected
        {
            get { return _connected; }
        }

        //----
        public DateTime LastSeen
        {
            get { return _lastSeen; }
        }

        public DateTime LastUsed
        {
            get { return DateTime.MinValue; }
        }

        //----
        public void Merge(IBluetoothDeviceInfo other)
        {
            _paired = other.Authenticated;
            //Debug.Assert(this._cod.Equals(other.ClassOfDevice), "ClassOfDevice " + this._cod + " <> " + other.ClassOfDevice);
            Debug.Assert(this._connected == other.Connected, "Connected " + this._connected + " <> " + other.Connected);
            Debug.Assert(this._addr == other.DeviceAddress, "DeviceAddress " + this._addr + " <> " + other.DeviceAddress);
            //Debug.Assert(this._cachedName == other.DeviceName, "DeviceName '" + this._cachedName + "' <> '" + other.DeviceName + "'");
            if (this._cachedName == null) {
                this._cachedName = other.DeviceName;
            }
            _remembered = other.Remembered;
        }

        public void SetDiscoveryTime(DateTime dt)
        {
            if (_lastSeen != DateTime.MinValue)
                throw new InvalidOperationException("LastSeen is already set.");
            _lastSeen = dt;
        }

        //----
        public byte[][] GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotSupportedException("Can't get the raw record from this stack.");
        }

        public ServiceRecord[] GetServiceRecords(Guid service)
        {
            return EndGetServiceRecords(
                BeginGetServiceRecords(service, null, null));
        }

        public IAsyncResult BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            if (_sdpQuery == null) {
                _sdpQuery = new BluetopiaSdpQuery(_factory);
            }
            return _sdpQuery.BeginQuery(DeviceAddress, service, false, callback, state);
        }

        public ServiceRecord[] EndGetServiceRecords(IAsyncResult asyncResult)
        {
            if (_sdpQuery == null)
                throw new InvalidOperationException("BeginGetServiceRecords not called");
            try {
                var list = _sdpQuery.EndQuery(asyncResult);
                return list.ToArray();
            } catch (SocketException) {
                const int WSASERVICE_NOT_FOUND = 10108; // ??????
                throw new SocketException(WSASERVICE_NOT_FOUND);
            }
        }

        //----
        public Guid[] InstalledServices
        {
            get { throw new NotSupportedException(); }
        }

        public void SetServiceState(Guid service, bool state, bool throwOnError)
        {
            throw new NotSupportedException();
        }

        public void SetServiceState(Guid service, bool state)
        {
            this.SetServiceState(service, state, false);
        }

        //----
        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            throw new NotImplementedException("GetVersions not currently supported on this stack.");
        }

        public void ShowDialog()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        #region Clock-Offset, Page Scan modes, etc.
        [StructLayout(LayoutKind.Auto)]
        internal struct ClockOffsetEtc
        {
            internal ClockOffsetEtc(Structs.GAP_Inquiry_Entry_Event_Data data)
                : this(data.Page_Scan_Repetition_Mode, data.Page_Scan_Period_Mode,
                    data.Page_Scan_Mode, data.Clock_Offset)
            {
            }

            internal ClockOffsetEtc(byte Page_Scan_Repetition_Mode, byte Page_Scan_Period_Mode,
                byte Page_Scan_Mode, ushort Clock_Offset)
            {
                this.Page_Scan_Repetition_Mode = Page_Scan_Repetition_Mode;
                this.Page_Scan_Period_Mode = Page_Scan_Period_Mode;
                this.Page_Scan_Mode = Page_Scan_Mode;
                this.Clock_Offset = Clock_Offset;
                wasSet = true;
            }

            internal readonly bool wasSet;
            internal readonly byte Page_Scan_Repetition_Mode;
            internal readonly byte Page_Scan_Period_Mode;
            internal readonly byte Page_Scan_Mode;
            internal readonly ushort Clock_Offset;
        }
        #endregion
    }
}

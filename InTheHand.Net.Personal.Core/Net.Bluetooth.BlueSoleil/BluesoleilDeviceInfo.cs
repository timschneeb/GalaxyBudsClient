// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

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
using InTheHand.Win32;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    class BluesoleilDeviceInfo : IBluetoothDeviceInfo
    {
        readonly BluesoleilFactory _factory;
        readonly UInt32 _hDev;
        readonly BluetoothAddress _addr;
        string _cachedName;
        bool _paired, _remembered, _connected;
        ClassOfDevice _cod;
        DateTime _lastSeen;
        Func<Guid, ServiceRecord[]> _dlgtGetServiceRecords;
        RadioVersions _versions;

        private BluesoleilDeviceInfo(UInt32 hDev, BluesoleilFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _factory = factory;
            BtSdkError ret;
            _hDev = hDev;
            //
            byte[] bd_addr = new byte[StackConsts.BTSDK_BDADDR_LEN];
            ret = factory.Api.Btsdk_GetRemoteDeviceAddress(_hDev, bd_addr);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_GetRemoteDeviceAddress");
            if (ret != BtSdkError.OK) { // Not known/present(?).
                return;
            }
            _addr = BluesoleilUtils.ToBluetoothAddress(bd_addr);
            //
            uint cod;
            ret = factory.Api.Btsdk_GetRemoteDeviceClass(_hDev, out cod);
            if (ret != BtSdkError.OK) { // Not known/present(?).
                return;
            }
            _cod = new ClassOfDevice(cod);
            //
            ret = _factory.Api.Btsdk_IsDevicePaired(_hDev, out _paired);
            Debug.Assert(ret == BtSdkError.OK, "Btsdk_IsDevicePaired ret: " + ret);
            //
            _connected = _factory.Api.Btsdk_IsDeviceConnected(_hDev);
            //
            GetInfo(ref _addr);
        }

        void GetInfo(ref BluetoothAddress addr)
        {
            BtSdkError ret;
            var props = new Structs.BtSdkRemoteDevicePropertyStru();
            ret = _factory.Api.Btsdk_GetRemoteDeviceProperty(_hDev, out props);
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_GetRemoteDeviceProperty");
            //
            if ((props.mask & Structs.BtSdkRemoteDevicePropertyStru.Mask.Handle) != 0) {
                Debug.Assert(_hDev == props.dev_hdl, "hDev");
            } else {
                Debug.Fail("Handle unknown?!?");
            }
            if ((props.mask & Structs.BtSdkRemoteDevicePropertyStru.Mask.Address) != 0) {
                addr = BluesoleilUtils.ToBluetoothAddress(props.bd_addr);
            }
            if ((props.mask & Structs.BtSdkRemoteDevicePropertyStru.Mask.Class) != 0) {
                _cod = new ClassOfDevice(props.dev_class);
            }
            if ((props.mask & Structs.BtSdkRemoteDevicePropertyStru.Mask.Name) != 0) {
                Debug.Assert(props.name.Length == StackConsts.BTSDK_DEVNAME_LEN,
                    "props.name.Length: " + props.name.Length + ", BTSDK_DEVNAME_LEN: " + StackConsts.BTSDK_DEVNAME_LEN);
                string name = BluesoleilUtils.FromNameString(props.name);
                _cachedName = name;
            }
            if ((props.mask & Structs.BtSdkRemoteDevicePropertyStru.Mask.LmpInfo) != 0) {
                var fs = (LmpFeatures)BitConverter.ToInt64(props.lmp_info.lmp_feature, 0);
                var v = new RadioVersions(props.lmp_info.lmp_version, props.lmp_info.lmp_subversion,
                    fs, props.lmp_info.manuf_name);
                _versions = v;
            }
            //props.
        }

        //----
        internal static BluesoleilDeviceInfo CreateFromGivenAddress(BluetoothAddress addr, BluesoleilFactory factory)
        {
            UInt32 hDev;
            byte[] bd_addr = BluesoleilUtils.FromBluetoothAddress(addr);
            bool remembered;
            hDev = factory.Api.Btsdk_GetRemoteDeviceHandle(bd_addr);
            if (hDev != StackConsts.BTSDK_INVALID_HANDLE) {
                remembered = true;
            } else {
                // Presumably this means that the device isn't known.
                // Does this add it forever? -- no???
                hDev = factory.Api.Btsdk_AddRemoteDevice(bd_addr);
                if (hDev == StackConsts.BTSDK_INVALID_HANDLE) {
                    BluesoleilUtils.CheckAndThrow(BtSdkError.SDK_UNINIT, "Btsdk_Get/AddRemoteDevice");
                }
                remembered = false;
            }
            var result = new BluesoleilDeviceInfo(hDev, factory);
            result._remembered = remembered;
            Debug.Assert(addr.Equals(result.DeviceAddress), "Address changed in create! was: "
                + addr + ", now: " + result.DeviceAddress);
            return result;
        }

        internal static BluesoleilDeviceInfo CreateFromHandleFromStored(uint hDev, BluesoleilFactory factory)
        {
            var result = new BluesoleilDeviceInfo(hDev, factory);
            result._remembered = true;
            return result;
        }

        internal static BluesoleilDeviceInfo CreateFromHandleFromConnection(uint hDev, BluesoleilFactory factory)
        {
            var result = new BluesoleilDeviceInfo(hDev, factory);
            return result;
        }

        internal static BluesoleilDeviceInfo CreateFromHandleFromInquiry(uint hDev, BluesoleilFactory factory)
        {
            var result = new BluesoleilDeviceInfo(hDev, factory);
            Debug.Assert(result._remembered == false, "NOT !_remembered, we don't set it, does the stack set it?");
            if (result.Authenticated) {
                result._remembered = true;
            }
            return result;
        }

        internal static List<IBluetoothDeviceInfo> CreateFromInquiryHandles(List<uint> discoverableHandles, BluesoleilFactory factory)
        {
            List<IBluetoothDeviceInfo> result = new List<IBluetoothDeviceInfo>(discoverableHandles.Count);
            foreach (var hCur in discoverableHandles) {
                IBluetoothDeviceInfo bdi = CreateFromHandleFromInquiry(hCur, factory);
                result.Add(bdi);
            }
            return result;
        }

        //----

        public void Refresh()
        {
            _cachedName = null;
            // TODO re-read paired, connected, etc...
        }

        public UInt32 Handle
        {
            get
            {
                Debug.Assert(_hDev != 0, "NOT _hDev != 0");
                return _hDev;
            }
        }

        public BluetoothAddress DeviceAddress
        {
            get { return _addr; }
        }

        public string DeviceName
        {
            get
            {
                if (_cachedName == null) {
                    BtSdkError ret;
                    byte[] arr = new byte[500]; // (BTSDK_DEVNAME_LEN = 64)
                    UInt16 len = checked((UInt16)arr.Length);
                    // TODO BluesoleilDeviceInfo: if user called Refresh only use Btsdk_UpdateRemoteDeviceName.
                    ret = _factory.Api.Btsdk_GetRemoteDeviceName(_hDev, arr, ref len);
                    if (ret == BtSdkError.OPERATION_FAILURE) {
                        // Name not known, contact the device to ask.
                        len = checked((UInt16)arr.Length);
                        ret = _factory.Api.Btsdk_UpdateRemoteDeviceName(_hDev, arr, ref len);
                    }
                    if (ret == BtSdkError.OK) {
                        _cachedName = BluesoleilUtils.FromNameString(arr, len);
                    }
                    if (_cachedName == null)
                        _cachedName = DeviceAddress.ToString("C", System.Globalization.CultureInfo.InvariantCulture);
                }
                return _cachedName;
            }
            set { _cachedName = value; }
        }

        public int Rssi
        {
            get
            {
                sbyte rssi;
                BtSdkError ret = _factory.Api.Btsdk_GetRemoteRSSI(_hDev, out rssi);
                if (ret == BtSdkError.OK) return rssi;
                else return int.MinValue;
            }
        }

#if PLAY_GETRSSI
        int? IBluetoothDeviceInfo.GetRssi(RssiType type)
        {
            Debug.Assert(type == RssiType.Live || type == RssiType.LiveDoForceConnect || type == RssiType.LiveOrLastGoodValue);
            // TODO BlueSoleil GetRssi
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
            Debug.Assert(this._cod.Equals(other.ClassOfDevice), "ClassOfDevice " + this._cod + " <> " + other.ClassOfDevice);
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

        RadioVersions IBluetoothDeviceInfo.GetVersions()
        {
            if (_versions == null) {
                throw new InvalidOperationException("Unknown error.");
            }
            return _versions;
        }

        //----
        public byte[][] GetServiceRecordsUnparsed(Guid service)
        {
            throw new NotSupportedException("Can't get the raw record from the Widcomm stack.");
        }

        const int MaxServiceRecordsLookup = 100;

        public ServiceRecord[] GetServiceRecords(Guid service)
        {
            BtSdkError ret;
            var search = new Structs.BtSdkSDPSearchPatternStru[1];
            search[0] = new Structs.BtSdkSDPSearchPatternStru(service);
            UInt32[] recordHandles = new uint[MaxServiceRecordsLookup];
            int num = recordHandles.Length;
            // Fetch the matching records (the handles of).
            int numSearch = search.Length;
            ret = _factory.Api.Btsdk_BrowseRemoteServicesEx(_hDev, search, numSearch, recordHandles, ref num);
            if (ret == BtSdkError.NO_SERVICE) { // None
                return new ServiceRecord[0];
            }
            BluesoleilUtils.CheckAndThrow(ret, "Btsdk_BrowseRemoteServicesEx");
            // Fetch the records' attributes and convert to our format.
            var results = new List<ServiceRecord>();
            for (int i = 0; i < num; ++i) {
                var hRcd = recordHandles[i];
                var attrs = new Structs.BtSdkRemoteServiceAttrStru(
                    StackConsts.AttributeLookup.ServiceName | StackConsts.AttributeLookup.ExtAttributes);
                // Use 'Get' here and not 'Refresh' as the 'Browse' call above
                // should have retrieved the attributes.
                ret = _factory.Api.Btsdk_GetRemoteServiceAttributes(hRcd, ref attrs);
                BluesoleilUtils.CheckAndThrow(ret, "Btsdk_RefreshRemoteServiceAttributes");
                Debug.Assert(attrs.dev_hdl == _hDev);
                //
                IBluesoleilApi hackApi = _factory.Api;
                ServiceRecord sr = CreateServiceRecord(ref attrs, hackApi);
                results.Add(sr);
            }
            return results.ToArray();
        }

        internal static ServiceRecord CreateServiceRecord(ref Structs.BtSdkRemoteServiceAttrStru attrs, IBluesoleilApi api)
        {
            ServiceRecordBuilder bldr = new ServiceRecordBuilder();
            //--
            Guid sc = BluetoothService.CreateBluetoothUuid(attrs.svc_class);
            bldr.AddServiceClass(sc);
            //--
            string name = ParseServiceName(ref attrs);
            if (name.Length != 0)
                bldr.ServiceName = name;
            //
            byte? port = null;
            var extras = new List<ServiceAttribute>();
            Debug.Assert(attrs.status == 0, "attrs.status: 0x" + attrs.status.ToString("X"));
            if (attrs.ext_attributes != IntPtr.Zero) {
                if (sc == BluetoothService.HumanInterfaceDevice) {
                    var hidInfo = (Structs.BtSdkRmtHidSvcExtAttrStru_HACK)Marshal.PtrToStructure(
                        attrs.ext_attributes, typeof(Structs.BtSdkRmtHidSvcExtAttrStru_HACK));
                    Debug.Assert(Marshal.SizeOf(typeof(Structs.BtSdkRmtHidSvcExtAttrStru_HACK))
                        == Marshal.SizeOf(hidInfo), "SizeOf x2");
                    Debug.Assert(hidInfo.size == Marshal.SizeOf(typeof(Structs.BtSdkRmtHidSvcExtAttrStru_HACK))
                        + Structs.BtSdkRmtHidSvcExtAttrStru_HACK.StackMiscountsPaddingSize,
                        "Different sizes!  hidInfo.size: " + hidInfo.size + ", SizeOf(): " + Marshal.SizeOf(typeof(Structs.BtSdkRmtHidSvcExtAttrStru_HACK)));
                    // TO-DO Human Interface (HID) record: Use "mask" field, it's undocumented, check for real life values
                    // With test SdpCreateAHumanInputDeviceRecordsAllTwoOfThree
                    // which adds two out of three of {DeviceReleaseNumber,DeviceSubclass,CountryCode}
                    // mask==0.  So mask apparently applies to other fields!
                    // So we check these three values for zero
                    // and discard them if so!
                    Debug.WriteLine($"HID.mask: {hidInfo.mask:X}");
                    var list = new List<ServiceAttribute>();
                    if (hidInfo.deviceReleaseNumber != 0) list.Add(
                        new ServiceAttribute(HidProfileAttributeId.DeviceReleaseNumber,
                            new ServiceElement(ElementType.UInt16, hidInfo.deviceReleaseNumber)));
                    if (hidInfo.deviceSubclass != 0) list.Add(
                        new ServiceAttribute(HidProfileAttributeId.DeviceSubclass,
                            new ServiceElement(ElementType.UInt8, hidInfo.deviceSubclass)));
                    if (hidInfo.countryCode != 0) list.Add(
                        new ServiceAttribute(HidProfileAttributeId.CountryCode,
                            new ServiceElement(ElementType.UInt8, hidInfo.countryCode)));
                    // TO-DO HID other...
                    extras.AddRange(list);
                } else if (sc == BluetoothService.PnPInformation) {
                    var deviceInfo = (Structs.BtSdkRmtDISvcExtAttrStru)Marshal.PtrToStructure(
                        attrs.ext_attributes, typeof(Structs.BtSdkRmtDISvcExtAttrStru));
                    Debug.Assert(Marshal.SizeOf(typeof(Structs.BtSdkRmtDISvcExtAttrStru))
                        == Marshal.SizeOf(deviceInfo), "SizeOf x2");
                    Debug.Assert(deviceInfo.size == Marshal.SizeOf(typeof(Structs.BtSdkRmtDISvcExtAttrStru))
                        + Structs.BtSdkRmtDISvcExtAttrStru.StackMiscountsPaddingSize,
                        "Different sizes!  deviceInfo.size: " + deviceInfo.size + ", Marshal.SizeOf: " + Marshal.SizeOf(typeof(Structs.BtSdkRmtDISvcExtAttrStru)));
                    // TO-DO Device Info (PnP) record: Use "mask" field, it's undocumented, check for real life values
                    //Debug.Assert(deviceInfo.mask == 0, "Is mask field in BtSdkRmtDISvcExtAttrStru ever set!!!, is here:" + deviceInfo.mask);
                    Debug.WriteLine($"PnP/DI.mask: {deviceInfo.mask:X}");
                    // Like above (PnP) we see mask==0 for the fields we handle
                    // here (six of).  So we check these values
                    // for zero and discard them if so!
                    var list = new List<ServiceAttribute>();
                    if (deviceInfo.spec_id != 0) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.SpecificationId,
                            new ServiceElement(ElementType.UInt16, deviceInfo.spec_id)));
                    if (deviceInfo.vendor_id != 0) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.VendorId,
                            new ServiceElement(ElementType.UInt16, deviceInfo.vendor_id)));
                    if (deviceInfo.product_id != 0) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.ProductId,
                            new ServiceElement(ElementType.UInt16, deviceInfo.product_id)));
                    if (deviceInfo.version != 0) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.Version,
                            new ServiceElement(ElementType.UInt16, deviceInfo.version)));
                    if (true/* Zero means False here!! */) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.PrimaryRecord,
                            new ServiceElement(ElementType.Boolean, deviceInfo.primary_record)));
                    if (deviceInfo.vendor_id_source != 0) list.Add(
                        new ServiceAttribute(DeviceIdProfileAttributeId.VendorIdSource,
                            new ServiceElement(ElementType.UInt16, deviceInfo.vendor_id_source)));
                    // TO-DO URLs...
                    extras.AddRange(list);
                } else {
                    // On testing we see this never working!  For one device
                    // with an ImagingResponder record the size of 0x18 and
                    // not 0x8 as per definition, and the port value is wrong.
                    // And for its PhonebookAccessPse record the size is
                    // correctly 0x8, but again the port value is wrong!
                    // 
                    var sppInfo = (Structs.BtSdkRmtSPPSvcExtAttrStru)Marshal.PtrToStructure(
                        attrs.ext_attributes, typeof(Structs.BtSdkRmtSPPSvcExtAttrStru));
                    Debug.Assert(sppInfo.size == Marshal.SizeOf(typeof(Structs.BtSdkRmtSPPSvcExtAttrStru)),
                        "Different sizes!");
                    port = sppInfo.server_channel;
                }
                api.Btsdk_FreeMemory(attrs.ext_attributes);
            }//if (attrs.ext_attributes != NULL)
            // Use a different API to try and get the RFCOMM port number as
            // the previous API is quite rubbish at doing that!!
            var svcB = new Structs.BtSdkAppExtSPPAttrStru(sc);
            var retSpp = api.Btsdk_SearchAppExtSPPService(attrs.dev_hdl, ref svcB);
            if (retSpp == BtSdkError.NO_SERVICE) { // error
            } else if (retSpp != BtSdkError.OK) { // error
                Debug.WriteLine("GetSvcRcds Btsdk_SearchAppExtSPPService ret: "
                    + BluesoleilUtils.BtSdkErrorToString(retSpp));
            } else { // success
                if (svcB.rf_svr_chnl != 0) {
                    byte newPort = svcB.rf_svr_chnl;
                    if (port.HasValue) {
                        Debug.Assert(port.Value == newPort, "port: " + port.Value + ", newPort: " + newPort);
                    } else {
                        port = newPort;
                    }
                }
                if (svcB.sdp_record_handle != 0) {
                    bldr.AddCustomAttribute(new ServiceAttribute(
                        UniversalAttributeId.ServiceRecordHandle,
                        ServiceElement.CreateNumericalServiceElement(ElementType.UInt32, svcB.sdp_record_handle)));
                }
#if DEBUG
                Debug.Assert(svcB.service_class_128 == sc, "svcSpp.service_class_128: " + svcB.service_class_128 + ", sc: " + sc);
                var snSpp = BluesoleilUtils.FromNameString(svcB.svc_name, StackConsts.BTSDK_SERVICENAME_MAXLENGTH);
                if (snSpp == null) {
                    Debug.Assert(name == null || name.Length == 0, "svcSpp.svc_name: null" + ", name: " + name);
                } else if (snSpp.Length == 1) {
                    // SearchAppExtSPPService doesn't handle Unicode
                    // but Btsdk_BrowseRemoteServicesEx etc does.
                    Debug.Assert(snSpp[0] == name[0], "svcSpp.svc_name: " + snSpp + ", name: " + name);
                } else {
                    Debug.Assert(snSpp == name, "svcSpp.svc_name: " + snSpp + ", bldr.ServiceName: " + name);
                }
#endif
            }
            //
            if (port.HasValue) {
            } else {
                bldr.ProtocolType = BluetoothProtocolDescriptorType.None;
            }
            if (extras.Count != 0) {
                bldr.AddCustomAttributes(extras);
            }
            //
            const ServiceAttributeId FakeDescr = (ServiceAttributeId)(-1);
            bldr.AddCustomAttribute(new ServiceAttribute(FakeDescr,
                new ServiceElement(ElementType.TextString,
                    "<partial BlueSoleil decode>")));
            ServiceRecord sr = bldr.ServiceRecord;
            if (port.HasValue) {
                Debug.Assert(bldr.ProtocolType == BluetoothProtocolDescriptorType.Rfcomm,
                    "type=" + bldr.ProtocolType);
                ServiceRecordHelper.SetRfcommChannelNumber(sr, port.Value);
            } else {
                bldr.ProtocolType = BluetoothProtocolDescriptorType.None;
            }
            return sr;
        }

        private static string ParseServiceName(ref Structs.BtSdkRemoteServiceAttrStru attrs)
        {
            string nameUtf8 = BluesoleilUtils.FixedLengthArrayToStringUtf8(attrs.svc_name);
            string nameUL = Encoding.Unicode.GetString(attrs.svc_name);
            nameUL = nameUL.Trim('\0');
#if DEBUG
            string nameUB = Encoding.BigEndianUnicode.GetString(attrs.svc_name);
            nameUB = nameUB.Trim('\0');
            Debug.Assert(nameUL.Length == nameUB.Length,
                "Our simple check of length doesn't work for Unicode little-endian vs big-endian, the lengths are the same."
                + "  BUT different here!!  nameUL.Length: " + nameUL.Length + ", nameUB.Length: " + nameUB.Length);
#endif
            //
            string name = nameUtf8;
            if (nameUL.Length > name.Length) name = nameUL;
            // As described in DEBUG above, can't do this!!   if (nameUB.Length > name.Length) name = nameUB;
            return name;
        }

        public IAsyncResult BeginGetServiceRecords(Guid service, AsyncCallback callback, object state)
        {
            if (_dlgtGetServiceRecords == null) {
                _dlgtGetServiceRecords = new Func<Guid, ServiceRecord[]>(this.GetServiceRecords);
            }
            return _dlgtGetServiceRecords.BeginInvoke(service, callback, state);
        }

        public ServiceRecord[] EndGetServiceRecords(IAsyncResult asyncResult)
        {
            return _dlgtGetServiceRecords.EndInvoke(asyncResult);
        }

        public Guid[] InstalledServices
        {
            get { throw new NotImplementedException(); }
        }

        public void SetServiceState(Guid service, bool state, bool throwOnError)
        {
            if (throwOnError)
                SetServiceStateDoIt(service, state);
            else {
                try {
                    SetServiceStateDoIt(service, state);
                } catch (SocketException) { }
            }
        }

        public void SetServiceStateDoIt(Guid service, bool state)
        {
            UInt16? classId16 = BluetoothService.GetAsClassId16(service);
            if (!classId16.HasValue)
                throw new ArgumentException("BlueSoleil only supports standard Bluetooth UUID16 services.");
            //
            // MSDN says the posible errors are:
            //   ERROR_INVALID_PARAMETER The dwServiceFlags are invalid. 
            //   ERROR_SERVICE_DOES_NOT_EXIST The GUID specified in pGuidService is not supported. 
            // Numerically:
            //   #define ERROR_FILE_NOT_FOUND             2L
            //   #define ERROR_SERVICE_DOES_NOT_EXIST     1060L
            //   #define ERROR_NOT_FOUND                  1168L
            //
            // Seen:
            // • 0x00000424 = 1060 ----> ERROR_SERVICE_DOES_NOT_EXIST
            // When service not present, or device not present.
            //
            // • 0x80070002        -/\-> ERROR_FILE_NOT_FOUND
            // PANU on Broadcom peer.  "No driver for service"?
            //
            // • 0x00000490 = 1168 ----> ERROR_NOT_FOUND
            // Setting 'False' on a service not set previously registered.
            //--
            BtSdkError ret;
            if (state) {
                UInt32 hConn;
                ret = _factory.Api.Btsdk_ConnectEx(_hDev, classId16.Value, 0, out hConn);
                if (ret != BtSdkError.OK)
                    throw new Win32Exception((int)Win32Error.ERROR_SERVICE_DOES_NOT_EXIST, "Failed to enable the service.");
            } else {
                var hConnList = FindConnection(_hDev, classId16.Value);
                if (hConnList == null || hConnList.Count == 0) {
                    throw new Win32Exception((int)Win32Error.ERROR_NOT_FOUND, "No matching enabled service found.");
                }
                // TO-DO SetServiceState, before disabling, ensure its not BluetoothClient's connection.
                Debug.Assert(hConnList.Count == 1, "SetServiceState: What to do if more than one match?");
                ret = _factory.Api.Btsdk_Disconnect(hConnList[0]);
                if (ret != BtSdkError.OK) {
                    throw new Win32Exception((int)Win32Error.ERROR_SERVICE_DOES_NOT_EXIST, "Failed to disabled the service.");
                }
            }
        }

        public void SetServiceState(Guid service, bool state)
        {
            this.SetServiceState(service, state, false);
        }

        IList<UInt32> FindConnection(UInt32 hDev, UInt16 classId)
        {
            var hConnList = new List<UInt32>();
            var hEnum = _factory.Api.Btsdk_StartEnumConnection();
            if (hEnum == StackConsts.BTSDK_INVALID_HANDLE)
                // BTSDK_INVALID_HANDLE here just means there are zero connections.
                return hConnList;
            while (true) {
                var connProp = new Structs.BtSdkConnectionPropertyStru();
                var hConn = _factory.Api.Btsdk_EnumConnection(hEnum, ref connProp);
                if (hConn == StackConsts.BTSDK_INVALID_HANDLE)
                    break;
                if (connProp.device_handle != hDev) continue;
                if (connProp.service_class != classId) continue;
                if (connProp.Role != StackConsts.BTSDK_CONNROLE.Initiator) continue;
                hConnList.Add(hConn);
            }
            var ret = _factory.Api.Btsdk_EndEnumConnection(hEnum);
            BluesoleilUtils.Assert(ret, "Btsdk_EndEnumConnection");
            return hConnList;
        }

        //----
        public void ShowDialog()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

    }
}

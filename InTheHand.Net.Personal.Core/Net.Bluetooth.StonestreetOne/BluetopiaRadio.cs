// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.StonestreetOne.BluetopiaRadio
// 
// Copyright (c) 2010 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the In The Hand Community License - see License.txt

using System;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;
using System.Text;

namespace InTheHand.Net.Bluetooth.StonestreetOne
{
    class BluetopiaRadio : IBluetoothRadio
    {
        readonly BluetopiaFactory _fcty;
        readonly BluetoothAddress _addr;
        string _name;
        readonly ClassOfDevice _cod;
        //
        bool _readVersions;
        HciVersion _hciVersion = HciVersion.Unknown;
        LmpVersion _lmpVersion;
        LmpFeatures _lmpFeatures;
        ushort _hciRev, _lmpSubver;
        Manufacturer _manuf;

        internal BluetopiaRadio(BluetopiaFactory factory)
        {
            _fcty = factory;
            byte[] bd_addr = new byte[StackConsts.BD_ADDR_SIZE];
            var ret = _fcty.Api.GAP_Query_Local_BD_ADDR(_fcty.StackId, bd_addr);
            BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Local_BD_ADDR");
            _addr = BluetopiaUtils.ToBluetoothAddress(bd_addr);
            //
            ReadName();
            //
            uint cod;
            ret = _fcty.Api.GAP_Query_Class_Of_Device(_fcty.StackId, out cod);
            BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Class_Of_Device");
            _cod = new ClassOfDevice(cod);
        }

        void ReadVersionsOnce()
        {
            if (_readVersions)
                return;
            _readVersions = true; // Just do once even if error
            BluetopiaError ret;
            try {
                StackConsts.HCI_ERROR_CODE hciStatus;
                ret = _fcty.Api.HCI_Read_Local_Version_Information(_fcty.StackId,
                    out hciStatus, out _hciVersion, out _hciRev,
                    out _lmpVersion, out _manuf, out _lmpSubver);
            } catch (MissingMethodException) { // Function added later to the SDK.
                ret = BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
            }
            BluetopiaUtils.Assert(ret, "HCI_Read_Local_Version_Information");
            if (!BluetopiaUtils.IsSuccess(ret)) {
                _hciVersion = HciVersion.Unknown;
                _lmpVersion = LmpVersion.Unknown;
                _manuf = Manufacturer.Unknown;
                _hciRev = _lmpSubver = 0;
            }
            var arr = new byte[8];
            try {
                StackConsts.HCI_ERROR_CODE hciStatus;
                ret = _fcty.Api.HCI_Read_Local_Supported_Features(_fcty.StackId,
                    out hciStatus, arr);
            } catch (MissingMethodException) { // Function added later to the SDK.
                ret = BluetopiaError.UNSUPPORTED_PLATFORM_ERROR;
            }
            BluetopiaUtils.Assert(ret, "HCI_Read_Local_Version_Information");
            if (BluetopiaUtils.IsSuccess(ret)) {
                _lmpFeatures = (LmpFeatures)BitConverter.ToInt64(arr, 0);
            } else {
                _lmpFeatures = LmpFeatures.None;
            }
        }

        private void ReadName()
        {
            byte[] arr = new byte[StackConsts.MAX_NAME_LENGTH];
            var ret = _fcty.Api.GAP_Query_Local_Device_Name(_fcty.StackId,
                arr.Length, arr);
            BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Local_Device_Name");
            _name = BluetopiaUtils.FromNameString(arr);
        }

        //----
        BluetoothAddress IBluetoothRadio.LocalAddress
        {
            get { return _addr; }
        }

        string IBluetoothRadio.Name
        {
            get { return _name; }
            set
            {
                var arr = Encoding.UTF8.GetBytes(value + "\0");
                var ret = NativeMethods.GAP_Set_Local_Device_Name(_fcty.StackId, arr);
                ReadName(); // refresh
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Local_Device_Name");
            }
        }

        RadioModes IBluetoothRadio.Modes
        {
            get
            {
                RadioModes modes = 0;
                modes |= RadioModes.PowerOn;
                //
                StackConsts.GAP_Connectability_Mode connMode;
                BluetopiaError ret = _fcty.Api.GAP_Query_Connectability_Mode(_fcty.StackId, out connMode);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Connectability_Mode");
                if (connMode == StackConsts.GAP_Connectability_Mode.ConnectableMode) {
                    modes |= RadioModes.Connectable;
                }
                StackConsts.GAP_Discoverability_Mode discoMode;
                uint max_Discoverable_Time;
                ret = _fcty.Api.GAP_Query_Discoverability_Mode(_fcty.StackId, out discoMode, out max_Discoverable_Time);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Discoverability_Mode");
                if (discoMode == StackConsts.GAP_Discoverability_Mode.GeneralDiscoverableMode) {
                    modes |= RadioModes.Discoverable;
                }
                return modes;
            }
        }

        public void SetMode(bool? connectable, bool? discoverable)
        {
            if (connectable.HasValue) {
                var ret = _fcty.Api.GAP_Set_Connectability_Mode(_fcty.StackId,
                    connectable.Value ? StackConsts.GAP_Connectability_Mode.ConnectableMode
                                        : StackConsts.GAP_Connectability_Mode.NonConnectableMode);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Connectability_Mode");
            }
            if (discoverable.HasValue) {
                var ret = _fcty.Api.GAP_Set_Discoverability_Mode(_fcty.StackId,
                    discoverable.Value ? StackConsts.GAP_Discoverability_Mode.GeneralDiscoverableMode
                                        : StackConsts.GAP_Discoverability_Mode.NonDiscoverableMode, 0);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Discoverability_Mode");
            }
        }

        RadioMode IBluetoothRadio.Mode
        {
            get
            {
                //var hwStatus = HardwareStatus;
                //if (hwStatus != HardwareStatus.Running) {
                //    return RadioMode.PowerOff;
                //}
                StackConsts.GAP_Connectability_Mode connMode;
                BluetopiaError ret = _fcty.Api.GAP_Query_Connectability_Mode(_fcty.StackId, out connMode);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Connectability_Mode");
                if (connMode == StackConsts.GAP_Connectability_Mode.NonConnectableMode)
                    return RadioMode.PowerOff;
                //
                StackConsts.GAP_Discoverability_Mode discoMode;
                uint max_Discoverable_Time;
                ret = _fcty.Api.GAP_Query_Discoverability_Mode(_fcty.StackId, out discoMode, out max_Discoverable_Time);
                BluetopiaUtils.CheckAndThrow(ret, "GAP_Query_Discoverability_Mode");
                if (discoMode == StackConsts.GAP_Discoverability_Mode.GeneralDiscoverableMode)
                    return RadioMode.Discoverable;
                else
                    return RadioMode.Connectable;
            }
            set
            {
                if (value != ((IBluetoothRadio)this).Mode) {
                    BluetopiaError ret = _fcty.Api.GAP_Set_Connectability_Mode(_fcty.StackId, value != RadioMode.PowerOff ? StackConsts.GAP_Connectability_Mode.ConnectableMode : StackConsts.GAP_Connectability_Mode.NonConnectableMode);
                    BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Connectability_Mode");

                    ret = _fcty.Api.GAP_Set_Discoverability_Mode(_fcty.StackId, value == RadioMode.Discoverable ? StackConsts.GAP_Discoverability_Mode.GeneralDiscoverableMode : StackConsts.GAP_Discoverability_Mode.NonDiscoverableMode, 0);
                    BluetopiaUtils.CheckAndThrow(ret, "GAP_Set_Discoverability_Mode");


                }

                //BluetopiaError ret;
                //if (value == RadioMode.PowerOff) {
                //    ret = _fcty.Api.Btsdk_StopBluetooth();
                //    BluesoleilUtils.Assert(ret, "Radio.set_Mode Stop");
                //} else {
                //    ret = _fcty.Api.Btsdk_StartBluetooth();
                //    BluesoleilUtils.Assert(ret, "Radio.set_Mode Start");
                //    StackConsts.DiscoveryMode dMode;
                //    ret = _fcty.Api.Btsdk_GetDiscoveryMode(out dMode);
                //    BluesoleilUtils.Assert(ret, "Radio.set_Mode Get");
                //    if (ret != BluetopiaError.OK) {
                //        dMode = StackConsts.BTSDK_DISCOVERY_DEFAULT_MODE;
                //    }
                //    // Not PowerOff, so must be Conno, and check if Disco.
                //    dMode |= StackConsts.DiscoveryMode.BTSDK_CONNECTABLE;
                //    if ((value & RadioMode.Discoverable) == RadioMode.Discoverable) {
                //        dMode |= StackConsts.DiscoveryMode.BTSDK_GENERAL_DISCOVERABLE;
                //    } else {
                //        dMode &= ~StackConsts.DiscoveryMode.BTSDK_GENERAL_DISCOVERABLE;
                //    }
                //    ret = _fcty.Api.Btsdk_SetDiscoveryMode(dMode);
                //    BluesoleilUtils.Assert(ret, "Radio.set_Mode Set");
                //}
            }
        }

        ClassOfDevice IBluetoothRadio.ClassOfDevice
        {
            get { return _cod; }
        }

        Manufacturer IBluetoothRadio.SoftwareManufacturer
        {
            get { return Manufacturer.StonestreetOne; }
        }

        IntPtr IBluetoothRadio.Handle
        {
            [DebuggerNonUserCode]
            get { return new IntPtr(_fcty.StackId); }
        }

        string IBluetoothRadio.Remote
        {
            get { return null; }
        }


        HardwareStatus IBluetoothRadio.HardwareStatus
        {
            get { return HardwareStatus.Running; }
        }

        LmpVersion IBluetoothRadio.LmpVersion
        {
            get
            {
                ReadVersionsOnce();
                return _lmpVersion;
            }
        }

        int IBluetoothRadio.LmpSubversion
        {
            get
            {
                ReadVersionsOnce();
                return _lmpSubver;
            }
        }

        LmpFeatures IBluetoothRadio.LmpFeatures
        {
            get
            {
                ReadVersionsOnce();
                return _lmpFeatures;
            }
        }

        HciVersion IBluetoothRadio.HciVersion
        {
            get
            {
                ReadVersionsOnce();
                return _hciVersion;
            }
        }

        int IBluetoothRadio.HciRevision
        {
            get
            {
                ReadVersionsOnce();
                return _hciRev;
            }
        }

        Manufacturer IBluetoothRadio.Manufacturer
        {
            get
            {
                ReadVersionsOnce();
                return _manuf;
            }
        }

    }
}

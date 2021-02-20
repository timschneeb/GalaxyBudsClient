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
using System.Diagnostics;
//
using BtSdkUUIDStru = System.Guid;
using BTDEVHDL = System.UInt32;
using BTSVCHDL = System.UInt32;
using BTCONNHDL = System.UInt32;
using BTSHCHDL = System.UInt32;
using BTSDKHANDLE = System.UInt32;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    class BluesoleilSecurity : IBluetoothSecurity
    {
        readonly BluesoleilFactory _factory;
        readonly object _lock = new object();
        readonly Dictionary<BTDEVHDL, byte[]> _pinResponses = new Dictionary<uint, byte[]>();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Looks like FxCop bug")]
        internal BluesoleilSecurity(BluesoleilFactory fcty)
        {
            _factory = fcty;
        }

        #region IBluetoothSecurity Members

        internal StackConsts.CallbackResult HandlePinReqInd(BTDEVHDL dev_hdl)
        {
            byte[] pin;
            lock (_lock) {
                if (_pinResponses.ContainsKey(dev_hdl)) {
                    pin = _pinResponses[dev_hdl];
                } else {
                    pin = null;
                }
            }//lock
            if (pin != null) {
                ThreadPool.QueueUserWorkItem(PinReply_Runner,
                    new DeviceAndPin { Device = dev_hdl, Pin = pin });
                return StackConsts.CallbackResult.Handled;
            } else {
                return StackConsts.CallbackResult.NotHandled;
            }
        }

        private class DeviceAndPin
        {
            public UInt32 Device { get; set; }
            public byte[] Pin { get; set; }
        }

        void PinReply_Runner(object state)
        {
            var args = (DeviceAndPin)state;
            BtSdkError ret = _factory.Api.Btsdk_PinCodeReply(args.Device,
                args.Pin, (UInt16)args.Pin.Length);
            Debug.Assert(ret == BtSdkError.OK);
        }

        bool IBluetoothSecurity.PairRequest(BluetoothAddress device, string pin)
        {
            //if (pin != null) {
            //    throw new NotImplementedException("Only support pin 'null'/'Nothing' to show the UI dialog.");
            //
            BluesoleilDeviceInfo bdi = BluesoleilDeviceInfo.CreateFromGivenAddress(device, _factory);
            BTDEVHDL hDev = bdi.Handle;
            _factory.RegisterCallbacksOnce();
            try {
                if (pin != null) SetPin(hDev, pin);
                BtSdkError ret = _factory.Api.Btsdk_PairDevice(hDev);
                // TODO Do we have to wait here for completion???
                return (ret == BtSdkError.OK);
            } finally {
                if (pin != null) RevokePin(hDev);
            }
        }

        bool IBluetoothSecurity.RemoveDevice(BluetoothAddress device)
        {
            BluesoleilDeviceInfo bdi = BluesoleilDeviceInfo.CreateFromGivenAddress(device, _factory);
            // TO-DO We don't need to do Btsdk_UnPairDevice also do we?
            BtSdkError ret = _factory.Api.Btsdk_DeleteRemoteDeviceByHandle(bdi.Handle);
            return (ret == BtSdkError.OK);
        }

        //----
        void SetPin(BTDEVHDL hDev, string pin)
        {
            lock (_lock) {
                byte[] pinUtf8 = Encoding.UTF8.GetBytes(pin);
                _pinResponses.Add(hDev, pinUtf8);
            }//lock
        }

        void RevokePin(BTDEVHDL hDev)
        {
            lock (_lock) {
                _pinResponses.Remove(hDev);
            }//lock
        }

        //----
        bool IBluetoothSecurity.SetPin(InTheHand.Net.BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.RevokePin(InTheHand.Net.BluetoothAddress device)
        {
            throw new NotImplementedException();
        }

        //----
        BluetoothAddress IBluetoothSecurity.GetPinRequest()
        {
            throw new NotSupportedException();
        }

        bool IBluetoothSecurity.RefusePinRequest(BluetoothAddress device)
        {
            throw new NotSupportedException();
        }

        bool IBluetoothSecurity.SetLinkKey(BluetoothAddress device, Guid linkKey)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

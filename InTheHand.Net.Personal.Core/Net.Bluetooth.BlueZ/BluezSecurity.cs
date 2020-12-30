// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.BlueZ.BluezSecurity
// 
// Copyright (c) 2008-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

#if BlueZ

using System;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.BlueZ
{
    class BluezSecurity : IBluetoothSecurity
    {
        readonly BluezFactory _fcty;

        //----
        internal BluezSecurity(BluezFactory fcty)
        {
            _fcty = fcty;
        }

        //----
        bool IBluetoothSecurity.PairRequest(BluetoothAddress device, string pin)
        {
            var bus = _fcty.BluezDbus;
            return bus.PairRequest_OnDefaultAdapter(device, pin);
        }

        bool IBluetoothSecurity.RemoveDevice(BluetoothAddress device)
        {
            var bus = _fcty.BluezDbus;
            return bus.RemoveDeviceFind_OnDefaultAdapter(device);
        }

        #region Unused IBluetoothSecurity Members
        bool IBluetoothSecurity.SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.RevokePin(BluetoothAddress device)
        {
            throw new NotImplementedException();
        }

        BluetoothAddress IBluetoothSecurity.GetPinRequest()
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.RefusePinRequest(BluetoothAddress device)
        {
            throw new NotImplementedException();
        }

        bool IBluetoothSecurity.SetLinkKey(BluetoothAddress device, Guid linkKey)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
#endif
// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSocketExceptions
// 
// Copyright (c) 2009-2010 In The Hand Ltd, All rights reserved.
// Copyright (c) 2009-2010 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth.Widcomm
{
    sealed class WidcommBluetoothSecurity : IBluetoothSecurity
    {
        readonly WidcommBluetoothFactoryBase _factory;

        internal WidcommBluetoothSecurity(WidcommBluetoothFactoryBase factory)
        {
            Debug.Assert(factory != null);
            _factory = factory;
        }

        #region Pair/Un-pair
        public bool PairRequest(BluetoothAddress device, string pin)
        {
            // Win32+MSFT supports pin=null to show the UI, Widcomm has no 
            // support for that, so copy what we do in CE/WM and fail here.
            if (pin == null) {
                throw new ArgumentNullException("pin");
            }
            BOND_RETURN_CODE ret = Bond_(device, pin);
            Debug.Assert((ret == BOND_RETURN_CODE.SUCCESS)
                    || (ret == BOND_RETURN_CODE.ALREADY_BONDED)
                    || (ret == BOND_RETURN_CODE.FAIL),
                "Unexpected Bond result: " + ret);
            return (ret == BOND_RETURN_CODE.SUCCESS)
                    || (ret == BOND_RETURN_CODE.ALREADY_BONDED);
        }

        internal BOND_RETURN_CODE Bond_(BluetoothAddress device, string pin)
        {
            Utils.MiscUtils.Trace_WriteLine("Calling CBtIf:Bond...");
            BOND_RETURN_CODE ret = _factory.GetWidcommBtInterface().Bond(device, pin);
            Utils.MiscUtils.Trace_WriteLine("Bond returned: {0} = 0x{1:X}", ret, (int)ret);
            return ret;
        }

        public bool RemoveDevice(BluetoothAddress device)
        {
            bool ret = _factory.GetWidcommBtInterface().UnBond(device);
            bool doReallyDeleteDevice = true;
            if (doReallyDeleteDevice) {
                bool retD = WidcommBtInterface.DeleteKnownDevice(device);
                return retD;
            }
            return ret;
        }
        #endregion

        #region SavePin/forget
        public bool SetPin(BluetoothAddress device, string pin)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public bool RevokePin(BluetoothAddress device)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion

        #region PinRequest/etc
        public BluetoothAddress GetPinRequest()
        {
            throw new NotSupportedException();
        }

        public bool RefusePinRequest(BluetoothAddress device)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region SetLinkKey
        public bool SetLinkKey(BluetoothAddress a, Guid linkkey)
        {
            throw new NotSupportedException();
        }
        #endregion

    }
}

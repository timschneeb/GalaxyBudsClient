// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Msft.SocketsBluetoothFactory
// 
// Copyright (c) 2003-2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2003-2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth.Factory;
using InTheHand.Net.Bluetooth.Msft;

// Don't ever change the namespace.  Users' app.config files could be
// referencing the factory by full name.
namespace InTheHand.Net.Bluetooth
{
    class SocketsBluetoothFactory : BluetoothFactory
    {
        static WindowsBluetoothSecurity m_btSecurity;

        public SocketsBluetoothFactory()
        {
            bool supp = WindowsBluetoothRadio.IsPlatformSupported;
            if (!supp)
                throw new PlatformNotSupportedException("Microsoft Bluetooth stack not supported (radio).");
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override IBluetoothClient GetBluetoothClient()
        {
            return new SocketBluetoothClient(this);
        }
        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            return new SocketBluetoothClient(this, acceptedSocket);
        }
        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStream)
        {
            throw  new NotSupportedException();
        }
        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            return new SocketBluetoothClient(this, localEP);
        }

        protected override IBluetoothListener GetBluetoothListener()
        {
            return new WindowsBluetoothListener(this);
        }

        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            return new WindowsBluetoothDeviceInfo(address);
        }

        protected override IBluetoothRadio GetPrimaryRadio()
        {
            return WindowsBluetoothRadio.GetPrimaryRadio();
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            return WindowsBluetoothRadio.AllRadios;
        }

        //----------------
        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            if (m_btSecurity == null) {
                m_btSecurity = new WindowsBluetoothSecurity();
            }
            return m_btSecurity;
        }

    }
}

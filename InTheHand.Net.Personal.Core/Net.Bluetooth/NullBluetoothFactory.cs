// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.NullBluetoothFactory
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth.Factory;

namespace InTheHand.Net.Bluetooth
{
    internal class NullBluetoothFactory : BluetoothFactory
    {
#if !NETCF
        internal NullBtListener TheBtLsnr { get; private set; }
#endif

        internal NullBluetoothFactory()
        {
            TestUtilities.IsUnderTestHarness();
        }

        //----
        protected override void Dispose(bool disposing)
        {
        }

        protected override IBluetoothRadio GetPrimaryRadio()
        {
            return new NullRadio();
        }

        protected override IBluetoothRadio[] GetAllRadios()
        {
            return new IBluetoothRadio[] { GetPrimaryRadio() };
        }

        protected override IBluetoothClient GetBluetoothClient()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        protected override IBluetoothClient GetBluetoothClient(System.Net.Sockets.Socket acceptedSocket)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        protected override IBluetoothClient GetBluetoothClientForListener(CommonRfcommStream acceptedStream)
        {
#if NETCF
            throw new NotImplementedException("The method or operation is not implemented.");
#else
            return new NullBtCli(this, acceptedStream);
#endif
        }

        protected override IBluetoothClient GetBluetoothClient(BluetoothEndPoint localEP)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        protected override IBluetoothDeviceInfo GetBluetoothDeviceInfo(BluetoothAddress address)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        protected override IBluetoothListener GetBluetoothListener()
        {
#if NETCF
            throw new NotImplementedException("The method or operation is not implemented.");
#else
            TheBtLsnr = new NullBtListener(this);
            return TheBtLsnr;
#endif
        }

        protected override IBluetoothSecurity GetBluetoothSecurity()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        //--
        internal class NullRadio : IBluetoothRadio
        {
            #region IBluetoothRadio Members
            public string Remote { get { return null; } }

            public virtual ClassOfDevice ClassOfDevice
            {
                get { return new ClassOfDevice(0xFF2000 | 0x80c); } // All Services + Device=ToyController
            }

            public virtual IntPtr Handle
            {
                get { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public virtual HardwareStatus HardwareStatus
            {
                get { return HardwareStatus.Shutdown; }
            }

            public virtual int LmpSubversion
            {
                get { return 99; }
            }

            LmpFeatures IBluetoothRadio.LmpFeatures { get { return LmpFeatures.None; } }

            LmpVersion IBluetoothRadio.LmpVersion
            {
                get { return LmpVersion.Unknown; }
            }

            public virtual int HciRevision
            {
                get { return 99; }
            }

            HciVersion IBluetoothRadio.HciVersion
            {
                get { return HciVersion.Unknown; }
            }

            public virtual BluetoothAddress LocalAddress
            {
                get { return BluetoothAddress.Parse("00:11:22:33:44:55"); }
            }

            public virtual Manufacturer Manufacturer
            {
                get { return Manufacturer.AccelSemiconductor; }
            }

            public virtual RadioModes Modes
            {
                get { return RadioModes.PowerOff; }
            }

            public virtual void SetMode(bool? connectable, bool? discoverable)
            {
                throw new NotImplementedException();
            }

            public virtual RadioMode Mode
            {
                get { return RadioMode.PowerOff; }
                set { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public virtual string Name
            {
                get { return "NullRadio"; }
                set { throw new NotImplementedException("The method or operation is not implemented."); }
            }

            public virtual Manufacturer SoftwareManufacturer
            {
                get { return Manufacturer.AccelSemiconductor; }
            }

            #endregion
        }
    }
}

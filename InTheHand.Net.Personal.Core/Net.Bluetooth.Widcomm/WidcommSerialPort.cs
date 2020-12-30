// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Widcomm.WidcommSerialPort
// 
// Copyright (c) 2011 In The Hand Ltd, All rights reserved.
// Copyright (c) 2011 Alan J. McFarlane, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Ports;

namespace InTheHand.Net.Bluetooth.Widcomm
{
#pragma warning disable 1591 // "Missing XML comment for publicly visible type or member..."
    public abstract class WidcommSerialPort : IDisposable //, ITmpBluetoothSerialPort
    {
        public static WidcommSerialPort CreateClient(BluetoothAddress device)
        {
            var sp = CreateClient2(device);
            return sp;
        }

        private static WidcommSerialPort CreateClient2(BluetoothAddress device)
        {
            var fcty = WidcommBluetoothFactory.GetWidcommIfExists();
            var sppCli = new WidcommSppClient(fcty);
            sppCli.CreatePort(device);
            return sppCli;
        }


        #region ITmpBluetoothSerialPort Members

        public abstract string PortName { get; }

        public abstract BluetoothAddress Address { get; }

        public abstract Guid Service { get; }

        public void Close()
        {
            Dispose();
        }
        #endregion

        #region Events
        protected void OnPortStatusChanged(object server, PortStatusChangedEventArgs e)
        {
            StatusChanged(server, e);
        }

        // Never null. :-)
        private
            //public 
            event EventHandler<PortStatusChangedEventArgs> StatusChanged = delegate { };
        #endregion

        #region Disposing
        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}

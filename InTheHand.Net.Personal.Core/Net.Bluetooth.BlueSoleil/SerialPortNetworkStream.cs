// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.Widcomm.WidcommBluetoothFactoryBase
// 
// Copyright (c) 2010 Alan J McFarlane, All rights reserved.
// Copyright (c) 2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using System.Diagnostics;
using System.IO.Ports;
using InTheHand.Net.Bluetooth.Factory;
using System.Diagnostics.CodeAnalysis;


namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    abstract class SerialPortNetworkStream : InTheHand.Net.Bluetooth.Factory.DecoratorNetworkStream
    {
        readonly protected ISerialPortWrapper _port;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SerialPortNetworkStream(SerialPort port,
            IBluetoothClient cli)
            : this(new SerialPortWrapper(port), cli)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cli")]
        internal SerialPortNetworkStream(ISerialPortWrapper port,
            IBluetoothClient cli)
            : base(port.BaseStream)
        {
            _port = port;
        }

        public override bool DataAvailable
        {
            get { return _port.BytesToRead > 0; }
        }

        internal int Available
        {
            get { return _port.BytesToRead; }
        }

        /// <summary>
        /// For FooBarClient.Connected
        /// </summary>
        internal abstract bool Connected { get; }

        //----
        public override void Flush()
        {
            try {
                base.Flush();
            } catch (ObjectDisposedException) {
            }
        }

        //----
        ~SerialPortNetworkStream()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing) {
                    _port.Close();
                }
            } finally {
                base.Dispose(disposing);
            }
        }

    }//class
}
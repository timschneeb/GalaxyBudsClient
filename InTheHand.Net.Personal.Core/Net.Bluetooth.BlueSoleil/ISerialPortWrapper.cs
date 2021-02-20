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
using System.ComponentModel;
using System.IO;


namespace InTheHand.Net.Bluetooth.BlueSoleil
{
    interface ISerialPortWrapper
    {
        // Events
        [MonitoringDescription("SerialDataReceived")]
        event SerialDataReceivedEventHandler DataReceived;
        //[MonitoringDescription("SerialErrorReceived")]
        //event SerialErrorReceivedEventHandler ErrorReceived;
        //[MonitoringDescription("SerialPinChanged")]
        //event SerialPinChangedEventHandler PinChanged;

        //// Methods
        //SerialPort();
        //SerialPort(IContainer container);
        //SerialPort(string portName);
        //SerialPort(string portName, int baudRate);
        //SerialPort(string portName, int baudRate, Parity parity);
        //SerialPort(string portName, int baudRate, Parity parity, int dataBits);
        //SerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits);
        //
        void Close();
        //void DiscardInBuffer();
        //void DiscardOutBuffer();
        //protected override void Dispose(bool disposing);
        //static string[] GetPortNames();
        void Open();
        //int Read(byte[] buffer, int offset, int count);
        //int Read(char[] buffer, int offset, int count);
        //int ReadByte();
        //int ReadChar();
        //string ReadExisting();
        //string ReadLine();
        //string ReadTo(string value);
        //void Write(string text);
        //void Write(byte[] buffer, int offset, int count);
        //void Write(char[] buffer, int offset, int count);
        //void WriteLine(string text);

        //// Properties
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        Stream BaseStream { get; }
        //[MonitoringDescription("BaudRate"), Browsable(true), DefaultValue(0x2580)]
        //int BaudRate { get; set; }
        //[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //bool BreakState { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        int BytesToRead { get; }
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        //int BytesToWrite { get; }
        //[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //bool CDHolding { get; }
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        //bool CtsHolding { get; }
        //[MonitoringDescription("DataBits"), DefaultValue(8), Browsable(true)]
        //int DataBits { get; set; }
        //[MonitoringDescription("DiscardNull"), Browsable(true), DefaultValue(false)]
        //bool DiscardNull { get; set; }
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        //bool DsrHolding { get; }
        //[MonitoringDescription("DtrEnable"), DefaultValue(false), Browsable(true)]
        //bool DtrEnable { get; set; }
        //[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), MonitoringDescription("Encoding")]
        //Encoding Encoding { get; set; }
        [Browsable(true), DefaultValue(0), MonitoringDescription("Handshake")]
        Handshake Handshake { get; set; }
        //[Browsable(false)]
        //bool IsOpen { get; }
        //[MonitoringDescription("NewLine"), Browsable(false), DefaultValue("\n")]
        //string NewLine { get; set; }
        //[MonitoringDescription("Parity"), Browsable(true), DefaultValue(0)]
        //Parity Parity { get; set; }
        //[Browsable(true), MonitoringDescription("ParityReplace"), DefaultValue((byte) 0x3f)]
        //byte ParityReplace { get; set; }
        [MonitoringDescription("PortName"), Browsable(true), DefaultValue("COM1")]
        string PortName { get; set; }
        [Browsable(true), MonitoringDescription("ReadBufferSize"), DefaultValue(0x1000)]
        int ReadBufferSize { get; set; }
        //[Browsable(true), MonitoringDescription("ReadTimeout"), DefaultValue(-1)]
        //int ReadTimeout { get; set; }
        //[MonitoringDescription("ReceivedBytesThreshold"), Browsable(true), DefaultValue(1)]
        //int ReceivedBytesThreshold { get; set; }
        //[Browsable(true), MonitoringDescription("RtsEnable"), DefaultValue(false)]
        //bool RtsEnable { get; set; }
        //[DefaultValue(1), MonitoringDescription("StopBits"), Browsable(true)]
        //StopBits StopBits { get; set; }
        [Browsable(true), DefaultValue(0x800), MonitoringDescription("WriteBufferSize")]
        int WriteBufferSize { get; set; }
        //[DefaultValue(-1), MonitoringDescription("WriteTimeout"), Browsable(true)]
        //public int WriteTimeout { get; set; }
    }

    class SerialPortWrapper : ISerialPortWrapper
    {
        readonly SerialPort _child;

        internal SerialPortWrapper(SerialPort port)
        {
            _child = port;
        }

        //--------
        #region ISerialPortWrapper Members

        void ISerialPortWrapper.Close()
        {
            _child.Close();
        }

        void ISerialPortWrapper.Open()
        {
            _child.Open();
        }

        Stream ISerialPortWrapper.BaseStream
        {
            get { return _child.BaseStream; }
        }

        int ISerialPortWrapper.BytesToRead
        {
            get { return _child.BytesToRead; }
        }

        Handshake ISerialPortWrapper.Handshake
        {
            get { return _child.Handshake; }
            set { _child.Handshake = value; }
        }

        string ISerialPortWrapper.PortName
        {
            get { return _child.PortName; }
            set { _child.PortName = value; }
        }

        int ISerialPortWrapper.ReadBufferSize
        {
            get { return _child.ReadBufferSize; }
            set { _child.ReadBufferSize = value; }
        }

        int ISerialPortWrapper.WriteBufferSize
        {
            get { return _child.WriteBufferSize; }
            set { _child.WriteBufferSize = value; }
        }

        event SerialDataReceivedEventHandler ISerialPortWrapper.DataReceived
        {
            add { _child.DataReceived += value; }
            remove { _child.DataReceived -= value; }
        }

        #endregion
    }


}
using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth.Widcomm
{
#pragma warning disable 1591 // "Missing XML comment for publicly visible type or member..."
    [Obsolete("Prelimary, will change.")]
    public interface ITmpBluetoothSerialPort : IDisposable
    {
        //public static BluetoothSerialPort CreateServer(string portName, Guid service);
        //public static BluetoothSerialPort CreateServer(Guid service);
        //public static BluetoothSerialPort CreateClient(string portName, BluetoothEndPoint endPoint);
        //public static BluetoothSerialPort CreateClient(BluetoothEndPoint endPoint);
        //public static BluetoothSerialPort FromHandle(IntPtr handle);
        //
        string PortName { get; }
        BluetoothAddress Address { get; }
        Guid Service { get; }
        // Yeh_But. IntPtr Handle { get; }
        //
        void Close();
        //protected virtual void Dispose(bool disposing);
        //void Dispose();
        //~BluetoothSerialPort();
    }
}

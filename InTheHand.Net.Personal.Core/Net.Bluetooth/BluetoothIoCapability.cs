using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// The BLUETOOTH_IO_CAPABILITY enumeration defines the input/output capabilities of a Bluetooth Device.
    /// </summary>
    public enum BluetoothIoCapability : int // MSFT+Win32 BLUETOOTH_IO_CAPABILITY
    {
        /// <summary>
        /// The Bluetooth device is capable of output via display only.
        /// </summary>
        DisplayOnly = 0x00,
        /// <summary>
        /// The Bluetooth device is capable of output via a display, 
        /// and has the additional capability to presenting a yes/no question to the user.
        /// </summary>
        DisplayYesNo = 0x01,
        /// <summary>
        /// The Bluetooth device is capable of input via keyboard.
        /// </summary>
        KeyboardOnly = 0x02,
        /// <summary>
        /// The Bluetooth device is not capable of input/output.
        /// </summary>
        NoInputNoOutput = 0x03,
        /// <summary>
        /// The input/output capabilities for the Bluetooth device are undefined.
        /// </summary>
        Undefined = 0xff
    }
}

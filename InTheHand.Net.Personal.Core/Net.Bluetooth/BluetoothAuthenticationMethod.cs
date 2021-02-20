using System;
using System.Collections.Generic;
using System.Text;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// The BluetoothAuthenticationMethod enumeration defines the supported
    /// authentication types during device pairing.
    /// </summary>
    public enum BluetoothAuthenticationMethod : int // MSFT+Win32 BLUETOOTH_AUTHENTICATION_METHOD
    {
        /// <summary>
        /// The Bluetooth device supports authentication via a PIN.
        /// </summary>
        Legacy = 0x1,
        /// <summary>
        /// The Bluetooth device supports authentication via out-of-band data.
        /// </summary>
        OutOfBand,
        /// <summary>
        /// The Bluetooth device supports authentication via numeric comparison.
        /// </summary>
        NumericComparison,
        /// <summary>
        /// The Bluetooth device supports authentication via passkey notification.
        /// </summary>
        PasskeyNotification,
        /// <summary>
        /// The Bluetooth device supports authentication via passkey.
        /// </summary>
        Passkey,
    }
}

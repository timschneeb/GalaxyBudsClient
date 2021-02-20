// 32feet.NET - Personal Area Networking for .NET
//
// InTheHand.Net.Bluetooth.RadioMode
// 
// Copyright (c) 2003-2010 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

namespace InTheHand.Net.Bluetooth
{
	/// <summary>
	/// Determine all the possible modes of operation of the Bluetooth radio.
	/// </summary>
    /// -
    /// <remarks>See <see cref="P:InTheHand.Net.Bluetooth.BluetoothRadio.Mode">BluetoothRadio.Mode</see>
    /// for what is supported on what platforms.  For instance setting the mode
    /// is not supported on Widcomm+Win32.  On Widcomm WM/CE setting <c>PowerOff</c>
    /// actually sets 'CONNECT_ALLOW_NONE', and not actually disabled/off.
    /// Also when the stack is disabled, setting connectable/discoverable 
    /// does not manage to turn the radio on.
    /// </remarks>
    /// -
    /// <seealso cref="P:InTheHand.Net.Bluetooth.BluetoothRadio.Mode">BluetoothRadio.Mode</seealso>
	public enum RadioMode
	{
		/// <summary>
		/// Bluetooth is disabled on the device.
		/// </summary>
		PowerOff,
		/// <summary>
		/// Bluetooth is connectable but your device cannot be discovered by other devices.
		/// </summary>
		Connectable,
		/// <summary>
		/// Bluetooth is activated and fully discoverable.
		/// </summary>
		Discoverable,
	}
}

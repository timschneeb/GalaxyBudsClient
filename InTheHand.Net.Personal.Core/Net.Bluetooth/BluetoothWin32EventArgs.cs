// 32feet.NET - Personal Area Networking for .NET
//
// Net.Bluetooth.BluetoothWin32Radio**EventArgs
// 
// Copyright (c) 2008-2011 Alan J.McFarlane, All rights reserved.
// Copyright (c) 2008-2011 In The Hand Ltd, All rights reserved.
// This source code is licensed under the MIT License

using System;
using InTheHand.Net.Bluetooth.Msft;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// The base class for classes containing Radio In- and Out-of-Range events.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Supported only by the Microsoft stack on desktop Windows.
    /// </para>
    /// <para>Produced by class <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Events"/>.
    /// </para>
    /// </remarks>
    public abstract class BluetoothWin32RadioEventArgs : EventArgs
    {
        readonly WindowsBluetoothDeviceInfo _bdiWin;
        readonly BluetoothDeviceInfo _bdi;

        //--------
        internal /*and protected*/ BluetoothWin32RadioEventArgs(BLUETOOTH_DEVICE_INFO bdi0)
        {
            _bdiWin = new WindowsBluetoothDeviceInfo(bdi0);
            _bdi = new BluetoothDeviceInfo(_bdiWin);
        }

        //----
        /// <summary>
        /// Gets the device to which the event applies.
        /// </summary>
        public BluetoothDeviceInfo Device
        {
            get { return _bdi; }
        }

        internal WindowsBluetoothDeviceInfo DeviceWindows
        {
            get { return _bdiWin; }
        }

    }


    /// <summary>
    /// The data for Radio Out-of-Range event.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Supported only by the Microsoft stack on desktop Windows.
    /// </para>
    /// <para>Produced by class <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Events"/>.
    /// </para>
    /// </remarks>
    public sealed class BluetoothWin32RadioOutOfRangeEventArgs : BluetoothWin32RadioEventArgs
    {
        private BluetoothWin32RadioOutOfRangeEventArgs(BLUETOOTH_DEVICE_INFO bdi0)
            : base(bdi0)
        {
        }

        //----
        public static BluetoothWin32RadioOutOfRangeEventArgs Create(long addrLong)
        {
            BLUETOOTH_DEVICE_INFO bdi0 = new BLUETOOTH_DEVICE_INFO(addrLong);
            return new BluetoothWin32RadioOutOfRangeEventArgs(bdi0);
        }

        //----
        /// <summary>
        /// Gets a string representation of the event.
        /// </summary>
        /// <returns>A string (e.g. contains the device address and name).</returns>
        public override string ToString()
        {
            string s = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Device: {0} '{1}'",
                Device.DeviceAddress, Device.DeviceName);
            return s;
        }

    }


    /// <summary>
    /// The data for Radio Out-of-Range event.
    /// </summary>
    /// -
    /// <remarks>
    /// <para>Supported only by the Microsoft stack on desktop Windows.
    /// </para>
    /// <para>Produced by class <see cref="T:InTheHand.Net.Bluetooth.BluetoothWin32Events"/>.
    /// </para>
    /// </remarks>
    public sealed class BluetoothWin32RadioInRangeEventArgs : BluetoothWin32RadioEventArgs
    {
        readonly BluetoothDeviceInfoProperties _currentFlags, _previousFlags;

        //----
        private BluetoothWin32RadioInRangeEventArgs(BLUETOOTH_DEVICE_INFO bdi0, BluetoothDeviceInfoProperties currentFlags, BluetoothDeviceInfoProperties previousFlags)
            :base(bdi0)
        {
            _currentFlags = currentFlags;
            _previousFlags = previousFlags;
        }

        //----
        [CLSCompliant(false)]
        public static BluetoothWin32RadioInRangeEventArgs Create(
            BluetoothDeviceInfoProperties previousFlags,
            BluetoothDeviceInfoProperties currentFlags,
            BLUETOOTH_DEVICE_INFO deviceInfo)
        {
            var e = new BluetoothWin32RadioInRangeEventArgs(deviceInfo, currentFlags, previousFlags);
            return e;
        }

        //----
        /// <summary>
        /// The current state of the device according to the Bluetooth stack.
        /// </summary>
        public BluetoothDeviceInfoProperties CurrentState
        {
            get { return _currentFlags; }
        }

        /// <summary>
        /// The previous state of the device according to the Bluetooth stack.
        /// </summary>
        public BluetoothDeviceInfoProperties PreviousState
        {
            get { return _previousFlags; }
        }

        BluetoothDeviceInfoProperties DifferentStates
        {
            get { return _currentFlags ^ _previousFlags; }
        }

        /// <summary>
        /// The flags that are set in the current state
        /// and weren't in the previous state (calculated).
        /// </summary>
        public BluetoothDeviceInfoProperties GainedStates
        {
            get { return DifferentStates & _currentFlags; }
        }

        /// <summary>
        /// The flags that are not set in the current state
        /// but were in the previous state (calculated).
        /// </summary>
        public BluetoothDeviceInfoProperties LostStates
        {
            get { return DifferentStates & _previousFlags; }
        }

        //----
        /// <summary>
        /// Gets a string representation of the event.
        /// </summary>
        /// <returns>A string (e.g. contains the device address, name and the current and previous flags).</returns>
        public override string ToString()
        {
            string s = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Device: {0} '{1}', cur flags: '{2}', old: '{3}'",
                Device.DeviceAddress, Device.DeviceName,
                _currentFlags, _previousFlags);
            return s;
        }

    }
}

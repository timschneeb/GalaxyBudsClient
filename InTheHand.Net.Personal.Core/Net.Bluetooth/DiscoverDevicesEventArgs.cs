using System;
using System.ComponentModel;
using InTheHand.Net.Sockets;

namespace InTheHand.Net.Bluetooth
{
    /// <summary>
    /// Provides data for the <see cref="E:InTheHand.Net.Bluetooth.BluetoothComponent.DiscoverDevicesComplete"/>
    /// event.
    /// </summary>
    public sealed class DiscoverDevicesEventArgs : AsyncCompletedEventArgs
    {
        BluetoothDeviceInfo[] m_devices;

        /// <summary>
        /// Initialise a new instance.
        /// </summary>
        /// -
        /// <param name="devices">The result, may be empty but not null.
        /// </param>
        /// <param name="userState">Any user state object.
        /// </param>
        public DiscoverDevicesEventArgs(BluetoothDeviceInfo[] devices, object userState)
            : base(null, false, userState)
        {
            if (devices == null) throw new ArgumentNullException("devices");
            // (Can be zero devices at completion event).
            m_devices = devices;
        }

        /// <summary>
        /// Initialise a new instance.
        /// </summary>
        /// -
        /// <param name="exception">The resultant error.
        /// </param>
        /// <param name="userState">Any user state object.
        /// </param>
        public DiscoverDevicesEventArgs(Exception exception, object userState)
            : base(exception, false, userState)
        {
        }

        /// <summary>
        /// Gets the list of discovered Bluetooth devices.
        /// </summary>
        public BluetoothDeviceInfo[] Devices
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return m_devices;
            }
        }

    }


}

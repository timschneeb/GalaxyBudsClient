using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public class DiscoDevsParams
    {
#pragma warning disable 1591
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly DateTime discoTime;
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly int maxDevices;
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public readonly bool authenticated, remembered, unknown, discoverableOnly;

        public DiscoDevsParams(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            DateTime discoTime)
        {
            this.maxDevices = maxDevices;
            //
            this.authenticated = authenticated;
            this.remembered = remembered;
            this.unknown = unknown;
            this.discoverableOnly = discoverableOnly;
            //
            this.discoTime = discoTime;
        }

        //----
        public static List<IBluetoothDeviceInfo> DiscoverDevicesMerge(
            bool authenticated, bool remembered, bool unknown,
            List<IBluetoothDeviceInfo> knownDevices,
            List<IBluetoothDeviceInfo> discoverableDevices,
            bool discoverableOnly, DateTime discoTime)
        {
            return InTheHand.Net.Sockets.BluetoothClient.DiscoverDevicesMerge(
                authenticated, remembered, unknown,
            knownDevices, discoverableDevices,
            discoverableOnly, discoTime);
        }
    }

}

using System;
using System.Diagnostics.CodeAnalysis;
namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public interface IBluetoothDeviceInfo
    {
#pragma warning disable 1591
        /// <summary>
        /// Update the instance with value from the other.
        /// </summary>
        /// -
        /// <param name="other">The other device, to read properties from.
        /// </param>
        /// -
        /// <remarks>
        /// <para>Used by the device discovery code in merging the devices 
        /// found by Inquiry and the 'remembered' devices. The current
        /// device is the one found by inqury and the <paramref name="other"/>
        /// device is a 'remembered' one. So its common to update the
        /// 'Remembered' and 'Authenticated' properties for instance.
        /// </para>
        /// </remarks>
        void Merge(IBluetoothDeviceInfo other);
        void SetDiscoveryTime(DateTime dt);
        //
        bool Authenticated { get; }
        InTheHand.Net.Bluetooth.ClassOfDevice ClassOfDevice { get; }
        bool Connected { get; }
        BluetoothAddress DeviceAddress { get; }
        string DeviceName { get; set; }
        byte[][] GetServiceRecordsUnparsed(Guid service);
        InTheHand.Net.Bluetooth.ServiceRecord[] GetServiceRecords(Guid service);
#if !V1
        IAsyncResult BeginGetServiceRecords(Guid service, AsyncCallback callback, object state);
        InTheHand.Net.Bluetooth.ServiceRecord[] EndGetServiceRecords(IAsyncResult asyncResult);
#endif
        Guid[] InstalledServices { get; }
        DateTime LastSeen { get; }
        DateTime LastUsed { get; }
        void Refresh();
        bool Remembered { get; }
        int Rssi { get;}
        void SetServiceState(Guid service, bool state, bool throwOnError);
        void SetServiceState(Guid service, bool state);
        void ShowDialog();
        void Update();
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        RadioVersions GetVersions();
    }
}

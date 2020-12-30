using System;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public interface IBluetoothClient : IDisposable
    {
#pragma warning disable 1591
        void Connect(BluetoothEndPoint remoteEP);
        IAsyncResult BeginConnect(BluetoothEndPoint remoteEP, AsyncCallback requestCallback, object state);
        void EndConnect(IAsyncResult asyncResult);
        bool Connected { get; }
        //
        System.Net.Sockets.Socket Client { get; set; }
        //
        int Available { get; }
        //
        bool Authenticate { get; set; }
        IBluetoothDeviceInfo[] DiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly);
#if !V1
        IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly, 
            AsyncCallback callback, object state);
        IBluetoothDeviceInfo[] EndDiscoverDevices(IAsyncResult asyncResult);
        IAsyncResult BeginDiscoverDevices(int maxDevices,
            bool authenticated, bool remembered, bool unknown, bool discoverableOnly,
            AsyncCallback callback, object state,
            InTheHand.Net.Sockets.BluetoothClient.LiveDiscoveryCallback handler, object liveDiscoState);
#endif
        bool Encrypt { get; set; }
        string GetRemoteMachineName(BluetoothAddress a);
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        System.Net.Sockets.NetworkStream GetStream();
#if TEST_EARLY
        [Obsolete("Now that we've wrapped NetworkStream there's no need for this property.")]
        System.IO.Stream GetStream2();
#endif
        System.Net.Sockets.LingerOption LingerState { get; set; }
        TimeSpan InquiryLength { get; set; }
        int InquiryAccessCode { get; set; }
        Guid LinkKey { get; }
        InTheHand.Net.Bluetooth.LinkPolicy LinkPolicy { get; }
        BluetoothEndPoint RemoteEndPoint { get;}
        string RemoteMachineName { get; }
        void SetPin(string pin);
        void SetPin(BluetoothAddress device, string pin);
    }
}

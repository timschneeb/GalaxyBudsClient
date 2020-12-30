using System;
using InTheHand.Net.Bluetooth;
using System.Diagnostics.CodeAnalysis;

namespace InTheHand.Net.Bluetooth.Factory
{
    /// <exclude/>
    public interface IBluetoothListener
    {
#pragma warning disable 1591
        void Construct(Guid service);
        void Construct(BluetoothAddress localAddress, Guid service);
        void Construct(BluetoothEndPoint localEP);
        void Construct(Guid service, byte[] sdpRecord, int channelOffset);
        void Construct(BluetoothAddress localAddress, Guid service, byte[] sdpRecord, int channelOffset);
        void Construct(BluetoothEndPoint localEP, byte[] sdpRecord, int channelOffset);
        void Construct(Guid service, ServiceRecord sdpRecord);
        void Construct(BluetoothAddress localAddress, Guid service, ServiceRecord sdpRecord);
        void Construct(BluetoothEndPoint localEP, ServiceRecord sdpRecord);
        //
        IBluetoothClient AcceptBluetoothClient();
        System.Net.Sockets.Socket AcceptSocket();
        bool Authenticate { get; set; }
        IAsyncResult BeginAcceptBluetoothClient(AsyncCallback callback, object state);
        IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state);
        bool Encrypt { get; set; }
        IBluetoothClient EndAcceptBluetoothClient(IAsyncResult asyncResult);
        System.Net.Sockets.Socket EndAcceptSocket(IAsyncResult asyncResult);
        BluetoothEndPoint LocalEndPoint { get; }
        bool Pending();
        System.Net.Sockets.Socket Server { get; }
        InTheHand.Net.Bluetooth.ServiceClass ServiceClass { get; set; }
        string ServiceName { get; set; }
        InTheHand.Net.Bluetooth.ServiceRecord ServiceRecord { get; }
        void SetPin(string pin);
        void SetPin(BluetoothAddress device, string pin);
        void Start(int backlog);
        void Start();
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Stop",
            Justification="Non public.  Also follows .NET pattern from TcpListener etc.")]
        void Stop();
    }
}

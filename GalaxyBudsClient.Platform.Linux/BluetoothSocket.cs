using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using ThePBone.BlueZNet;
using ThePBone.BlueZNet.Interop;
using Tmds.DBus;

namespace GalaxyBudsClient.Platform.Linux
{
    internal class BluetoothSocket : IProfile1, IDisposable
    {
        public enum SocketStates
        {
            ProfileUnregistered,
            Disconnected,
            Connected
        }
        
        private CloseSafeHandle? _fileDescriptor;
        private UnixStream? _stream;
        private readonly object _streamPadlock = new();
        
        public UnixStream? Stream
        {
            get
            {
                lock (_streamPadlock)
                {
                    return _stream;
                }
            }
            private set
            {
                lock (_streamPadlock)
                {
                    _stream = value;
                }
            }
        }
        public SocketStates SocketState { get; set; } = SocketStates.Disconnected;

        public Action<ObjectPath,CloseSafeHandle?,IDictionary<string,object>>? NewConnection {get;set;}
        public Action<ObjectPath,CloseSafeHandle?>? RequestDisconnection { get; set; }
        public Action<CloseSafeHandle?>? Release { get; set; }

        public Task ReleaseAsync ()
        {
            SocketState = SocketStates.ProfileUnregistered;
            Log.Debug("Linux.BluetoothSocket: Released");
            
            Task.Run(() => Release?.Invoke(_fileDescriptor));

            return Task.CompletedTask;
        }
        
        public Task NewConnectionAsync (ObjectPath device, CloseSafeHandle fileDescriptor, IDictionary<string,object> properties)
        {
            SocketState = SocketStates.Connected;
            _fileDescriptor = fileDescriptor;
            Log.Debug("Linux.BluetoothSocket: Connected to profile");

            Stream = new UnixStream(fileDescriptor.DangerousGetHandle().ToInt32());
            
            Task.Run(() => NewConnection?.Invoke(device, _fileDescriptor, properties));
            return Task.CompletedTask;
        }
        
        public Task RequestDisconnectionAsync (ObjectPath device)
        {
            SocketState = SocketStates.Disconnected;
            
            Log.Debug("Linux.BluetoothSocket: Disconnection requested");
            if (RequestDisconnection != null) 
            {
                Task.Run(() => RequestDisconnection?.Invoke(device, _fileDescriptor));
            }
            
            Stream?.Close();
            _fileDescriptor?.Close();

            return Task.CompletedTask;
        }

        public ObjectPath ObjectPath => "/bluez/profiles/galaxybudsclient";

        public async void Dispose()
        {
            await ReleaseAsync();
            _fileDescriptor?.Dispose();
            _stream?.Dispose();
        }
    }
}
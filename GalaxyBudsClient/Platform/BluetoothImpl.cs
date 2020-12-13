using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GalaxyBudsClient.Bluetooth;
using GalaxyBudsClient.Interface.Pages;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Utils;
using Serilog;
using Task = System.Threading.Tasks.Task;

namespace GalaxyBudsClient.Platform
{
    public class BluetoothImpl
    { 
        private static readonly object Padlock = new object();
        private static BluetoothImpl? _instance;
        public static BluetoothImpl Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new BluetoothImpl();
                }
            }
        }

        private readonly IBluetoothService _backend;
        
        public event EventHandler? Connected;
        public event EventHandler? Connecting;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<SPPMessage>? MessageReceived;
        public event EventHandler<InvalidDataException>? InvalidDataReceived;
        public event EventHandler<byte[]>? NewDataReceived;
        public event EventHandler<BluetoothException>? BluetoothError;
        
        public Models ActiveModel => SettingsProvider.Instance.RegisteredDevice.Model;
        public IDeviceSpec DeviceSpec => DeviceSpecHelper.FindByModel(ActiveModel) ?? new StubDeviceSpec();

        public bool IsConnected => _backend.IsStreamConnected;
        
        private BluetoothImpl()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                _backend = new Bluetooth.Linux.BluetoothService();
            else
                throw new PlatformNotSupportedException();

            _backend.Connecting += (sender, args) => Connecting?.Invoke(this, EventArgs.Empty); 
            _backend.NewDataAvailable += OnNewDataAvailable;
            _backend.NewDataAvailable += (sender, bytes) => NewDataReceived?.Invoke(this, bytes);
            _backend.BluetoothErrorAsync += (sender, exception) => BluetoothError?.Invoke(this, exception); 
            _backend.RfcommConnected += (sender, args) => Task.Run(async () =>
                    await Task.Delay(150).ContinueWith((_) => Connected?.Invoke(this, EventArgs.Empty)));
            _backend.Disconnected += (sender, reason) => Disconnected?.Invoke(this, reason);
            MessageReceived += SPPMessageHandler.Instance.MessageReceiver;
        }

        public async Task<BluetoothDevice[]> GetDevicesAsync()
        {
            return await _backend.GetDevicesAsync();
        }

        public async Task ConnectAsync(string? macAddress = null, Models? model = null, bool noRetry = false)
        {
            /* Load from configuration */
            if (macAddress == null && model == null)
            {
                if (RegisteredDeviceValid && ServiceUuid != null)
                {
                    try
                    {
                        await _backend.ConnectAsync(SettingsProvider.Instance.RegisteredDevice.MacAddress,
                            ServiceUuid.ToString()!, noRetry);
                    }
                    catch (BluetoothException ex)
                    {
                        BluetoothError?.Invoke(this, ex);
                    }
                }
                else
                {
                    Log.Error("BluetoothImpl: Connection attempt without valid cached device");
                }
            }
            /* Update device registration */
            else if(macAddress != null && model != null)
            {
                if (IsDeviceValid((Models) model, macAddress))
                {
                    SettingsProvider.Instance.RegisteredDevice.Model = (Models) model;
                    SettingsProvider.Instance.RegisteredDevice.MacAddress = macAddress;

                    /* Load from configuration this time */
                    await ConnectAsync();
                }
                else
                {
                    Log.Error("BluetoothImpl: Connection attempt without valid device");
                }
            }
            else
            {
                throw new ArgumentException("Either all or none arguments must be null");
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _backend.DisconnectAsync();
            }
            catch (BluetoothException ex)
            {
                BluetoothError?.Invoke(this, ex);
            }
        }
        
        public async Task SendAsync(SPPMessage msg)
        {
            try
            {
                Log.Verbose($"<< Outgoing: {msg}");
                await _backend.SendAsync(msg.EncodeMessage());
            }
            catch (BluetoothException ex)
            {
                BluetoothError?.Invoke(this, ex);
            }
        }
        
        public async Task SendRequestAsync(SPPMessage.MessageIds id, params byte[]? payload)
        {
            await SendAsync(new SPPMessage{Id = id, Payload = payload ?? new byte[0], Type = SPPMessage.MsgType.Request});
        }
        
        public async Task SendRequestAsync(SPPMessage.MessageIds id, bool payload)
        {
            await SendAsync(new SPPMessage{Id = id, Payload = payload ? new byte[]{0x01} : new byte[]{0x00}, Type = SPPMessage.MsgType.Request});
        }
        
        public async Task UnregisterDevice()
        {
            SettingsProvider.Instance.RegisteredDevice.Model = Models.NULL;
            SettingsProvider.Instance.RegisteredDevice.MacAddress = string.Empty;
            await DisconnectAsync();
        }
        
        public bool RegisteredDeviceValid =>
            IsDeviceValid(SettingsProvider.Instance.RegisteredDevice.Model,
                SettingsProvider.Instance.RegisteredDevice.MacAddress);

        private static bool IsDeviceValid(Models model, string macAddress)
        {
            return model != Models.NULL && macAddress.Length >= 12;
        }

        private Guid? ServiceUuid
        {
            get
            {
                switch (ActiveModel)
                {
                    case Models.Buds:
                        return Uuids.Buds;
                    case Models.BudsPlus:
                        return Uuids.BudsPlus;
                    case Models.BudsLive:
                        return Uuids.BudsLive;
                    default:
                        return null;
                }
            }
        }
        
        private void OnNewDataAvailable(object? sender, byte[] frame)
        {
            ArrayList data = new ArrayList(frame);
            do
            {
                try
                {
                    SPPMessage msg = SPPMessage.DecodeMessage(data.OfType<byte>().ToArray());
                    Log.Verbose($">> Incoming: {msg}");
                    MessageReceived?.Invoke(this, msg);
                
                    if (msg.TotalPacketSize >= data.Count)
                        break;
                    data.RemoveRange(0, msg.TotalPacketSize);
                    
                    if (ByteArrayUtils.IsBufferZeroedOut(data))
                    {
                        /* No more data remaining */
                        break;
                    }
                }
                catch (InvalidDataException e)
                {
                    InvalidDataReceived?.Invoke(this, e);
                    break;
                }
            } while (data.Count > 0);
        }
    }
}
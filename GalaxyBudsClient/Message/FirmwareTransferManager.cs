using System;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using Serilog;

namespace GalaxyBudsClient.Message
{
    public class FirmwareTransferManager
    {
        private static readonly object Padlock = new object();
        private static FirmwareTransferManager? _instance;
        public static FirmwareTransferManager Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ??= new FirmwareTransferManager();
                }
            }
        }
        
        public enum States
        {
            Ready,
            InitializingSession,
            Uploading
        }

        private States _state;
        public States State
        {
            private set
            {
                var old = _state;
                _state = value;
                // Only notify on change
                if (old != value)
                {
                    StateChanged?.Invoke(this, value);
                }
            }
            get => _state;
        }

        public event EventHandler<FirmwareTransferException>? Error; 
        public event EventHandler<int>? ProgressChanged; 
        public event EventHandler<States>? StateChanged; 
        public event EventHandler? Finished; 
        
        private readonly Timer _sessionTimeout;
        private readonly Timer _controlTimeout;
        private readonly Timer _genericTimeout;

        private int _mtuSize;
        private int _currentSegment;
        private int _currentProgress;
        private long _lastSegmentOffset;
        private bool _lastFragment;
        private FirmwareBinary? _binary;
        
        public FirmwareTransferManager()
        {
            _sessionTimeout = new Timer(20000);
            _controlTimeout = new Timer(20000);
            _genericTimeout = new Timer(600000);
            _sessionTimeout.Elapsed += OnSessionTimeoutElapsed;
            _controlTimeout.Elapsed += OnControlTimeoutElapsed;
            _genericTimeout.Elapsed += OnCopyTimeoutElapsed;

            Error += (sender, exception) => Cancel();
            StateChanged += (sender, state) => Log.Debug($"FirmwareTransferManager: Status changed to {state}");
            
            BluetoothImpl.Instance.Disconnected += (sender, s) =>
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.Disconnected, 
                    "Lost connection to the device. The firmware transfer has been cancelled."));
            };
            
            SPPMessageHandler.Instance.AnyMessageReceived += OnMessageReceived;
        }

        private async void OnMessageReceived(object? sender, BaseMessageParser? e)
        {
            if (_binary == null)
            {
                return;
            }
            
            switch (e)
            {
                case FotaSessionParser session:
                {
                    Log.Debug($"FirmwareTransferManager.OnMessageReceived: Session result is {session.ResultCode}");
                
                    _sessionTimeout.Stop();
                    if (session.ResultCode != 0)
                    {
                        Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.SessionFail, 
                            $"Failed to open a new session. The device returned an error code ({session.ResultCode})"));
                    }
                    else
                    {
                        _controlTimeout.Start();
                    }

                    break;
                }
                case FotaControlParser control:
                    Log.Debug($"FirmwareTransferManager.OnMessageReceived: Control block has CID: {control.ControlId}");
                    switch (control.ControlId)
                    {
                        case FirmwareConstants.ControlIds.SendMtu:
                            _controlTimeout.Stop();
                            _mtuSize = control.MtuSize;

                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.MtuSize));
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: MTU size set to {control.MtuSize}");
                            break;
                        case FirmwareConstants.ControlIds.ReadyToDownload:
                            _currentSegment = control.Id;
                        
                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.Id));
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: Ready to download id {control.Id}");
                            break;
                    }
                    break;
                case FotaDownloadDataParser download:
                    State = States.Uploading;
                    
                    for (var i2 = 0; i2 < download.RequestPacketNumber; i2++)
                    {
                        var downloadEncoder = new FotaDownloadDataEncoder(_binary, _currentSegment, (int)download.ReceivedOffset + (_mtuSize * i2), _mtuSize);

                        _lastFragment = downloadEncoder.IsLastFragment();
                        _lastSegmentOffset = downloadEncoder.Offset;

                        await BluetoothImpl.Instance.SendAsync(downloadEncoder.Build());
                    }
                    break;
                case FotaUpdateParser update:
                    switch (update.UpdateId)
                    {
                        case FirmwareConstants.UpdateIds.Percent:
                            _currentProgress = update.Percent;
                            ProgressChanged?.Invoke(this, _currentProgress);
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: Copy progress: {update.Percent}%");
                            break;
                        case FirmwareConstants.UpdateIds.StateChange:
                            await BluetoothImpl.Instance.SendResponseAsync(SPPMessage.MessageIds.FOTA_UPDATE, 1);
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: State changed: {update.State}, result code: {update.ResultCode}");

                            if (update.State == 0)
                            {
                                Log.Debug($"FirmwareTransferManager.OnMessageReceived: Transfer complete. The device will now proceed with the flashing process on its own.");
                                Finished?.Invoke(this, EventArgs.Empty);
                                Cancel();
                            }
                            else
                            {
                                Log.Debug($"FirmwareTransferManager.OnMessageReceived: Copy failed, result code: {update.ResultCode}");
                                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyFail, 
                                    $"Failed to copy firmware binary. Please reconnect and try again. Error code returned by device: {update.ResultCode}"));
                            }
                            break;
                    }
                    
                    break;
            }
        }

        public async Task Install(string path)
        {
            if (!BluetoothImpl.Instance.IsConnected)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.Disconnected, 
                    "The device is currently not connected. Please check your Bluetooth system settings and reconnect."));
                return;
            }
            
            if (State != States.Ready)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.InProgress, 
                    "Another firmware transfer is already in progress. Please cancel it properly first."));
                return;
            }

            _mtuSize = 0;
            _currentSegment = 0;
            _lastSegmentOffset = 0;
            _lastFragment = false;
                            
            try
            {
                State = States.InitializingSession;
                _binary = new FirmwareBinary(path);
                await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FOTA_OPEN, _binary.SerializeTable());
                _sessionTimeout.Start();
                _genericTimeout.Start();
            }
            catch (FirmwareParseException ex)
            {
                Error?.Invoke(this, new FirmwareTransferException(ex));
            }
        }

        public async void Cancel()
        {
            _binary = null;
            _mtuSize = 0;
            _currentSegment = 0;
            _lastSegmentOffset = 0;
            _lastFragment = false;
            
            State = States.Ready;
            
            _sessionTimeout.Stop();
            _controlTimeout.Stop();
            _genericTimeout.Stop();

            await BluetoothImpl.Instance.DisconnectAsync();
            await Task.Delay(100);
            await BluetoothImpl.Instance.ConnectAsync();
        }

        private void OnSessionTimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.SessionTimeout, 
                "Timed out while waiting for the device to open a new session"));
        } 
        
        private void OnControlTimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.ControlTimeout, 
                "Timed out while waiting for the device to return a control block"));
        }
        
        private void OnCopyTimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyTimeout, 
                "Timed out while waiting for the transfer process to finish"));
        }
    }
}
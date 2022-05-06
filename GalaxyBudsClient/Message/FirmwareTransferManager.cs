using System;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.DynamicLocalization;
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
        public event EventHandler<FirmwareProgressEventArgs>? ProgressChanged; 
        public event EventHandler<States>? StateChanged; 
        public event EventHandler? Finished; 
        public event EventHandler<short>? MtuChanged; 
        public event EventHandler<short>? CurrentSegmentIdChanged; 
        public event EventHandler<FirmwareBlockChangedEventArgs>? CurrentBlockChanged; 
        
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
            _genericTimeout = new Timer(600000 * 2); // 20 min
            _sessionTimeout.Elapsed += OnSessionTimeoutElapsed;
            _controlTimeout.Elapsed += OnControlTimeoutElapsed;
            _genericTimeout.Elapsed += OnCopyTimeoutElapsed;

            Error += (sender, exception) =>
            {
                Log.Error($"FirmwareTransferManager.OnError: {exception}");
                Cancel();
            };
            StateChanged += (sender, state) => Log.Debug($"FirmwareTransferManager: Status changed to {state}");
            
            BluetoothImpl.Instance.Disconnected += (sender, s) =>
            {
                if (_binary != null)
                {
                    Log.Debug("FirmwareTransferManager: Disconnected. Transfer cancelled");
                    Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.Disconnected,
                        Loc.Resolve("fw_fail_connection")));
                }
            };
            BluetoothImpl.Instance.BluetoothError += (sender, exception) =>
            {
                if (_binary != null)
                {
                    Log.Debug("FirmwareTransferManager: Bluetooth error. Transfer cancelled");
                    Cancel();
                }
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
                            string.Format("fw_fail_session", session.ResultCode)));
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
                            MtuChanged?.Invoke(this, control.MtuSize);

                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.MtuSize));
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: MTU size set to {control.MtuSize}");
                            break;
                        case FirmwareConstants.ControlIds.ReadyToDownload:
                            _currentSegment = control.Id;
                            CurrentSegmentIdChanged?.Invoke(this, control.Id);
                            
                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.Id));
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: Ready to download segment {control.Id}");
                            break;
                    }
                    break;
                case FotaDownloadDataParser download:
                    State = States.Uploading;

                    var segment = _binary.GetSegmentById(_currentSegment);
                    CurrentBlockChanged?.Invoke(this, new FirmwareBlockChangedEventArgs(_currentSegment, (int)download.ReceivedOffset, 
                        (int)download.ReceivedOffset + (_mtuSize * download.RequestPacketNumber), download.RequestPacketNumber, (int?)segment?.Size ?? 0, (int?)segment?.Crc32 ?? 0));

                    for (byte i2 = 0; i2 < download.RequestPacketNumber; i2++)
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
                            ProgressChanged?.Invoke(this, new FirmwareProgressEventArgs(
                                _currentProgress, 
                                (long)Math.Round(_binary.TotalSize * (_currentProgress / 100f)), 
                                _binary.TotalSize));
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: Copy progress: {update.Percent}% ({(long)Math.Round(_binary.TotalSize * (_currentProgress / 100f)) / 1000f}KB/{_binary.TotalSize / 1000f}KB)");
                            break;
                        case FirmwareConstants.UpdateIds.StateChange:
                            await BluetoothImpl.Instance.SendResponseAsync(SPPMessage.MessageIds.FOTA_UPDATE, 1);
                            Log.Debug($"FirmwareTransferManager.OnMessageReceived: State changed: {update.State}, result code: {update.ResultCode}");

                            if (update.State == 0)
                            {
                                Log.Debug($"FirmwareTransferManager.OnMessageReceived: Transfer complete (FOTA_STATE_CHANGE). The device will now proceed with the flashing process on its own.");
                                Finished?.Invoke(this, EventArgs.Empty);
                                Cancel();
                            }
                            else
                            {
                                Log.Debug($"FirmwareTransferManager.OnMessageReceived: Copy failed, result code: {update.ResultCode}");
                                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyFail, 
                                    string.Format(Loc.Resolve("fw_fail_copy"), update.ResultCode)));
                            }
                            break;
                    }
                    break;
                case FotaResultParser result:
                    await BluetoothImpl.Instance.SendResponseAsync(SPPMessage.MessageIds.FOTA_RESULT, 1);
                    Log.Debug($"FirmwareTransferManager.OnMessageReceived: Finished. Result: {result.Result}, error code: {result.ErrorCode}");

                    if (result.Result == 0)
                    {
                        Log.Debug($"FirmwareTransferManager.OnMessageReceived: Transfer complete (FOTA_RESULT). The device will now proceed with the flashing process on its own.");
                        Finished?.Invoke(this, EventArgs.Empty);
                        Cancel();
                    }
                    else
                    {
                        Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.VerifyFail, 
                            string.Format(Loc.Resolve("fw_fail_verify"), result.ErrorCode)));
                    }
                    break;
            }
        }

        public async Task Install(FirmwareBinary binary)
        {
            if (!BluetoothImpl.Instance.IsConnected)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.Disconnected,
                    Loc.Resolve("fw_fail_connection_precheck")));
                return;
            }

            if (State != States.Ready)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.InProgress,
                    Loc.Resolve("fw_fail_pending")));
                return;
            }

            if (DeviceMessageCache.Instance.ExtendedStatusUpdate?.IsCoupled ?? true)
            {
                if ((DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryL ?? 0) < 15 ||
                    (DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryL ?? 0) < 15)
                {
                    Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.BatteryLow,
                        Loc.Resolve("fw_fail_lowbattery")));
                    return;
                }
            }
            else if ((DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryL ?? 0) < 15 &&
                     (DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryL ?? 0) < 15)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.BatteryLow,
                    Loc.Resolve("fw_fail_lowbattery")));
                return;
            }

            _mtuSize = 0;
            _currentSegment = 0;
            _lastSegmentOffset = 0;
            _lastFragment = false;
            _binary = binary;
            
            State = States.InitializingSession;
            
            await BluetoothImpl.Instance.SendRequestAsync(SPPMessage.MessageIds.FOTA_OPEN, _binary.SerializeTable());
            _sessionTimeout.Start();
            _genericTimeout.Start();
        }

        public bool IsInProgress()
        {
            return State != States.Ready;
        }
        
        public async void Cancel()
        {
            _binary?.Dispose();
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

        private void OnSessionTimeoutElapsed(object? sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.SessionTimeout, 
                Loc.Resolve("fw_fail_session_timeout")));
        } 
        
        private void OnControlTimeoutElapsed(object? sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.ControlTimeout, 
                Loc.Resolve("fw_fail_control_timeout")));
        }
        
        private void OnCopyTimeoutElapsed(object? sender, ElapsedEventArgs e)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyTimeout, 
                Loc.Resolve("fw_fail_copy_timeout")));
        }
    }
}
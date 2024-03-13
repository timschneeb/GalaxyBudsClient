using System;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;
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
            _sessionTimeout.Elapsed += OnSessionTimeoutElapsed;
            _controlTimeout.Elapsed += OnControlTimeoutElapsed;

            Error += (sender, exception) =>
            {
                Log.Error(exception, "FirmwareTransferManager.OnError");
                Cancel();
            };
            StateChanged += (sender, state) => Log.Debug("FirmwareTransferManager: Status changed to {State}", state);
            
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
            
            SppMessageHandler.Instance.AnyMessageReceived += OnMessageReceived;
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
                    Log.Debug("FirmwareTransferManager.OnMessageReceived: Session result is {Code}", session.ResultCode);
                
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
                    Log.Debug("FirmwareTransferManager.OnMessageReceived: Control block has CID: {Id}", control.ControlId);
                    switch (control.ControlId)
                    {
                        case FirmwareConstants.ControlIds.SendMtu:
                            _controlTimeout.Stop();
                            _mtuSize = control.MtuSize;
                            MtuChanged?.Invoke(this, control.MtuSize);

                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.MtuSize));
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: MTU size set to {MtuSize}", control.MtuSize);
                            break;
                        case FirmwareConstants.ControlIds.ReadyToDownload:
                            _currentSegment = control.Id;
                            CurrentSegmentIdChanged?.Invoke(this, control.Id);
                            
                            await BluetoothImpl.Instance.SendAsync(FotaControlEncoder.Build(control.ControlId, control.Id));
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: Ready to download segment {Id}", control.Id);
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
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: Copy progress: {Percent}% ({Done}KB/{TotalSize}KB)", 
                                update.Percent, 
                                (long)Math.Round(_binary.TotalSize * (_currentProgress / 100f)) / 1000f, 
                                _binary.TotalSize / 1000f);
                            break;
                        case FirmwareConstants.UpdateIds.StateChange:
                            await BluetoothImpl.Instance.SendResponseAsync(SppMessage.MessageIds.FOTA_UPDATE, 1);
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: State changed: {State}, result code: {ResultCode}", 
                                update.State, update.ResultCode);

                            if (update.State == 0)
                            {
                                Log.Debug($"FirmwareTransferManager.OnMessageReceived: Transfer complete (FOTA_STATE_CHANGE). The device will now proceed with the flashing process on its own.");
                                Finished?.Invoke(this, EventArgs.Empty);
                                Cancel();
                            }
                            else
                            {
                                Log.Debug("FirmwareTransferManager.OnMessageReceived: Copy failed, result code: {Code}", update.ResultCode);
                                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyFail, 
                                    string.Format(Loc.Resolve("fw_fail_copy"), update.ResultCode)));
                            }
                            break;
                    }
                    break;
                case FotaResultParser result:
                    await BluetoothImpl.Instance.SendResponseAsync(SppMessage.MessageIds.FOTA_RESULT, 1);
                    Log.Debug("FirmwareTransferManager.OnMessageReceived: Finished. Result: {Result}, error code: {Code}", 
                        result.Result, result.ErrorCode);

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

            /*if ((DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryL ?? 100) < 15 ||
                (DeviceMessageCache.Instance.BasicStatusUpdate?.BatteryR ?? 100) < 15)
            {
                Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.BatteryLow,
                    Loc.Resolve("fw_fail_lowbattery")));
                return;
            }*/

            _mtuSize = 0;
            _currentSegment = 0;
            _lastSegmentOffset = 0;
            _lastFragment = false;
            _binary = binary;
            
            State = States.InitializingSession;
            
            await BluetoothImpl.Instance.SendRequestAsync(SppMessage.MessageIds.FOTA_OPEN, _binary.SerializeTable());
            _sessionTimeout.Start();
        }

        public bool IsInProgress()
        {
            return State != States.Ready;
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
    }
}
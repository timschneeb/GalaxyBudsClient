using System;
using System.Threading.Tasks;
using System.Timers;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Encoder;
using GalaxyBudsClient.Model.Firmware;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface;
using Serilog;

namespace GalaxyBudsClient.Message;

public class FirmwareTransferManager
{
    private static readonly object Padlock = new();
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
                    Strings.FwFailConnection));
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
            
        SppMessageReceiver.Instance.AnyMessageDecoded += OnMessageDecoded;
    }

    private async void OnMessageDecoded(object? sender, BaseMessageDecoder? e)
    {
        if (_binary == null)
        {
            return;
        }
            
        switch (e)
        {
            case FotaSessionDecoder session:
            {
                Log.Debug("FirmwareTransferManager.OnMessageReceived: Session result is {Code}", session.ResultCode);
                
                _sessionTimeout.Stop();
                if (session.ResultCode != 0)
                {
                    Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.SessionFail, 
                        string.Format(Strings.FwFailSession, session.ResultCode)));
                }
                else
                {
                    _controlTimeout.Start();
                }

                break;
            }
            case FotaControlDecoder control:
                Log.Debug("FirmwareTransferManager.OnMessageReceived: Control block has CID: {Id}", control.ControlId);
                switch (control.ControlId)
                {
                    case FirmwareConstants.ControlIds.SendMtu:
                        _controlTimeout.Stop();
                        _mtuSize = control.MtuSize;
                        MtuChanged?.Invoke(this, control.MtuSize);

                        await BluetoothImpl.Instance.SendAsync(new FotaControlEncoder
                        {
                            ControlId = control.ControlId,
                            Parameter = control.MtuSize
                        });
                        Log.Debug("FirmwareTransferManager.OnMessageReceived: MTU size set to {MtuSize}", control.MtuSize);
                        break;
                    case FirmwareConstants.ControlIds.ReadyToDownload:
                        _currentSegment = control.Id;
                        CurrentSegmentIdChanged?.Invoke(this, control.Id);
                        
                        await BluetoothImpl.Instance.SendAsync(new FotaControlEncoder
                        {
                            ControlId = control.ControlId,
                            Parameter = control.Id
                        });
                        Log.Debug("FirmwareTransferManager.OnMessageReceived: Ready to download segment {Id}", control.Id);
                        break;
                }
                break;
            case FotaDownloadDataDecoder download:
                State = States.Uploading;

                var segment = _binary.GetSegmentById(_currentSegment);
                CurrentBlockChanged?.Invoke(this, new FirmwareBlockChangedEventArgs(_currentSegment, (int)download.ReceivedOffset, 
                    (int)download.ReceivedOffset + _mtuSize * download.RequestPacketNumber, download.RequestPacketNumber, (int?)segment?.Size ?? 0, (int?)segment?.Crc32 ?? 0));

                for (byte i2 = 0; i2 < download.RequestPacketNumber; i2++)
                {   
                    var downloadEncoder = new FotaDownloadDataEncoder
                    {
                        Binary = _binary,
                        EntryId = _currentSegment,
                        Offset = (int)download.ReceivedOffset + _mtuSize * i2,
                        MtuSize = _mtuSize
                    };
                    _lastFragment = downloadEncoder.IsLastFragment();
                    _lastSegmentOffset = downloadEncoder.Offset;
                        
                    await BluetoothImpl.Instance.SendAsync(downloadEncoder);
                }
                break;
            case FotaUpdateDecoder update:
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
                        await BluetoothImpl.Instance.SendResponseAsync(MsgIds.FOTA_UPDATE, 1);
                        Log.Debug("FirmwareTransferManager.OnMessageReceived: State changed: {State}, result code: {ResultCode}", 
                            update.State, update.ResultCode);

                        if (update.State == 0)
                        {
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: Transfer complete (FOTA_STATE_CHANGE). The device will now proceed with the flashing process on its own.");
                            Finished?.Invoke(this, EventArgs.Empty);
                            Cancel();
                        }
                        else
                        {
                            Log.Debug("FirmwareTransferManager.OnMessageReceived: Copy failed, result code: {Code}", update.ResultCode);
                            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.CopyFail, 
                                string.Format(Strings.FwFailCopy, update.ResultCode)));
                        }
                        break;
                }
                break;
            case FotaResultDecoder result:
                await BluetoothImpl.Instance.SendResponseAsync(MsgIds.FOTA_RESULT, 1);
                Log.Debug("FirmwareTransferManager.OnMessageReceived: Finished. Result: {Result}, error code: {Code}", 
                    result.Result, result.ErrorCode);

                if (result.Result == 0)
                {
                    Log.Debug("FirmwareTransferManager.OnMessageReceived: Transfer complete (FOTA_RESULT). The device will now proceed with the flashing process on its own.");
                    Finished?.Invoke(this, EventArgs.Empty);
                    Cancel();
                }
                else
                {
                    Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.VerifyFail, 
                        string.Format(Strings.FwFailVerify, result.ErrorCode)));
                }
                break;
        }
    }

    public async Task Install(FirmwareBinary binary)
    {
        if (!BluetoothImpl.Instance.IsConnected)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.Disconnected,
                Strings.FwFailConnectionPrecheck));
            return;
        }

        if (State != States.Ready)
        {
            Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.InProgress,
                Strings.FwFailPending));
            return;
        }

        _mtuSize = 0;
        _currentSegment = 0;
        _lastSegmentOffset = 0;
        _lastFragment = false;
        _binary = binary;
            
        State = States.InitializingSession;
            
        await BluetoothImpl.Instance.SendRequestAsync(MsgIds.FOTA_OPEN, _binary.SerializeTable());
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
            Strings.FwFailSessionTimeout));
    } 
        
    private void OnControlTimeoutElapsed(object? sender, ElapsedEventArgs e)
    {
        Error?.Invoke(this, new FirmwareTransferException(FirmwareTransferException.ErrorCodes.ControlTimeout, 
            Strings.FwFailControlTimeout));
    }
}
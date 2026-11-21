using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Message.Parameter;
using GalaxyBudsClient.Model;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using Tmds.DBus;

#pragma warning disable CS0414 // Field is assigned but its value is never used

// ReSharper disable InconsistentNaming

[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
[assembly: InternalsVisibleTo("GalaxyBudsClient.Tests")]
namespace GalaxyBudsClient.Cli.Ipc.Objects;

[DBusInterface("me.timschneeberger.GalaxyBudsClient.Device")]
public interface IDeviceObject : IDBusObject
{
    Task<object> GetAsync(string prop);
    Task<DeviceProperties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    Task SetNoiseControlModeAsync(string mode);
    Task SetAmbientSoundVolumeAsync(int volume);
}

[Dictionary]
public class DeviceProperties : BaseProperties
{
    [Property(Access = PropertyAccess.Read)] internal string _Name = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _Address = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _Model = Models.NULL.ToString();
    [Property(Access = PropertyAccess.Read)] internal int _BatteryLeft = -1;
    [Property(Access = PropertyAccess.Read)] internal int _BatteryRight = -1;
    [Property(Access = PropertyAccess.Read)] internal int _BatteryCase = -1;
    [Property(Access = PropertyAccess.Read)] internal double _VoltageLeft = -1;
    [Property(Access = PropertyAccess.Read)] internal double _VoltageRight = -1;
    [Property(Access = PropertyAccess.Read)] internal double _ThermistorLeftCelsius = -1;
    [Property(Access = PropertyAccess.Read)] internal double _ThermistorRightCelsius = -1;
    [Property(Access = PropertyAccess.Read)] internal string _WearStateLeft = PlacementStates.Disconnected.ToString();
    [Property(Access = PropertyAccess.Read)] internal string _WearStateRight = PlacementStates.Disconnected.ToString();
    [Property(Access = PropertyAccess.Read)] internal string _FirmwareVersion = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _HardwareVersion = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _SkuLeft = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _SkuRight = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal string _NoiseControlMode = string.Empty;
    [Property(Access = PropertyAccess.Read)] internal int _AmbientSoundVolume = -1;
    [Property(Access = PropertyAccess.Read)] internal int _AmbientSoundVolumeMax = -1;
}
    
public sealed class DeviceObject : BaseObjectWithProperties<DeviceProperties>, IDeviceObject, IDisposable
{
    private static readonly TimeSpan AcknowledgementTimeout = TimeSpan.FromSeconds(5);

    public static readonly ObjectPath Path = new("/me/timschneeberger/galaxybudsclient/device");
    public ObjectPath ObjectPath => Path;

    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private readonly object _pendingLock = new();
    private TaskCompletionSource<bool>? _pendingNoiseControl;
    private TaskCompletionSource<bool>? _pendingAmbientVolume;
    private NoiseControlModes _pendingNoiseControlMode;
    private int _pendingAmbientVolumeValue;
    private NoiseControlModes? _currentNoiseControlMode;
    private bool _isExtraHighAmbientEnabled;
    private bool _legacyAmbientEnabled;
    private bool _legacyAncEnabled;

    public DeviceObject()
    {
        SppMessageReceiver.Instance.BaseUpdate += OnBaseUpdate;
        SppMessageReceiver.Instance.GetAllDataResponse += OnGetAllDataResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate += OnDebugSkuUpdate;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.NoiseControlUpdateResponse += OnNoiseControlUpdate;
        SppMessageReceiver.Instance.AmbientEnabledUpdateResponse += OnAmbientEnabledUpdate;
        SppMessageReceiver.Instance.AncEnabledUpdateResponse += OnAncEnabledUpdate;
        SppMessageReceiver.Instance.AcknowledgementResponse += OnAcknowledgement;

        BluetoothImpl.Instance.Connected += OnConnected;
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
    }

    public async Task SetNoiseControlModeAsync(string mode)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;
        if (!TryParseNoiseControlMode(mode, out var parsed) || !IsNoiseControlModeSupported(parsed, spec))
            throw new ArgumentException("Invalid or unsupported noise control mode", nameof(mode));

        if (!BluetoothImpl.Instance.IsConnected)
            throw new InvalidOperationException("Device is not connected");

        await _operationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_currentNoiseControlMode == parsed)
                return;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_pendingLock)
            {
                _pendingNoiseControl = tcs;
                _pendingNoiseControlMode = parsed;
            }

            try
            {
                EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, parsed);
                await tcs.Task.WaitAsync(AcknowledgementTimeout).ConfigureAwait(false);
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException(
                    $"Device did not acknowledge noise control mode '{parsed}' within " +
                    $"{AcknowledgementTimeout.TotalSeconds:0} seconds.", ex);
            }
            finally
            {
                lock (_pendingLock)
                {
                    if (ReferenceEquals(_pendingNoiseControl, tcs))
                        _pendingNoiseControl = null;
                }
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    public async Task SetAmbientSoundVolumeAsync(int volume)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;
        var maximumVolume = GetEffectiveAmbientSoundVolumeMax(spec, _isExtraHighAmbientEnabled);
        if (maximumVolume < 0)
            throw new NotSupportedException("Device does not support ambient sound volume");
        if (volume < 0 || volume > maximumVolume)
            throw new ArgumentOutOfRangeException(nameof(volume),
                $"Ambient sound volume must be between 0 and {maximumVolume}.");

        if (!BluetoothImpl.Instance.IsConnected)
            throw new InvalidOperationException("Device is not connected");

        await _operationLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if ((int)Get(nameof(DeviceProperties._AmbientSoundVolume)) == volume)
                return;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_pendingLock)
            {
                _pendingAmbientVolume = tcs;
                _pendingAmbientVolumeValue = volume;
            }

            try
            {
                EventDispatcher.Instance.Dispatch(Event.SetAmbientSoundVolume, volume);
                await tcs.Task.WaitAsync(AcknowledgementTimeout).ConfigureAwait(false);
            }
            catch (TimeoutException ex)
            {
                throw new TimeoutException(
                    $"Device did not acknowledge ambient sound volume '{volume}' within " +
                    $"{AcknowledgementTimeout.TotalSeconds:0} seconds.", ex);
            }
            finally
            {
                lock (_pendingLock)
                {
                    if (ReferenceEquals(_pendingAmbientVolume, tcs))
                        _pendingAmbientVolume = null;
                }
            }
        }
        finally
        {
            _operationLock.Release();
        }
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;
        var cachedUpdate = DeviceMessageCache.Instance.ExtendedStatusUpdate;

        _currentNoiseControlMode = null;
        _isExtraHighAmbientEnabled = cachedUpdate?.ExtraHighAmbientEnabled ?? false;
        _legacyAmbientEnabled = cachedUpdate?.AmbientSoundEnabled ?? false;
        _legacyAncEnabled = cachedUpdate?.NoiseCancelling ?? false;

        Set(nameof(DeviceProperties._Name), BluetoothImpl.Instance.DeviceName);
        Set(nameof(DeviceProperties._Address), BluetoothImpl.Instance.Device.Current?.MacAddress ?? "null");
        Set(nameof(DeviceProperties._Model), BluetoothImpl.Instance.CurrentModel.ToString());
        UpdateAmbientSoundVolumeMax();

        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnBaseUpdate(this, DeviceMessageCache.Instance.BasicStatusUpdate);
        if (DeviceMessageCache.Instance.DebugGetAllData != null)
            OnGetAllDataResponse(this, DeviceMessageCache.Instance.DebugGetAllData);
        if (DeviceMessageCache.Instance.DebugSku != null)
            OnDebugSkuUpdate(this, DeviceMessageCache.Instance.DebugSku);
        if (cachedUpdate != null)
            OnExtendedStatusUpdate(this, cachedUpdate);
    }

    private void OnDisconnected(object? sender, string reason)
    {
        FailPending(new InvalidOperationException($"Device disconnected: {reason}"));
    }

    private void OnBaseUpdate(object? sender, IBasicStatusUpdate update)
    {
        Set(nameof(DeviceProperties._BatteryLeft), update.BatteryL);
        Set(nameof(DeviceProperties._BatteryRight), update.BatteryR);
        Set(nameof(DeviceProperties._BatteryCase), update.BatteryCase);
        Set(nameof(DeviceProperties._WearStateLeft), update.PlacementL.ToString());
        Set(nameof(DeviceProperties._WearStateRight), update.PlacementR.ToString());
    }

    private void OnGetAllDataResponse(object? sender, DebugGetAllDataDecoder decoder)
    {
        Set(nameof(DeviceProperties._VoltageLeft), decoder.LeftAdcVCell);
        Set(nameof(DeviceProperties._VoltageRight), decoder.RightAdcVCell);
        Set(nameof(DeviceProperties._ThermistorLeftCelsius), decoder.LeftThermistor);
        Set(nameof(DeviceProperties._ThermistorRightCelsius), decoder.RightThermistor);
        Set(nameof(DeviceProperties._FirmwareVersion), decoder.SoftwareVersion ?? "null");
        Set(nameof(DeviceProperties._HardwareVersion), decoder.HardwareVersion ?? "null");
    }

    private void OnExtendedStatusUpdate(object? sender, ExtendedStatusUpdateDecoder update)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;

        if (spec.Supports(Features.NoiseControl))
        {
            if (Enum.IsDefined(update.NoiseControlMode))
                OnNoiseControlUpdate(this, update.NoiseControlMode);
        }
        else
        {
            _legacyAmbientEnabled = update.AmbientSoundEnabled;
            _legacyAncEnabled = update.NoiseCancelling;
            UpdateLegacyNoiseControlMode(spec);
        }

        if (spec.Supports(Features.AmbientSoundVolume))
            Set(nameof(DeviceProperties._AmbientSoundVolume), update.AmbientSoundVolume);

        if (spec.Supports(Features.AmbientExtraLoud))
            _isExtraHighAmbientEnabled = update.ExtraHighAmbientEnabled;

        UpdateAmbientSoundVolumeMax();
        TryCompleteAmbientVolume(update.AmbientSoundVolume);
    }

    private void OnNoiseControlUpdate(object? sender, NoiseControlModes mode)
    {
        _currentNoiseControlMode = mode;
        Set(nameof(DeviceProperties._NoiseControlMode), mode.ToString());
        TryCompleteNoiseControl(mode);
    }

    private void OnAmbientEnabledUpdate(object? sender, bool enabled)
    {
        _legacyAmbientEnabled = enabled;
        UpdateLegacyNoiseControlMode(BluetoothImpl.Instance.DeviceSpec);
    }

    private void OnAncEnabledUpdate(object? sender, bool enabled)
    {
        _legacyAncEnabled = enabled;
        UpdateLegacyNoiseControlMode(BluetoothImpl.Instance.DeviceSpec);
    }

    private void UpdateLegacyNoiseControlMode(IDeviceSpec spec)
    {
        if (spec.Supports(Features.NoiseControl))
            return;

        var mode = _legacyAmbientEnabled
            ? NoiseControlModes.AmbientSound
            : _legacyAncEnabled
                ? NoiseControlModes.NoiseReduction
                : NoiseControlModes.Off;

        _currentNoiseControlMode = mode;
        Set(nameof(DeviceProperties._NoiseControlMode), mode.ToString());
        TryCompleteLegacyNoiseControl(spec);
    }

    private void OnAcknowledgement(object? sender, AcknowledgementDecoder ack)
    {
        if (ack.Parameters is not SimpleAckParameter param)
            return;

        switch (ack.Id)
        {
            case MsgIds.NOISE_CONTROLS:
                var mode = (NoiseControlModes)param.Value;
                if (Enum.IsDefined(mode))
                    OnNoiseControlUpdate(this, mode);
                break;
            case MsgIds.AMBIENT_VOLUME:
                var volume = (int)param.Value;
                Set(nameof(DeviceProperties._AmbientSoundVolume), volume);
                TryCompleteAmbientVolume(volume);
                break;
            case MsgIds.EXTRA_HIGH_AMBIENT:
                _isExtraHighAmbientEnabled = param.Value != 0;
                UpdateAmbientSoundVolumeMax();
                break;
            case MsgIds.SET_NOISE_REDUCTION:
                _legacyAncEnabled = param.Value != 0;
                UpdateLegacyNoiseControlMode(BluetoothImpl.Instance.DeviceSpec);
                break;
        }
    }

    private void OnDebugSkuUpdate(object? sender, DebugSkuDecoder decoder)
    {
        Set(nameof(DeviceProperties._SkuLeft), decoder.LeftSku ?? "null");
        Set(nameof(DeviceProperties._SkuRight), decoder.RightSku ?? "null");
    }

    private void UpdateAmbientSoundVolumeMax()
    {
        Set(nameof(DeviceProperties._AmbientSoundVolumeMax),
            GetEffectiveAmbientSoundVolumeMax(BluetoothImpl.Instance.DeviceSpec, _isExtraHighAmbientEnabled));
    }

    private void TryCompleteNoiseControl(NoiseControlModes mode)
    {
        lock (_pendingLock)
        {
            if (_pendingNoiseControl != null && _pendingNoiseControlMode == mode)
                _pendingNoiseControl.TrySetResult(true);
        }
    }

    private void TryCompleteLegacyNoiseControl(IDeviceSpec spec)
    {
        lock (_pendingLock)
        {
            if (_pendingNoiseControl == null)
                return;

            var ambientSupported = spec.Supports(Features.AmbientSound);
            var ancSupported = spec.Supports(Features.Anc);
            var satisfied = _pendingNoiseControlMode switch
            {
                NoiseControlModes.Off =>
                    (!ambientSupported || !_legacyAmbientEnabled) &&
                    (!ancSupported || !_legacyAncEnabled),
                NoiseControlModes.AmbientSound =>
                    ambientSupported && _legacyAmbientEnabled &&
                    (!ancSupported || !_legacyAncEnabled),
                NoiseControlModes.NoiseReduction =>
                    ancSupported && _legacyAncEnabled &&
                    (!ambientSupported || !_legacyAmbientEnabled),
                _ => false
            };

            if (satisfied)
                _pendingNoiseControl.TrySetResult(true);
        }
    }

    private void TryCompleteAmbientVolume(int volume)
    {
        lock (_pendingLock)
        {
            if (_pendingAmbientVolume != null && _pendingAmbientVolumeValue == volume)
                _pendingAmbientVolume.TrySetResult(true);
        }
    }

    private void FailPending(Exception exception)
    {
        TaskCompletionSource<bool>? noiseControl;
        TaskCompletionSource<bool>? ambientVolume;

        lock (_pendingLock)
        {
            noiseControl = _pendingNoiseControl;
            ambientVolume = _pendingAmbientVolume;
            _pendingNoiseControl = null;
            _pendingAmbientVolume = null;
        }

        noiseControl?.TrySetException(exception);
        ambientVolume?.TrySetException(exception);
    }

    internal static bool TryParseNoiseControlMode(string? value, out NoiseControlModes mode)
    {
        if (Enum.TryParse(value, true, out mode) && Enum.IsDefined(mode))
            return true;

        mode = default;
        return false;
    }

    internal static bool IsNoiseControlModeSupported(NoiseControlModes mode, IDeviceSpec spec)
    {
        if (spec.Supports(Features.NoiseControl))
        {
            return mode switch
            {
                NoiseControlModes.Off or NoiseControlModes.AmbientSound or NoiseControlModes.NoiseReduction => true,
                NoiseControlModes.Adaptive => spec.Supports(Features.NoiseControlAdaptive),
                _ => false
            };
        }

        return mode switch
        {
            NoiseControlModes.Off => spec.Supports(Features.AmbientSound) || spec.Supports(Features.Anc),
            NoiseControlModes.AmbientSound => spec.Supports(Features.AmbientSound),
            NoiseControlModes.NoiseReduction => spec.Supports(Features.Anc),
            _ => false
        };
    }

    internal static int GetEffectiveAmbientSoundVolumeMax(IDeviceSpec spec, bool extraHighAmbientEnabled)
    {
        if (!spec.Supports(Features.AmbientSoundVolume))
            return -1;

        var maximumVolume = spec.MaximumAmbientVolume;
        if (spec.Supports(Features.AmbientExtraLoud) && extraHighAmbientEnabled)
            maximumVolume++;

        return maximumVolume;
    }

    public void Dispose()
    {
        SppMessageReceiver.Instance.BaseUpdate -= OnBaseUpdate;
        SppMessageReceiver.Instance.GetAllDataResponse -= OnGetAllDataResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate -= OnDebugSkuUpdate;
        SppMessageReceiver.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.NoiseControlUpdateResponse -= OnNoiseControlUpdate;
        SppMessageReceiver.Instance.AmbientEnabledUpdateResponse -= OnAmbientEnabledUpdate;
        SppMessageReceiver.Instance.AncEnabledUpdateResponse -= OnAncEnabledUpdate;
        SppMessageReceiver.Instance.AcknowledgementResponse -= OnAcknowledgement;

        BluetoothImpl.Instance.Connected -= OnConnected;
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;

        FailPending(new ObjectDisposedException(nameof(DeviceObject)));
    }
}
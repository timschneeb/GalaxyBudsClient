using System;
using System.Numerics;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Extensions;
using ReactiveUI;
using Serilog;

namespace GalaxyBudsClient.Platform.SpatialAudio;

public class SpatialOrientationEventArgs : EventArgs
{
    public float Yaw { get; }
    public float Pitch { get; }
    public float Roll { get; }
    public Quaternion RelativeQuaternion { get; }
    public Quaternion RawQuaternion { get; }

    public SpatialOrientationEventArgs(float yaw, float pitch, float roll, Quaternion relativeQuaternion, Quaternion rawQuaternion)
    {
        Yaw = yaw;
        Pitch = pitch;
        Roll = roll;
        RelativeQuaternion = relativeQuaternion;
        RawQuaternion = rawQuaternion;
    }
}

public sealed class SpatialAudioService : ReactiveObject, IDisposable
{
    private static readonly object Padlock = new();
    private static SpatialAudioService? _instance;
    public static SpatialAudioService Instance
    {
        get
        {
            lock (Padlock)
            {
                return _instance ??= new SpatialAudioService();
            }
        }
    }

    private SpatialSensorManager? _sensorManager;
    private Quaternion _referenceQuaternion = Quaternion.Identity;
    private Quaternion _filteredQuaternion = Quaternion.Identity;
    private bool _hasReference;

    public event EventHandler<SpatialOrientationEventArgs>? OrientationUpdated;

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        private set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    private float _currentYaw;
    public float CurrentYaw
    {
        get => _currentYaw;
        private set => this.RaiseAndSetIfChanged(ref _currentYaw, value);
    }

    private float _currentPitch;
    public float CurrentPitch
    {
        get => _currentPitch;
        private set => this.RaiseAndSetIfChanged(ref _currentPitch, value);
    }

    private float _currentRoll;
    public float CurrentRoll
    {
        get => _currentRoll;
        private set => this.RaiseAndSetIfChanged(ref _currentRoll, value);
    }

    public bool IsSupported => BluetoothImpl.Instance.DeviceSpec.Supports(Features.SpatialSensor) ||
                               BluetoothImpl.Instance.DeviceSpec.Supports(Features.HeadTracking);

    private SpatialAudioService()
    {
        BluetoothImpl.Instance.Disconnected += OnDisconnected;
        BluetoothImpl.Instance.BluetoothError += (_, _) => Stop();
    }

    public void Start()
    {
        if (IsActive)
            return;

        if (!BluetoothImpl.Instance.IsConnected)
        {
            Log.Warning("SpatialAudioService: Cannot start, device not connected");
            return;
        }

        try
        {
            Log.Information("SpatialAudioService: Starting head tracking sensor");
            _sensorManager = new SpatialSensorManager();
            _sensorManager.NewQuaternionReceived += OnNewQuaternionReceived;
            _sensorManager.Attach();
            _hasReference = false;
            IsActive = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "SpatialAudioService: Failed to start sensor");
            Stop();
        }
    }

    public void Stop()
    {
        if (!IsActive && _sensorManager == null)
            return;

        Log.Information("SpatialAudioService: Stopping head tracking sensor");
        try
        {
            if (_sensorManager != null)
            {
                _sensorManager.NewQuaternionReceived -= OnNewQuaternionReceived;
                _sensorManager.Detach();
                _sensorManager.Dispose();
                _sensorManager = null;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "SpatialAudioService: Error while stopping sensor");
        }

        IsActive = false;
        CurrentYaw = 0;
        CurrentPitch = 0;
        CurrentRoll = 0;
        _hasReference = false;
    }

    public void Toggle()
    {
        if (IsActive)
            Stop();
        else
            Start();
    }

    public void Recenter()
    {
        if (_filteredQuaternion != Quaternion.Identity)
        {
            _referenceQuaternion = _filteredQuaternion;
            _hasReference = true;
            CurrentYaw = 0;
            CurrentPitch = 0;
            CurrentRoll = 0;
            OrientationUpdated?.Invoke(this, new SpatialOrientationEventArgs(0, 0, 0, Quaternion.Identity, _filteredQuaternion));
            Log.Debug("SpatialAudioService: Recentered / Tared orientation to {Reference}", _referenceQuaternion);
        }
    }

    private void OnNewQuaternionReceived(object? sender, Quaternion raw)
    {
        if (!_hasReference)
        {
            _referenceQuaternion = raw;
            _filteredQuaternion = raw;
            _hasReference = true;
        }

        // Slerp smoothing (factor 0.35 gives responsive feel with zero jitter)
        _filteredQuaternion = Quaternion.Slerp(_filteredQuaternion, raw, 0.35f);

        // Compute relative rotation in world frame: R_world = q_current * q_reference^-1
        // This decouples head rotations (around vertical gravity axis) cleanly from earbud placement
        var invRef = Quaternion.Inverse(_referenceQuaternion);
        var relQuat = Quaternion.Normalize(Quaternion.Multiply(_filteredQuaternion, invRef));

        // Convert to Euler angles (Roll, Pitch, Yaw)
        var (rollRad, pitchRad, yawRad) = relQuat.ToRollPitchYaw();

        // Yaw: positive when head turns RIGHT, negative when head turns LEFT (standard OpenTrack/aviation convention)
        var yawDeg = -(float)(yawRad * (180.0 / Math.PI));
        var pitchDeg = (float)(pitchRad * (180.0 / Math.PI));
        var rollDeg = (float)(rollRad * (180.0 / Math.PI));

        CurrentYaw = yawDeg;
        CurrentPitch = pitchDeg;
        CurrentRoll = rollDeg;

        OrientationUpdated?.Invoke(this, new SpatialOrientationEventArgs(yawDeg, pitchDeg, rollDeg, relQuat, raw));
    }

    private void OnDisconnected(object? sender, string e)
    {
        Stop();
    }

    public void Dispose()
    {
        Stop();
        BluetoothImpl.Instance.Disconnected -= OnDisconnected;
    }
}

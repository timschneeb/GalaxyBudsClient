using System;
using System.Runtime.CompilerServices;
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
    public static readonly ObjectPath Path = new("/me/timschneeberger/galaxybudsclient/device");
    public ObjectPath ObjectPath => Path;
            
    public DeviceObject()
    {
        SppMessageReceiver.Instance.BaseUpdate += OnBaseUpdate;
        SppMessageReceiver.Instance.GetAllDataResponse += OnGetAllDataResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate += OnDebugSkuUpdate;
        SppMessageReceiver.Instance.ExtendedStatusUpdate += OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.NoiseControlUpdateResponse += OnNoiseControlUpdate;
        SppMessageReceiver.Instance.AcknowledgementResponse += OnAcknowledgement;
            
        BluetoothImpl.Instance.Connected += OnConnected;
    }

    public Task SetNoiseControlModeAsync(string mode)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;
        if (!spec.Supports(Features.NoiseControl))
            return Task.FromException(new NotSupportedException("Device does not support noise control modes"));
        if (!Enum.TryParse<NoiseControlModes>(mode, true, out var parsed) || !Enum.IsDefined(parsed) ||
            (parsed == NoiseControlModes.Adaptive && !spec.Supports(Features.NoiseControlAdaptive)))
            return Task.FromException(new ArgumentException("Invalid or unsupported noise control mode"));

        EventDispatcher.Instance.Dispatch(Event.SetNoiseControlState, parsed);
        return Task.CompletedTask;
    }

    public Task SetAmbientSoundVolumeAsync(int volume)
    {
        var spec = BluetoothImpl.Instance.DeviceSpec;
        if (!spec.Supports(Features.AmbientSoundVolume))
            return Task.FromException(new NotSupportedException("Device does not support ambient sound volume"));
        if (volume < 0 || volume > spec.MaximumAmbientVolume)
            return Task.FromException(new ArgumentOutOfRangeException(nameof(volume)));

        EventDispatcher.Instance.Dispatch(Event.SetAmbientSoundVolume, volume);
        return Task.CompletedTask;
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        Set(nameof(DeviceProperties._Name), BluetoothImpl.Instance.DeviceName);
        Set(nameof(DeviceProperties._Address), BluetoothImpl.Instance.Device.Current?.MacAddress ?? "null");
        Set(nameof(DeviceProperties._Model), BluetoothImpl.Instance.CurrentModel.ToString());
        Set(nameof(DeviceProperties._AmbientSoundVolumeMax),
            BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientSoundVolume)
                ? BluetoothImpl.Instance.DeviceSpec.MaximumAmbientVolume : -1);

        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnBaseUpdate(this, DeviceMessageCache.Instance.BasicStatusUpdate);
        if (DeviceMessageCache.Instance.DebugGetAllData != null)
            OnGetAllDataResponse(this, DeviceMessageCache.Instance.DebugGetAllData);
        if (DeviceMessageCache.Instance.DebugSku != null)
            OnDebugSkuUpdate(this, DeviceMessageCache.Instance.DebugSku);
        if (DeviceMessageCache.Instance.ExtendedStatusUpdate != null)
            OnExtendedStatusUpdate(this, DeviceMessageCache.Instance.ExtendedStatusUpdate);
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
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.NoiseControl))
            OnNoiseControlUpdate(this, update.NoiseControlMode);
        if (BluetoothImpl.Instance.DeviceSpec.Supports(Features.AmbientSoundVolume))
            Set(nameof(DeviceProperties._AmbientSoundVolume), update.AmbientSoundVolume);
    }

    private void OnNoiseControlUpdate(object? sender, NoiseControlModes mode)
    {
        Set(nameof(DeviceProperties._NoiseControlMode), mode.ToString());
    }

    private void OnAcknowledgement(object? sender, AcknowledgementDecoder ack)
    {
        if (ack.Parameters is not SimpleAckParameter param)
            return;

        switch (ack.Id)
        {
            case MsgIds.NOISE_CONTROLS:
                OnNoiseControlUpdate(this, (NoiseControlModes)param.Value);
                break;
            case MsgIds.AMBIENT_VOLUME:
                Set(nameof(DeviceProperties._AmbientSoundVolume), (int)param.Value);
                break;
        }
    }

    private void OnDebugSkuUpdate(object? sender, DebugSkuDecoder decoder)
    {
        Set(nameof(DeviceProperties._SkuLeft), decoder.LeftSku ?? "null");
        Set(nameof(DeviceProperties._SkuRight), decoder.RightSku ?? "null");
    }

    public void Dispose()
    {
        SppMessageReceiver.Instance.BaseUpdate -= OnBaseUpdate;
        SppMessageReceiver.Instance.GetAllDataResponse -= OnGetAllDataResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate -= OnDebugSkuUpdate;
        SppMessageReceiver.Instance.ExtendedStatusUpdate -= OnExtendedStatusUpdate;
        SppMessageReceiver.Instance.NoiseControlUpdateResponse -= OnNoiseControlUpdate;
        SppMessageReceiver.Instance.AcknowledgementResponse -= OnAcknowledgement;
            
        BluetoothImpl.Instance.Connected -= OnConnected;
    }
}
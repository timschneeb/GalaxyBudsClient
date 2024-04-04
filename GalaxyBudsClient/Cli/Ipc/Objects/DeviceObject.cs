using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils;
using Tmds.DBus;

#pragma warning disable CS0414 // Field is assigned but its value is never used

// ReSharper disable InconsistentNaming

[assembly: InternalsVisibleTo(Connection.DynamicAssemblyName)]
namespace GalaxyBudsClient.Cli.Ipc.Objects;

[DBusInterface("me.timschneeberger.galaxybudsclient.Device")]
public interface IDeviceObject : IDBusObject
{
    Task<object> GetAsync(string prop);
    Task<DeviceProperties> GetAllAsync();
    Task SetAsync(string prop, object val);
    Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
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
            
        BluetoothImpl.Instance.Connected += OnConnected;
    }

    private void OnConnected(object? sender, EventArgs e)
    {
        Set(nameof(DeviceProperties._Name), BluetoothImpl.Instance.DeviceName);
        Set(nameof(DeviceProperties._Address), Settings.Instance.RegisteredDevice.MacAddress);
        Set(nameof(DeviceProperties._Model), BluetoothImpl.ActiveModel.ToString());

        if (DeviceMessageCache.Instance.BasicStatusUpdate != null)
            OnBaseUpdate(this, DeviceMessageCache.Instance.BasicStatusUpdate);
        if (DeviceMessageCache.Instance.DebugGetAllData != null)
            OnGetAllDataResponse(this, DeviceMessageCache.Instance.DebugGetAllData);
        if (DeviceMessageCache.Instance.DebugSku != null)
            OnDebugSkuUpdate(this, DeviceMessageCache.Instance.DebugSku);
    }

    private void OnBaseUpdate(object? sender, IBasicStatusUpdate update)
    {
        Set(nameof(DeviceProperties._BatteryLeft), update.BatteryL);
        Set(nameof(DeviceProperties._BatteryRight), update.BatteryR);
        Set(nameof(DeviceProperties._BatteryCase), update.BatteryCase);
        Set(nameof(DeviceProperties._WearStateLeft), update.PlacementL.ToString());
        Set(nameof(DeviceProperties._WearStateRight), update.PlacementR.ToString());
    }
        
    private void OnGetAllDataResponse(object? sender, DebugGetAllDataParser parser)
    {
        Set(nameof(DeviceProperties._VoltageLeft), parser.LeftAdcVCell);
        Set(nameof(DeviceProperties._VoltageRight), parser.RightAdcVCell);
        Set(nameof(DeviceProperties._ThermistorLeftCelsius), parser.LeftThermistor);
        Set(nameof(DeviceProperties._ThermistorRightCelsius), parser.RightThermistor);
        Set(nameof(DeviceProperties._FirmwareVersion), parser.SoftwareVersion ?? "null");
        Set(nameof(DeviceProperties._HardwareVersion), parser.HardwareVersion ?? "null");
    }
        
    private void OnDebugSkuUpdate(object? sender, DebugSkuParser parser)
    {
        Set(nameof(DeviceProperties._SkuLeft), parser.LeftSku ?? "null");
        Set(nameof(DeviceProperties._SkuRight), parser.RightSku ?? "null");
    }

    public void Dispose()
    {
        SppMessageReceiver.Instance.BaseUpdate -= OnBaseUpdate;
        SppMessageReceiver.Instance.GetAllDataResponse -= OnGetAllDataResponse;
        SppMessageReceiver.Instance.DebugSkuUpdate -= OnDebugSkuUpdate;
            
        BluetoothImpl.Instance.Connected -= OnConnected;
    }
}
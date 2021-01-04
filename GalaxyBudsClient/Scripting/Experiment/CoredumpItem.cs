using System.Globalization;
using System.Reflection;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Scripting.Experiment
{
    public class CoredumpItem
    {
        public byte[] Content { set; get; } = new byte[0];
        public string Side { set; get; } = string.Empty;
        
        public Models Device => BluetoothImpl.Instance.ActiveModel;
        public int Revision => DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision ?? 0;
        public string FirmwareVersion => DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "Unknown";
        public string? MacAddress => DeviceMessageCache.Instance.DebugGetAllData?.LeftBluetoothAddress;
        public string? AppVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        public string? CountryCode => RegionInfo.CurrentRegion.TwoLetterISORegionName;
        public PlatformUtils.Platforms Platform => PlatformUtils.Platform;
    }
}
using System;
using System.Globalization;
using System.Reflection;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Scripting.Experiment
{
    public class ExperimentResult
    {
        public int ResultCode { set; get; }
        public string ResultCodeString { set; get; } = string.Empty;
        public string Result { set; get; } = string.Empty;
        public long ExperimentId { set; get; }
        public Environment? Environment { set; get; }
        
        public Models Device => BluetoothImpl.Instance.ActiveModel;
        public int Revision => DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision ?? 0;
        public string FirmwareVersion => DeviceMessageCache.Instance.DebugGetAllData?.SoftwareVersion ?? "Unknown";
        public string? MacAddress => DeviceMessageCache.Instance.DebugGetAllData?.LeftBluetoothAddress;
        public string? AppVersion => Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        public string? CountryCode => RegionInfo.CurrentRegion.TwoLetterISORegionName;
        public PlatformUtils.Platforms Platform => PlatformUtils.Platform;
    }
}
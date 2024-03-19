using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Attributes;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public interface IDeviceSpec
    {
        public Dictionary<Features, FeatureRule?> Rules { get; }
        public Models Device { get; }
        public string DeviceBaseName { get; }
        public string FriendlyName => Device.GetModelMetadata()?.Name ?? "null";
        public ITouchOption TouchMap { get; }
        public Guid ServiceUuid { get; }
        public IReadOnlyCollection<TrayItemTypes> TrayShortcuts { get; }
        public string IconResourceKey { get; }
        public int MaximumAmbientVolume { get; }
        
        public bool Supports(Features features)
        {
            // TODO remove this
            return true;
            
            if (!Rules.ContainsKey(features))
            {
                return false;
            }

            if (Rules[features] == null)
            {
                return true;
            }

            if (DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision == null)
            {
                Log.Warning("IDeviceSpec: Cannot compare revision. No ExtendedStatusUpdate cached");
                return true;
            }

            return DeviceMessageCache.Instance.ExtendedStatusUpdate.Revision >= Rules[features]?.MinimumRevision;
        }

        public string RecommendedFwVersion(Features features)
        {
            return Rules.ContainsKey(features) ? Rules[features]?.RecommendedFirmwareVersion ?? "---" : "???";
        }
    }
    
    public class FeatureRule
    {
        public FeatureRule(int minimumRevision, string recommendedFirmwareVersion)
        {
            MinimumRevision = minimumRevision;
            RecommendedFirmwareVersion = recommendedFirmwareVersion;
        }
        public int MinimumRevision { get; }
        public string RecommendedFirmwareVersion { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public interface IDeviceSpec
    {
        public enum Feature
        {
            SeamlessConnection,
            AmbientExtraLoud,
            AmbientVoiceFocus,
            AmbientSidetone,
            AmbientPassthrough,
            AmbientSound,
            Anc,
            NoiseControl,
            DetectConversations,
            GamingMode,
            DoubleTapVolume,
            CaseBattery,
            FragmentedMessages,
            StereoPan
        }
        
        public Dictionary<Feature, FeatureRule?> Rules { get; }
        public Models Device { get; }
        public string DeviceBaseName { get; }
        public ITouchOption TouchMap { get; }
        public Guid ServiceUuid { get; }
        public IReadOnlyCollection<ItemType> TrayShortcuts { get; }
        
        public bool Supports(Feature feature)
        {
            if (!Rules.ContainsKey(feature))
            {
                return false;
            }

            if (Rules[feature] == null)
            {
                return true;
            }

            if (DeviceMessageCache.Instance.ExtendedStatusUpdate?.Revision == null)
            {
                Log.Warning("IDeviceSpec: Cannot compare revision. No ExtendedStatusUpdate cached.");
                return true;
            }

            return DeviceMessageCache.Instance.ExtendedStatusUpdate.Revision >= Rules[feature]?.MinimumRevision;
        }

        public string RecommendedFwVersion(Feature feature)
        {
            return Rules.ContainsKey(feature) ? Rules[feature]?.RecommendedFirmwareVersion ?? "---" : "???";
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
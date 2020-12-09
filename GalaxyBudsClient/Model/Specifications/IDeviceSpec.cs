using System.Collections.Generic;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
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
    
    public interface IDeviceSpec
    {
        enum Feature
        {
            SeamlessConnection,
            AmbientExtraLoud,
            AmbientVoiceFocus,
            AmbientSidetone,
            AmbientPassthrough,
            GamingMode,
        }
        
        Dictionary<Feature, FeatureRule?> Rules { get; }

        bool Supports(Feature feature)
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

        string RecommendedFwVersion(Feature feature)
        {
            return Rules.ContainsKey(feature) ? Rules[feature]?.RecommendedFirmwareVersion ?? "---" : "???";
        }
    }
}
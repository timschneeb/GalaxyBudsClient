using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;
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
            AmbientSound,
            GamingMode,
            DoubleTapVolume,
            CaseBattery
        }
        
        Dictionary<Feature, FeatureRule?> Rules { get; }
        Models Device { get; }

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
        
        public ITouchOption TouchMap
        {
            get
            {
                switch (Device)
                {
                    case Models.Buds:
                        return new BudsTouchOption();
                    case Models.BudsPlus:
                        return new BudsPlusTouchOption();
                    case Models.BudsLive:
                        return new BudsLiveTouchOption();
                    default:
                        return new StubTouchOption();
                }
            }
        }
    }
}
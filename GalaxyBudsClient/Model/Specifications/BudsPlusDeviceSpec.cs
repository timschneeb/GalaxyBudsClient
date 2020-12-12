using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsPlusDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.AmbientSound, null },
                { IDeviceSpec.Feature.AmbientSidetone, new FeatureRule(8, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.AmbientExtraLoud, new FeatureRule(9, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(11, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.CaseBattery, null },
            };
        
        public Models Device => Models.BudsPlus;
    }
}
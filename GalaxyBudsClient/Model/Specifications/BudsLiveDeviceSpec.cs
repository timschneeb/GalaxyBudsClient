using System;
using System.Collections.Generic;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsLiveDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(11, "R180XXU0ATF2")  },
                { IDeviceSpec.Feature.AmbientPassthrough, null },
                { IDeviceSpec.Feature.GamingMode, null }
            };
    }
}
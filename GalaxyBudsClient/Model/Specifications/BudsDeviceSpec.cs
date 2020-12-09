using System;
using System.Collections.Generic;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(3, "R170XXU0ATF2") },
                { IDeviceSpec.Feature.AmbientVoiceFocus, null },
            };
    }
}
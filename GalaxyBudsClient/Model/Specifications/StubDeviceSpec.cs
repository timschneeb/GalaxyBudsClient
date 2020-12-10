using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using Serilog;

namespace GalaxyBudsClient.Model.Specifications
{
    public class StubDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>();

        public StubDeviceSpec()
        {
            Log.Warning("StubDeviceSpec: initialized");
        }
        
        public Models Device => Models.NULL;
    }
}
using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;
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
        public string DeviceBaseName => "STUB_DEVICE";
        public ITouchOption TouchMap => new StubTouchOption();
        public Guid ServiceUuid => new Guid("{00000000-0000-0000-0000-000000000000}");
    }
}
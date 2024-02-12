using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(3, "R170XXU0ATF2") },
                { IDeviceSpec.Feature.AmbientVoiceFocus, null },
                { IDeviceSpec.Feature.AmbientSound, null },
                { IDeviceSpec.Feature.LegacyNoiseControlMode, null },
                { IDeviceSpec.Feature.DebugInfoLegacy, null },
                { IDeviceSpec.Feature.Voltage, null },
                { IDeviceSpec.Feature.Current, null }
            };

        public Models Device => Models.Buds;
        public string DeviceBaseName => "Galaxy Buds (";
        public ITouchOption TouchMap => new BudsTouchOption();
        public Guid ServiceUuid => Uuids.Buds;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            new [] {
                ItemType.ToggleAmbient,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            }
        );
        
        public string IconResourceKey => "Bud";
    }
}
using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsLiveDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(3, "R180XXU0ATF2")  },
                { IDeviceSpec.Feature.AmbientPassthrough, null },
                { IDeviceSpec.Feature.Anc, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.StereoPan, new FeatureRule(7, "R180XXU0AUB5") },
                { IDeviceSpec.Feature.BixbyWakeup, null },
                { IDeviceSpec.Feature.FirmwareUpdates, null },
                { IDeviceSpec.Feature.LegacyNoiseControlMode, null },
                { IDeviceSpec.Feature.DebugInfoLegacy, null },
                { IDeviceSpec.Feature.Voltage, null },
                { IDeviceSpec.Feature.DebugSku, null }
            };
        
        public Models Device => Models.BudsLive;
        public string DeviceBaseName => "Buds Live";
        public ITouchOption TouchMap => new BudsLiveTouchOption();
        public Guid ServiceUuid => Uuids.BudsLive;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            new[] {
                ItemType.ToggleAnc,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            }
        );
        
        public string IconResourceKey => "Bean";
    }
}
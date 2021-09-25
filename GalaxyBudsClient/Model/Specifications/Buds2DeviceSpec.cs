using System;
using System.Collections.Generic;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class Buds2DeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, null },
                { IDeviceSpec.Feature.StereoPan, null},
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.FirmwareUpdates, null },
                { IDeviceSpec.Feature.NoiseControl, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.BixbyWakeup, null },
                { IDeviceSpec.Feature.DoubleTapVolume, new FeatureRule(5, "R177XXU0AUI2") },
                { IDeviceSpec.Feature.AdvancedTouchLock, new FeatureRule(4, "R177XXU0AUH1") },
                { IDeviceSpec.Feature.AncWithOneEarbud, new FeatureRule(5, "R177XXU0AUH1") },
                { IDeviceSpec.Feature.AmbientCustomize, new FeatureRule(5, "R177XXU0AUH1") },
                { IDeviceSpec.Feature.AmbientSidetone, new FeatureRule(6, "R177XXU0AUI2")  },
            };
        
        public Models Device => Models.Buds2;
        public string DeviceBaseName => "Galaxy Buds2 (";
        public ITouchOption TouchMap => new Buds2TouchOption();
        public Guid ServiceUuid => Uuids.Buds2;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            new[] {
                ItemType.ToggleNoiseControl,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            }
        );
        
        public string IconResourceKey => "Pro";
    }
}
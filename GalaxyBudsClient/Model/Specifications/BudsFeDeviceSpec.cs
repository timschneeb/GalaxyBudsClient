using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsFeDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, null },
                { IDeviceSpec.Feature.StereoPan, null },
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.FirmwareUpdates, null },
                { IDeviceSpec.Feature.NoiseControl, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.GearFitTest, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.BixbyWakeup, null },
                { IDeviceSpec.Feature.AncNoiseReductionLevels, null },
                { IDeviceSpec.Feature.AmbientSidetone, null },
                { IDeviceSpec.Feature.AmbientCustomize, null },
                { IDeviceSpec.Feature.AncWithOneEarbud, null }
            };
        
        public Models Device => Models.BudsFe;
        public string DeviceBaseName => "Buds FE";
        public ITouchOption TouchMap => new BudsFeTouchOption();
        public Guid ServiceUuid => Uuids.BudsFe;

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
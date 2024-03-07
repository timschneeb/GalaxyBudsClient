using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class Buds2ProDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules => new()
            {
                { IDeviceSpec.Feature.SeamlessConnection, null },
                { IDeviceSpec.Feature.StereoPan, null },
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.FirmwareUpdates, null },
                { IDeviceSpec.Feature.DetectConversations, null },
                { IDeviceSpec.Feature.NoiseControl, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
                { IDeviceSpec.Feature.SpatialSensor, null },
                { IDeviceSpec.Feature.BixbyWakeup, null },
                { IDeviceSpec.Feature.GearFitTest, null },
                { IDeviceSpec.Feature.AncNoiseReductionLevels, null },
                { IDeviceSpec.Feature.AmbientSidetone, null },
                { IDeviceSpec.Feature.AmbientCustomize, null },
                /*{ IDeviceSpec.Feature.AncWithOneEarbud, null },
                 TODO: conflicts with AncNoiseReductionLevels and overlaps in UI due to lack of space
                 This will be fixed with the UI rewrite
                  */
                { IDeviceSpec.Feature.DebugSku, null }
            };
        
        public Models Device => Models.Buds2Pro;
        public string DeviceBaseName => "Buds2 Pro";
        public ITouchOption TouchMap => new StandardTouchOption();
        public Guid ServiceUuid => Uuids.Buds2Pro;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            [
                ItemType.ToggleNoiseControl,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Pro";
    }
}
using System;
using System.Collections.Generic;

using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsProDeviceSpec : IDeviceSpec
    {
        public Dictionary<Features, FeatureRule?> Rules => new()
            {
                { Features.SeamlessConnection, null },
                { Features.StereoPan, new FeatureRule(5, "R190XXU0AUA5") },
                { Features.DoubleTapVolume, new FeatureRule(7, "R190XXU0AUD1") },
                { Features.FirmwareUpdates, null },
                { Features.DetectConversations, null },
                { Features.NoiseControl, null },
                { Features.GamingMode, null },
                { Features.CaseBattery, null },
                { Features.FragmentedMessages, null },
                { Features.SpatialSensor, null },
                { Features.Voltage, null },
                { Features.BixbyWakeup, null },
                { Features.AmbientSound, null },
                { Features.Anc, null },
                { Features.AncNoiseReductionLevels, null },
                { Features.BuildInfo, null },
                { Features.AmbientSidetone, new FeatureRule(8, "R190XXU0AUI2")  },
                { Features.AmbientCustomize, new FeatureRule(8, "R190XXU0AUI2") },
                { Features.AmbientCustomizeLegacy, new FeatureRule(8, "R190XXU0AUI2") },
                { Features.DebugSku, null }
            };
        
        public Models Device => Models.BudsPro;
        public string DeviceBaseName => "Buds Pro";
        public ITouchOption TouchMap => new StandardTouchOption();
        public Guid ServiceUuid => Uuids.BudsPro;

        public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
            [
                TrayItemTypes.ToggleNoiseControl,
                TrayItemTypes.ToggleEqualizer,
                TrayItemTypes.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Pro";
        public int MaximumAmbientVolume => 3;
    }
}
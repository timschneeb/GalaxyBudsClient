using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsFeDeviceSpec : IDeviceSpec
    {
        public Dictionary<Features, FeatureRule?> Rules => new()
            {
                { Features.SeamlessConnection, null },
                { Features.StereoPan, null },
                { Features.DoubleTapVolume, null },
                { Features.FirmwareUpdates, null },
                { Features.NoiseControl, null },
                { Features.GamingMode, null },
                { Features.CaseBattery, null },
                { Features.GearFitTest, null },
                { Features.FragmentedMessages, null },
                { Features.BixbyWakeup, null },
                { Features.AmbientSound, null },
                { Features.Anc, null },
                { Features.AncNoiseReductionLevels, null },
                { Features.AmbientSidetone, null },
                { Features.AmbientCustomize, null },
                { Features.AncWithOneEarbud, null },
                { Features.DebugSku, null }
            };
        
        public Models Device => Models.BudsFe;
        public string DeviceBaseName => "Buds FE";
        public ITouchOption TouchMap => new StandardTouchOption();
        public Guid ServiceUuid => Uuids.BudsFe;

        public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
            [
                TrayItemTypes.ToggleNoiseControl,
                TrayItemTypes.ToggleEqualizer,
                TrayItemTypes.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Pro";
        public int MaximumAmbientVolume => 2;
    }
}
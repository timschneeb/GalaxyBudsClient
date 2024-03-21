using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsPlusDeviceSpec : IDeviceSpec
    {
        public Dictionary<Features, FeatureRule?> Rules => new()
            {
                { Features.AmbientSound, null },
                { Features.AmbientSidetone, new FeatureRule(8, "R175XXU0ASLE")  },
                { Features.AmbientExtraLoud, new FeatureRule(9, "R175XXU0ATB3")  },
                { Features.SeamlessConnection, new FeatureRule(11, "R175XXU0ATF2")  },
                { Features.FirmwareUpdates, new FeatureRule(8, "R175XXU0ASLE") },
                { Features.GamingMode, null },
                { Features.DoubleTapVolume, null },
                { Features.CaseBattery, null },
                { Features.FragmentedMessages, null },
                { Features.BuildInfo, null },
                { Features.Voltage, null }
            };
        
        public Models Device => Models.BudsPlus;
        public string DeviceBaseName => "Galaxy Buds+ (";
        public ITouchOption TouchMap => new BudsPlusTouchOption();
        public Guid ServiceUuid => Uuids.BudsPlus;

        public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
            [
                TrayItemTypes.ToggleAmbient,
                TrayItemTypes.ToggleEqualizer,
                TrayItemTypes.LockTouchpad
            ]
        );
        
        public string IconResourceKey => "Bud";
        public int MaximumAmbientVolume => 2; /* 3 if ExtraLoud is set */
    }
}
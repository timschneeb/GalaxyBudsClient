using System;
using System.Collections.Generic;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsPlusDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.AmbientSound, null },
                { IDeviceSpec.Feature.AmbientSidetone, new FeatureRule(8, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.AmbientExtraLoud, new FeatureRule(9, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(11, "R175XXU0ATF2")  },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.DoubleTapVolume, null },
                { IDeviceSpec.Feature.CaseBattery, null },
            };
        
        public Models Device => Models.BudsPlus;
        public string DeviceBaseName => "Galaxy Buds+ (";
        public ITouchOption TouchMap => new BudsPlusTouchOption();
        public Guid ServiceUuid => Uuids.BudsPlus;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            new[] {
                ItemType.ToggleAmbient,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            }
        );
    }
}
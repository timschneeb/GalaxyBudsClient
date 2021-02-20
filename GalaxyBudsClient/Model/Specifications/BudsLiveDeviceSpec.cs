using System;
using System.Collections.Generic;
using GalaxyBudsClient.Interop.TrayIcon;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Touchpad;

namespace GalaxyBudsClient.Model.Specifications
{
    public class BudsLiveDeviceSpec : IDeviceSpec
    {
        public Dictionary<IDeviceSpec.Feature, FeatureRule?> Rules =>
            new Dictionary<IDeviceSpec.Feature, FeatureRule?>()
            {
                { IDeviceSpec.Feature.SeamlessConnection, new FeatureRule(11, "R180XXU0ATF2")  },
                { IDeviceSpec.Feature.AmbientPassthrough, null },
                { IDeviceSpec.Feature.Anc, null },
                { IDeviceSpec.Feature.GamingMode, null },
                { IDeviceSpec.Feature.CaseBattery, null },
                { IDeviceSpec.Feature.FragmentedMessages, null },
            };
        
        public Models Device => Models.BudsLive;
        public string DeviceBaseName => "Galaxy Buds Live (";
        public ITouchOption TouchMap => new BudsLiveTouchOption();
        public Guid ServiceUuid => Uuids.BudsLive;

        public IReadOnlyCollection<ItemType> TrayShortcuts => Array.AsReadOnly(
            new[] {
                ItemType.ToggleAnc,
                ItemType.ToggleEqualizer,
                ItemType.LockTouchpad
            }
        );
    }
}
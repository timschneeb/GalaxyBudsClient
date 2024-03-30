using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsPlusDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.AmbientSound, null },
        { Features.AmbientSidetone, new FeatureRule(8)  },
        { Features.AmbientExtraLoud, new FeatureRule(9)  },
        { Features.SeamlessConnection, new FeatureRule(11)  },
        { Features.FirmwareUpdates, new FeatureRule(8) },
        { Features.GamingMode, null },
        { Features.DoubleTapVolume, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.BuildInfo, null },
        { Features.Voltage, null },
        { Features.CallPathControl, new FeatureRule(13) },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null }
    };
        
    public Models Device => Models.BudsPlus;
    public string DeviceBaseName => "Galaxy Buds+ (";
    public ITouchMap TouchMap => new BudsPlusTouchMap();
    public Guid ServiceUuid => Uuids.BudsPlus;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleAmbient,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 2; /* 3 if ExtraLoud is set */
}
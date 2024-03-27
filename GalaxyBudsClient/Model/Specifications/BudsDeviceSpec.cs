using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, new FeatureRule(3, "R170XXU0ATF2") },
        { Features.AmbientVoiceFocus, null },
        { Features.AmbientSound, null },
        { Features.LegacyAmbientSoundVolumeLevels, null },
        { Features.BuildInfo, null },
        { Features.BatteryType, null },
        { Features.Voltage, null },
        { Features.Current, null },
        { Features.PairingMode, null }
    };

    public Models Device => Models.Buds;
    public string DeviceBaseName => "Galaxy Buds (";
    public ITouchMap TouchMap => new BudsTouchMap();
    public Guid ServiceUuid => Uuids.Buds;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleAmbient,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 4;
}
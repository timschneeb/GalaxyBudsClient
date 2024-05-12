using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, new FeatureRule(3) },
        { Features.AmbientVoiceFocus, null },
        { Features.AmbientSound, null },
        { Features.LegacyAmbientSoundVolumeLevels, null },
        { Features.BuildInfo, null },
        { Features.BatteryType, null },
        { Features.Voltage, null },
        { Features.Current, null },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null },
        { Features.SppLegacyMessageHeader, null },
        { Features.UsageReport, null },
    };

    public Models Device => Models.Buds;
    public string DeviceBaseName => "Galaxy Buds (";
    public ITouchMap TouchMap => new BudsTouchMap();
    public Guid ServiceUuid => Uuids.SppLegacy;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleAmbient,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 4;
    public byte StartOfMessage => (byte)MsgConstants.LegacySom;
    public byte EndOfMessage => (byte)MsgConstants.LegacyEom;
}
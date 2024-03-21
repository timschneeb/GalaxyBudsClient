using System;
using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class Buds2ProDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, null },
        { Features.StereoPan, null },
        { Features.DoubleTapVolume, null },
        { Features.FirmwareUpdates, null },
        { Features.DetectConversations, null },
        { Features.NoiseControl, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.SpatialSensor, null },
        { Features.BixbyWakeup, null },
        { Features.GearFitTest, null },
        { Features.AmbientSound, null },
        { Features.Anc, null },
        { Features.AncNoiseReductionLevels, null },
        { Features.AmbientSidetone, null },
        { Features.AmbientCustomize, null },
        { Features.AncWithOneEarbud, null },
        { Features.DebugSku, null }
    };
        
    public Models Device => Models.Buds2Pro;
    public string DeviceBaseName => "Buds2 Pro";
    public ITouchOption TouchMap => new StandardTouchOption();
    public Guid ServiceUuid => Uuids.Buds2Pro;

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
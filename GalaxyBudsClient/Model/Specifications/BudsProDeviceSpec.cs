using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsProDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, null },
        { Features.StereoPan, new FeatureRule(5) },
        { Features.DoubleTapVolume, new FeatureRule(7) },
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
        { Features.AmbientSidetone, new FeatureRule(8)  },
        { Features.AmbientCustomize, new FeatureRule(8) },
        { Features.AmbientCustomizeLegacy, new FeatureRule(8) },
        { Features.DebugSku, null },
        { Features.CallPathControl, new FeatureRule(10) },
        { Features.NoiseControlModeDualSide, new FeatureRule(8) },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null },
        { Features.AncWithOneEarbud, new FeatureRule(8) },
        { Features.DeviceColor, null }
    };
        
    public Models Device => Models.BudsPro;
    public string DeviceBaseName => "Buds Pro";
    public ITouchMap TouchMap => new StandardTouchMap();
    public Guid ServiceUuid => Uuids.BudsPro;
    public Guid? AltUuid => Uuids.BudsProAlternative;
    public byte? StartOfMessageAlt => 252;
    public byte? EndOfMessageAlt => 204;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleNoiseControl,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 3;
    public byte StartOfMessage => (byte)MsgConstants.Som;
    public byte EndOfMessage => (byte)MsgConstants.Eom;
}
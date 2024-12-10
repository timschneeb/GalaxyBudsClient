using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class Buds3DeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, null },
        { Features.StereoPan, null},
        { Features.FirmwareUpdates, null },
        { Features.NoiseControl, null },
        { Features.AmbientSound, null },
        { Features.Anc, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.BixbyWakeup, null },
        { Features.GearFitTest, null },
        { Features.DoubleTapVolume, null },
        { Features.AdvancedTouchLock, null },
        { Features.AdvancedTouchLockForCalls, null },
        { Features.NoiseControlsWithOneEarbud, null },
        { Features.AmbientCustomize, null },
        { Features.AmbientSidetone, null },
        { Features.FmgRingWhileWearing, null },
        { Features.DebugSku, null },
        { Features.CallPathControl, null },
        { Features.ChargingState, null },
        { Features.NoiseControlModeDualSide, null },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null },
        { Features.DeviceColor, null },
        { Features.Rename, null },
        { Features.SpatialSensor, null  },
        { Features.SmartThingsFind, null },
        { Features.UsageReport, null },
        { Features.HotCommandLanguageUpdate, new FeatureRule(2) }
    };

    public Models Device => Models.Buds3;
    public string DeviceBaseName => "Buds3";
    public ITouchMap TouchMap => new StandardTouchMap();
    public Guid ServiceUuid => Uuids.SppNew;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleNoiseControl,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 2;
    public byte StartOfMessage => (byte)MsgConstants.Som;
    public byte EndOfMessage => (byte)MsgConstants.Eom;
}
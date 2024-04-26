using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

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
        { Features.DebugSku, null },
        { Features.AdvancedTouchLock, null },
        { Features.AdvancedTouchLockForCalls, null },
        { Features.CallPathControl, null },
        { Features.FmgRingWhileWearing, null },
        { Features.ChargingState, null },
        { Features.NoiseControlModeDualSide, null },
        { Features.DeviceColor, null },
        { Features.Rename, null }
    };
        
    public Models Device => Models.BudsFe;
    public string DeviceBaseName => "Buds FE";
    public ITouchMap TouchMap => new StandardTouchMap();
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
    public byte StartOfMessage => (byte)MsgConstants.Som;
    public byte EndOfMessage => (byte)MsgConstants.Eom;
}
using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class Buds3ProDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, null },
        { Features.StereoPan, null },
        { Features.DoubleTapVolume, null },
        { Features.FirmwareUpdates, null },
        { Features.DetectConversations, null },
        { Features.NoiseControl, null },
        { Features.NoiseControlModeDualSide, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.SpatialSensor, null },
        { Features.BixbyWakeup, null },
        { Features.GearFitTest, null },
        { Features.ExtraClearCallSound, null },
        { Features.AmbientExtraLoud, null },
        { Features.AmbientSound, null },
        { Features.Anc, null },
        { Features.AmbientSidetone, null },
        { Features.AmbientCustomize, null },
        { Features.NoiseControlsWithOneEarbud, null },
        { Features.DebugSku, null },
        { Features.AdvancedTouchLock, null },
        { Features.AdvancedTouchLockForCalls, null },
        { Features.FmgRingWhileWearing, null },
        { Features.CallPathControl, null },
        { Features.ChargingState, null },
        { Features.AutoAdjustSound, null },
        { Features.HeadTracking, null },
        { Features.CradleSerialNumber, null },
        { Features.DeviceColor, null },
        { Features.Rename, null },
        { Features.SmartThingsFind, null },
        { Features.UsageReport, null },
        { Features.HotCommandLanguageUpdate, new FeatureRule(2) }
    };
        
    public Models Device => Models.Buds3Pro;
    public string DeviceBaseName => "Buds3 Pro";
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
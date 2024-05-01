using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
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
        { Features.NoiseControlModeDualSide, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.SpatialSensor, null },
        { Features.BixbyWakeup, null },
        { Features.GearFitTest, null },
        { Features.ExtraClearCallSound, new FeatureRule(13) },
        { Features.AmbientExtraLoud, new FeatureRule(13) },
        { Features.AmbientSound, null },
        { Features.Anc, null },
        { Features.AmbientSidetone, new FeatureRule(1) },
        { Features.AmbientCustomize, null },
        { Features.NoiseControlsWithOneEarbud, null },
        { Features.DebugSku, null },
        { Features.AdvancedTouchLock, null },
        { Features.AdvancedTouchLockForCalls, new FeatureRule(1) },
        { Features.FmgRingWhileWearing, new FeatureRule(4) },
        { Features.CallPathControl, new FeatureRule(1) },
        { Features.ChargingState, new FeatureRule(11, 1) },
        { Features.AutoAdjustSound, new FeatureRule(3) },
        { Features.HeadTracking, new FeatureRule(8) },
        { Features.CradleSerialNumber, null },
        { Features.DeviceColor, null },
        { Features.Rename, null },
        { Features.SmartThingsFind, null }
    };
        
    public Models Device => Models.Buds2Pro;
    public string DeviceBaseName => "Buds2 Pro";
    public ITouchMap TouchMap => new StandardTouchMap();
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
    public byte StartOfMessage => (byte)MsgConstants.Som;
    public byte EndOfMessage => (byte)MsgConstants.Eom;
}
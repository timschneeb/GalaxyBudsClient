using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class BudsLiveDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new()
    {
        { Features.SeamlessConnection, new FeatureRule(3)  },
        { Features.AmbientPassthrough, new FeatureRule(5) },
        { Features.Anc, null },
        { Features.GamingMode, null },
        { Features.CaseBattery, null },
        { Features.FragmentedMessages, null },
        { Features.StereoPan, new FeatureRule(7) },
        { Features.SpatialSensor, new FeatureRule(9) },
        { Features.BixbyWakeup, new FeatureRule(1) },
        { Features.FirmwareUpdates, null },
        { Features.BuildInfo, null },
        { Features.Voltage, null },
        { Features.DebugSku, null },
        { Features.CallPathControl, new FeatureRule(8) },
        { Features.PairingMode, null },
        { Features.AmbientSoundVolume, null },
        { Features.DeviceColor, null },
        { Features.SmartThingsFind, new FeatureRule(4) },
        { Features.UsageReport, null },
        { Features.HiddenAtMode, null }
    };
        
    public Models Device => Models.BudsLive;
    public string DeviceBaseName => "Buds Live";
    public ITouchMap TouchMap => new BudsLiveTouchMap();
    public Guid ServiceUuid => Uuids.SppStandard;

    public IEnumerable<TrayItemTypes> TrayShortcuts => Array.AsReadOnly(
        [
            TrayItemTypes.ToggleAnc,
            TrayItemTypes.ToggleEqualizer,
            TrayItemTypes.LockTouchpad
        ]
    );
        
    public string IconResourceKey => "Bean";
    public int MaximumAmbientVolume => 0; /* ambient sound unsupported */
    public byte StartOfMessage => (byte)MsgConstants.Som;
    public byte EndOfMessage => (byte)MsgConstants.Eom;
}
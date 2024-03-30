using System;
using System.Collections.Generic;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Model.Specifications.Touch;

namespace GalaxyBudsClient.Model.Specifications;

public class StubDeviceSpec : IDeviceSpec
{
    public Dictionary<Features, FeatureRule?> Rules => new();
    public Models Device => Models.NULL;
    public string DeviceBaseName => "STUB_DEVICE";
    public ITouchMap TouchMap => new StubTouchMap();
    public Guid ServiceUuid => new("{00000000-0000-0000-0000-000000000000}");
    public IEnumerable<TrayItemTypes> TrayShortcuts => new List<TrayItemTypes>();
    public string IconResourceKey => "Pro";
    public int MaximumAmbientVolume => 3;
    public byte StartOfMessage => (byte)SppMessage.MsgConstants.Som;
    public byte EndOfMessage => (byte)SppMessage.MsgConstants.Eom;
}
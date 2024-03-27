using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications.Touch;

public class StubTouchMap : ITouchMap
{
    public Dictionary<TouchOptions, byte> LookupTable => new();
}
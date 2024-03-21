using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications.Touch
{
    public class StubTouchOption : ITouchOption
    {
        public Dictionary<TouchOptions, byte> LookupTable => new();
    }
}
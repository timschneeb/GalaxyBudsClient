using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Touchpad
{
    public class StubTouchOption : ITouchOption
    {
        public Dictionary<TouchOptions, byte> LookupTable => new Dictionary<TouchOptions, byte>();
    }
}
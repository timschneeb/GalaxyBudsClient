using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Touchpad
{
    public class BudsProTouchOption : ITouchOption
    {
        public Dictionary<TouchOptions, byte> LookupTable => new Dictionary<TouchOptions, byte>()
        {
            {TouchOptions.VoiceAssistant, 1},
            {TouchOptions.NoiseControl, 2},
            {TouchOptions.Volume, 3},
            {TouchOptions.SpotifySpotOn, 4},
            {TouchOptions.OtherL, 5},
            {TouchOptions.OtherR, 6},
        };
    }
}
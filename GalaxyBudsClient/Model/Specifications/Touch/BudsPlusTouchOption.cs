using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications.Touch;

public class BudsPlusTouchOption : ITouchOption
{
    public Dictionary<TouchOptions, byte> LookupTable => new()
    {
        {TouchOptions.VoiceAssistant, 1},
        {TouchOptions.AmbientSound, 2},
        {TouchOptions.Volume, 3},
        {TouchOptions.SpotifySpotOn, 4},
        {TouchOptions.OtherL, 5},
        {TouchOptions.OtherR, 6}
    };
}
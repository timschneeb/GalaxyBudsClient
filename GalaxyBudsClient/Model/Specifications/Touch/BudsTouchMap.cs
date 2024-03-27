using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Model.Specifications.Touch;

public class BudsTouchMap : ITouchMap
{
    public Dictionary<TouchOptions, byte> LookupTable => new()
    {
        {TouchOptions.VoiceAssistant, 0},
        {TouchOptions.QuickAmbientSound, 1},
        {TouchOptions.Volume, 2},
        {TouchOptions.AmbientSound, 3},
        {TouchOptions.SpotifySpotOn, 4},
        {TouchOptions.OtherL, 5},
        {TouchOptions.OtherR, 6}
    };
}
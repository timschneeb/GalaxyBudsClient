using System.Collections.Generic;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientToneConverter : IntToStringConverter
{
    // TODO: handle localization changes

    protected override Dictionary<int, string> Mapping => new()
    {
        { 0, Loc.Resolve("nc_as_custom_tone_soft") + " +2" },
        { 1, Loc.Resolve("nc_as_custom_tone_soft") + " +1" },
        { 2, Loc.Resolve("nc_as_custom_tone_neutral") },
        { 3, Loc.Resolve("nc_as_custom_tone_clear") + " +1" },
        { 4, Loc.Resolve("nc_as_custom_tone_clear") + " +2" }
    };
}
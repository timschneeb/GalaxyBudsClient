using System.Collections.Generic;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientCustomStrengthConverter() : IntToStringConverter
{
    // TODO: handle localization changes
    protected override Dictionary<int, string> Mapping => new()
    {
        { 0, "-2" },
        { 1, "-1" },
        { 2, Loc.Resolve("nc_as_custom_vol_normal") },
        { 3, "+1" },
        { 4, "+2" }
    };
}
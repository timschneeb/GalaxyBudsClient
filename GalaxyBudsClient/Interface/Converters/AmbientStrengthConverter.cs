using System.Collections.Generic;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientStrengthConverter(bool legacy) : IntToStringConverter
{
    // TODO: handle localization changes
    private static Dictionary<int, string> LegacyScale => new()
    {
        { 0, Loc.Resolve("as_scale_very_low") },
        { 1, Loc.Resolve("as_scale_low") },
        { 2, Loc.Resolve("as_scale_moderate") },
        { 3, Loc.Resolve("as_scale_high") },
        { 4, Loc.Resolve("as_scale_extraloud") }
    };

    private static Dictionary<int, string> Scale => new()
    {
        { 0, Loc.Resolve("as_scale_low") },
        { 1, Loc.Resolve("as_scale_moderate") },
        { 2, Loc.Resolve("as_scale_high") },
        { 3, Loc.Resolve("as_scale_extraloud") }
    };

    protected override Dictionary<int, string> Mapping => legacy ? LegacyScale : Scale;
}
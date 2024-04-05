using System.Collections.Generic;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Utils.Interface;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientCustomStrengthConverter : IntToStringConverter
{
    protected override Dictionary<int, string> Mapping => new()
    {
        { 0, "-2" },
        { 1, "-1" },
        { 2, Strings.NcAsCustomVolNormal },
        { 3, "+1" },
        { 4, "+2" }
    };
}
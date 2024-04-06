using System.Collections.Generic;
using GalaxyBudsClient.Generated.I18N;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientToneConverter : IntToStringConverter
{
    protected override Dictionary<int, string> Mapping => new()
    {
        { 0, Strings.NcAsCustomToneSoft + " +2" },
        { 1, Strings.NcAsCustomToneSoft + " +1" },
        { 2, Strings.NcAsCustomToneNeutral },
        { 3, Strings.NcAsCustomToneClear + " +1" },
        { 4, Strings.NcAsCustomToneClear + " +2" }
    };
}
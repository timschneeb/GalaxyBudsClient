using System.Collections.Generic;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;
using GalaxyBudsClient.Utils.Interface.DynamicLocalization;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientStrengthConverter : IntToStringConverter
{
    private bool _legacy;
        
    public AmbientStrengthConverter()
    {
        SelectScale();
        BluetoothService.Instance.PropertyChanged += (_, _) => SelectScale();
    }

    private void SelectScale()
    {
        _legacy = BluetoothService.ActiveModel == Models.Buds;
    }
    
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

    protected override Dictionary<int, string> Mapping => _legacy ? LegacyScale : Scale;
}
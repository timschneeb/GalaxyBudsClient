using System.Collections.Generic;
using GalaxyBudsClient.Generated.I18N;
using GalaxyBudsClient.Model.Constants;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Interface.Converters;

public class AmbientStrengthConverter : IntToStringConverter
{
    private bool _legacy;
        
    public AmbientStrengthConverter()
    {
        SelectScale();
        BluetoothImpl.Instance.PropertyChanged += (_, _) => SelectScale();
    }

    private void SelectScale()
    {
        _legacy = BluetoothImpl.ActiveModel == Models.Buds;
    }
    
    private static Dictionary<int, string> LegacyScale => new()
    {
        { 0, Strings.AsScaleVeryLow },
        { 1, Strings.AsScaleLow },
        { 2, Strings.AsScaleModerate },
        { 3, Strings.AsScaleHigh },
        { 4, Strings.AsScaleExtraloud }
    };

    private static Dictionary<int, string> Scale => new()
    {
        { 0, Strings.AsScaleLow },
        { 1, Strings.AsScaleModerate },
        { 2, Strings.AsScaleHigh },
        { 3, Strings.AsScaleExtraloud }
    };

    protected override Dictionary<int, string> Mapping => _legacy ? LegacyScale : Scale;
}
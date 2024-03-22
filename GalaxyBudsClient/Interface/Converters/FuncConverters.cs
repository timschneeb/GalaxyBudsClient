using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Converters;

public static class FuncConverters
{
    public static readonly IValueConverter IsBlurDarkModeSet =
        new FuncValueConverter<DarkModes, bool>(x => x == DarkModes.Dark);
}
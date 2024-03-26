using Avalonia.Data.Converters;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.Converters;

public static class FuncConverters
{
    public static readonly IValueConverter IsBlurDarkModeSet =
        new FuncValueConverter<Themes, bool>(x => x is Themes.DarkBlur or Themes.DarkMica);
}
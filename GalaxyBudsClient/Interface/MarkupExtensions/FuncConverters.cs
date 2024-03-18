using System.Collections;
using Avalonia.Data.Converters;
using FluentAvalonia.Core;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public static class FuncConverters
{
    public static readonly IValueConverter IsGreaterThanZero =
        new FuncValueConverter<IEnumerable?, bool>(x => x != null && x.Count() > 0);
    
    public static readonly IValueConverter IsZero =
        new FuncValueConverter<IEnumerable?, bool>(x => x == null || x.Count() == 0);
}
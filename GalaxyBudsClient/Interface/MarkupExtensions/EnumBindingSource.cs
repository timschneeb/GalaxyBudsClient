using System;
using System.Linq;
using Avalonia.Markup.Xaml;
using CommandLine;
using GalaxyBudsClient.Generated.Enums;
using GalaxyBudsClient.Model.Attributes;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class EnumBindingSource : MarkupExtension
{
    private readonly Type? _enumType;
    public Type? EnumType
    {
        get => _enumType;
        init
        {
            if (value == _enumType) 
                return;
            
            if (null != value)
            {
                var enumType = Nullable.GetUnderlyingType(value) ?? value;

                if (!enumType.IsEnum)
                    throw new ArgumentException("Type must be for an Enum.");
            }

            _enumType = value;
        }
    }
    
    public EnumBindingSource(Type enumType)
    {
        EnumType = enumType;
    }
    
    public override object ProvideValue(IServiceProvider? serviceProvider)
    {
        return CompiledEnums.GetValuesByType(EnumType!)
            .Cast<int[]>()
            .Select(x => (Enum)(object)x)
            .Where(obj => obj.IsPlatformConditionMet())
            .Where(obj => !obj.IsMemberIgnored())
            .ToArray();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Avalonia.Markup.Xaml;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Interface.MarkupExtensions;

public class EnumBindingSource : MarkupExtension
{
    private Type? _enumType;
    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;

                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be for an Enum.");
                }

                _enumType = value;
            }
        }
    }
    
    public EnumBindingSource(Type enumType)
    {
        EnumType = enumType;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) // or IXamlServiceProvider for UWP and WinUI
    {
        var values = new List<object>();
        foreach (var obj in Enum.GetValues(EnumType!))
        {
            if (obj == null) 
                continue;
                
            if (EnumType!.GetMember(EnumType.GetEnumName((int) obj) ?? string.Empty)[0]
                    .GetCustomAttributes(typeof(IgnoreDataMemberAttribute), false)
                    .Length > 0)
            {
                continue;
            }
            
            values.Add(obj);
        }
        return values.ToArray();
    }
}
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace GalaxyBudsClient.Model.Attributes
{
    public static class DescriptionAttributeExtension
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is not Enum) 
                return string.Empty;
            
            var type = e.GetType();
            foreach (var obj in Enum.GetValues(type))
            {
                if (obj == null || (int)obj != e.ToInt32(CultureInfo.InvariantCulture)) 
                    continue;
                
                if (type.GetMember(type.GetEnumName((int) obj) ?? string.Empty)[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                {
                    return descriptionAttribute.Description;
                }
            }

            return string.Empty;
        }
        
        public static string GetLocalizableKey<T>(this T e) where T : IConvertible
        {
            if (e is not Enum) 
                return string.Empty;
            
            var type = e.GetType();
            foreach (var obj in Enum.GetValues(type))
            {
                if (obj == null || (int)obj != e.ToInt32(CultureInfo.InvariantCulture)) 
                    continue;
                
                if (type.GetMember(type.GetEnumName((int) obj) ?? string.Empty)[0]
                        .GetCustomAttributes(typeof(LocalizedDescriptionAttribute), false)
                        .FirstOrDefault() is LocalizedDescriptionAttribute descriptionAttribute)
                {
                    return descriptionAttribute.Key;
                }
            }

            return string.Empty;
        }
    }
}

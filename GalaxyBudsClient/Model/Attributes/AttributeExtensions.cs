using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace GalaxyBudsClient.Model.Attributes
{
    public static class AttributeExtensions
    {
        private static TAttr? GetEnumAttribute<TAttr,TEnum>(this TEnum e) where TAttr : Attribute where TEnum : struct, IConvertible
        { 
            if(e is not Enum)
                throw new ArgumentException("Enum type expected");
            
            var type = e.GetType();
            foreach (var obj in Enum.GetValues(type))
            {
                if (obj == null || (int)obj != e.ToInt32(CultureInfo.InvariantCulture)) 
                    continue;
                
                if (type.GetMember(type.GetEnumName((int) obj) ?? string.Empty)[0]
                        .GetCustomAttributes(typeof(TAttr), false)
                        .FirstOrDefault() is TAttr attr)
                {
                    return attr;
                }
            }

            return null;
        }

        public static string GetDescription<T>(this T e) where T : struct, IConvertible
            => e.GetEnumAttribute<DescriptionAttribute, T>()?.Description ?? string.Empty;

        public static string GetLocalizableKey<T>(this T e) where T : struct, IConvertible
            => e.GetEnumAttribute<LocalizedDescriptionAttribute, T>()?.Key ?? string.Empty;
        
        public static bool IsHidden<T>(this T e) where T : struct, IConvertible
            => e.GetEnumAttribute<HiddenAttribute, T>() != null;
        
        public static ModelMetadataAttribute? GetModelMetadata<T>(this T e) where T : struct, IConvertible
            => e.GetEnumAttribute<ModelMetadataAttribute, T>();
    }
}

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace GalaxyBudsClient.Model.Attributes
{
    public static class AttributeExtensions
    {
        private static TAttr? GetEnumAttribute<TAttr,TEnum>(this TEnum e) where TAttr : Attribute where TEnum : IConvertible
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

        public static string GetDescription<T>(this T e) where T : IConvertible
            => e.GetEnumAttribute<DescriptionAttribute, T>()?.Description ?? string.Empty;

        public static string GetLocalizableKey<T>(this T e) where T : IConvertible
            => e.GetEnumAttribute<LocalizedDescriptionAttribute, T>()?.Key ?? string.Empty;
        
        public static bool IsMemberIgnored<T>(this T e) where T : IConvertible
            => e.GetEnumAttribute<IgnoreDataMemberAttribute, T>() != null;
        
        public static ModelMetadataAttribute? GetModelMetadata<T>(this T e) where T : IConvertible
            => e.GetEnumAttribute<ModelMetadataAttribute, T>();
    }
}

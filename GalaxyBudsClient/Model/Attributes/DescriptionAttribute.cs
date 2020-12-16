using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace GalaxyBudsClient.Model.Attributes
{
    static class DescriptionAttributeExtension
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                foreach (var obj in Enum.GetValues(type))
                {
                    if (obj is {} val)
                    {
                        if ((int) val == e.ToInt32(CultureInfo.InvariantCulture))
                        {
                            var memInfo = type.GetMember(type.GetEnumName((int) val) ?? string.Empty);

                            if (memInfo[0]
                                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                            {
                                return descriptionAttribute.Description;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}

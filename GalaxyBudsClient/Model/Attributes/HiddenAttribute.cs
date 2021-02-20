using System;
using System.Globalization;
using System.Linq;

namespace GalaxyBudsClient.Model.Attributes
{
    class HiddenAttribute : Attribute
    {
    }
    
    static class HiddenAttributeExtension
    {
        public static bool IsHidden<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (var val in values)
                {
                    if (val == null)
                    {
                        continue;
                    }
                    
                    if ((int)val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val) ?? string.Empty);

                        if (memInfo[0]
                            .GetCustomAttributes(typeof(HiddenAttribute), false)
                            .FirstOrDefault() is HiddenAttribute hiddenAttribute)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

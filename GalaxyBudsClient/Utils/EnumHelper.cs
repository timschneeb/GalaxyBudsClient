using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyBudsClient.Utils
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetUniqueFlags<T>(this Enum flags)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("The generic type parameter must be an Enum.");

            if (flags.GetType() != typeof(T))
                throw new ArgumentException("The generic type parameter does not match the target type.");

            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value as Enum))
                {
                    yield return value;
                }
            }
        }
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}

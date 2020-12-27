using System;
using System.Collections.Generic;
using System.Linq;
using Config.Net;
using Serilog;

namespace GalaxyBudsClient.Model
{
    public class ConfigArrayParser<T> : ITypeParser
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(T[]) };

        public string? ToRawString(object? value)
        {
            if (value is T[] enumerable)
            {
                return string.Join(';', enumerable);
            }
            return null;
        }

        public bool TryParse(string? value, Type t, out object? result)
        {
            if(string.IsNullOrEmpty(value))
            {
                result = new T[0];
                return false;
            }

            if(t == typeof(T[]))
            {
                try
                {
                    result = value
                        .Split(';')
                        .Select(long.Parse)
                        .ToArray();
                }
                catch (FormatException ex)
                {
                    Log.Warning($"ConfigArrayParser<{typeof(T)}>: FormatException raised ({ex.Message})");
                    result = new T[0];
                    return false;
                }

                return true;
            }

            result = new T[0];
            return false;
        }
    }
}
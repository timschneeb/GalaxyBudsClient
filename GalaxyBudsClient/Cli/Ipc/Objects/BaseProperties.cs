using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyBudsClient.Cli.Ipc.Objects;

public abstract class BaseProperties
{
    /// <summary>
    /// Gets all properties by its key using reflection
    /// </summary>
    /// <returns>Property map</returns>
    internal IDictionary<string, object> GetAll()
    {
        return GetType()
                   .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Select(x => new KeyValuePair<string, object?>(StripFieldName(x), x.GetValue(this)))
                   .Where(kv => kv.Value != null)
                   .Cast<KeyValuePair<string, object>>()
                   .ToDictionary();
    } 
    
    /// <summary>
    /// Gets a property by its key using reflection
    /// </summary>
    /// <param name="prop">Property key</param>
    /// <returns>Object with requested key</returns>
    /// <exception cref="System.ArgumentException">Thrown if the key does not exist</exception>
    internal object Get(string prop)
    {
        return GetType()
                   .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .FirstOrDefault(f => CompareKeys(prop, f))?
                   .GetValue(this)
               ?? throw new ArgumentException("Property not found", prop);
    }

    /// <summary>
    /// Sets a property to a value by its key using reflection
    /// </summary>
    /// <param name="prop">Property key</param>
    /// <param name="val">New value</param>
    /// <returns>True if the value has been modified, otherwise false</returns>
    /// <exception cref="System.ArgumentException">Thrown if the key does not exist</exception>
    internal bool Set(string prop, object val)
    {
        var oldValue = Get(prop);
            
        GetType()
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => CompareKeys(prop, f))?
            .SetValue(this, val);
        
        // Has value changed?
        return val switch
        {
            // Handle special cases
            double d => Math.Abs((double)oldValue - d) > 0.0001,
            int i => (int)oldValue != i,
            string s => (string)oldValue != s,
            _ => oldValue != val
        };
    }
    
    private static bool CompareKeys(string key, MemberInfo field)
    {
        // We may have to remove the first character of the property name because it is a private field and starts with an underscore
        return key == field.Name || key == StripFieldName(field);
    }

    private static string StripFieldName(MemberInfo field) => field.Name.Remove(0, 1);
}
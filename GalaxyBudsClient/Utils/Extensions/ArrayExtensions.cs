using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyBudsClient.Utils.Extensions;

public static class ArrayExtensions
{
    public static T[] Add<T>(this T[]? a, T n)
    {
        if (a == null)
        {
            return [n];
        }
            
        T[] newArray = new T[a.Length + 1];
        a.CopyTo(newArray, 0);
        newArray[^1] = n;
        return newArray;
    }
        
    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if( index > 0 )
            Array.Copy(source, 0, dest, 0, index);

        if( index < source.Length - 1 )
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }
        
    public static T[] Remove<T>(this IEnumerable<T>? source, T obj)
    {
        var list = source?.ToList();
        list?.Remove(obj);
        return list?.ToArray() ?? Array.Empty<T>();
    }
        
    public static int? FindIndex<T>(this IEnumerable<T>? source, T obj)
    {
        var list = source?.ToList();
        return list?.IndexOf(obj);
    }
}
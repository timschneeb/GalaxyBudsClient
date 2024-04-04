namespace GalaxyBudsClient.Utils.Extensions;

public static class ArrayExtensions
{
    public static T[] Add<T>(this T[]? a, T n)
    {
        if (a == null)
        {
            return [n];
        }
            
        var newArray = new T[a.Length + 1];
        a.CopyTo(newArray, 0);
        newArray[^1] = n;
        return newArray;
    }
}
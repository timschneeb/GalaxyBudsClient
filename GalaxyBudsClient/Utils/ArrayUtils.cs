namespace GalaxyBudsClient.Utils
{
    public static class ArrayUtils
    {
        public static T[] Add<T>(T[]? a, T n)
        {
            if (a == null)
            {
                return new T[1] {n};
            }
            
            T[] newArray = new T[a.Length + 1];
            a.CopyTo(newArray, 0);
            newArray[^1] = n;
            return newArray;
        }
    }
}
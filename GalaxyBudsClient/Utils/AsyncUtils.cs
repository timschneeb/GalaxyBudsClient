using System.Threading.Tasks;

namespace GalaxyBudsClient.Utils
{
    public static class AsyncUtils
    {
        public static T RunSync<T>(this Task<T> task)
        {
            var wrapper = Task.Run(async () => await task);
            if (wrapper.IsFaulted && wrapper.Exception != null)
            {
                throw wrapper.Exception;
            }

            return wrapper.Result;
        }
    }
}
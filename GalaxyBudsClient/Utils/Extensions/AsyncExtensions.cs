using System.Threading.Tasks;

namespace GalaxyBudsClient.Utils.Extensions;

public static class AsyncExtensions
{
    public static T WaitAndReturnResult<T>(this Task<T> task)
    {
        task.Wait();
        return task.Result;
    }
}
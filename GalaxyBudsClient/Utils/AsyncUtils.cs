using System.Diagnostics;
using System.Threading;
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
        
        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
        /// immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsync(this Process process, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object?>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if(cancellationToken != default(CancellationToken))
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
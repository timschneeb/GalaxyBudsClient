using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace GalaxyBudsClient.Utils
{
    public static class DialogUtils
    {
        public static void ShowDialogSync(this Window window, Window? parent = null)
        {
            if (parent is null) parent = window;
            using (var source = new CancellationTokenSource())
            {
                window.ShowDialog(parent).ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
                Dispatcher.UIThread.MainLoop(source.Token);
            }
        }

        public static T ShowDialogSync<T>(this Window window, Window? parent = null)
        {
            if (parent is null) parent = window;
            using (var source = new CancellationTokenSource())
            {
                var task = window.ShowDialog<T>(parent);
                task.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
                Dispatcher.UIThread.MainLoop(source.Token);
                return task.Result;
            }

            return default(T);
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Ghostly.Uwp
{
    internal static class CoreDispatcherExtensions
    {
        public static void FireAndForgetSafe(this CoreDispatcher dispatcher, Action action)
        {
            if (dispatcher != null && action != null)
            {
                try
                {
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        action();
                    }).AsTask().FireAndForgetSafeAsync();
                }
                catch
                {
                }
            }
        }

        public static async Task<T> RunTaskAsync<T>(
            this CoreDispatcher dispatcher,
            Func<Task<T>> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        // There is no TaskCompletionSource<void> so we use a bool that we throw away.
        public static async Task RunTaskAsync(
            this CoreDispatcher dispatcher,
            Func<Task> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            await RunTaskAsync(dispatcher, async () =>
            {
                await func();
                return false;
            }, priority);
        }
    }
}

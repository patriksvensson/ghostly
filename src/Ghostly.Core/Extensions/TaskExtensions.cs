using System;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;

namespace Ghostly
{
    public static class TaskUtilities
    {
        public static void FireAndForgetSafe(this Task task, IGhostlyLog log, string message = null)
        {
            task.FireAndForgetSafeAsync(new LoggingErrorHandler(log, message));
        }

        public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler handler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }
    }
}

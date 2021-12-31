using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;

namespace Ghostly.Uwp.Utilities
{
    internal static class ExtendedExecutionHelper
    {
        private static ExtendedExecutionSession session;
        private static int taskCount;

        public static bool IsRunning
        {
            get
            {
                if (session != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static async Task<ExtendedExecutionResult> RequestSessionAsync(
            ExtendedExecutionReason reason,
            TypedEventHandler<object, ExtendedExecutionRevokedEventArgs> revoked,
            string description)
        {
            if (taskCount > 0)
            {
                return ExtendedExecutionResult.Allowed;
            }

            var newSession = new ExtendedExecutionSession();
            newSession.Reason = reason;
            newSession.Description = description;
            newSession.Revoked += SessionRevoked;

            // Add a revoked handler provided by the app in order to clean up an operation that had to be halted prematurely
            if (revoked != null)
            {
                newSession.Revoked += revoked;
            }

            var result = await newSession.RequestExtensionAsync();

            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    session = newSession;
                    break;
                default:
                case ExtendedExecutionResult.Denied:
                    newSession.Dispose();
                    break;
            }

            return result;
        }

        public static void ClearSession()
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }

            taskCount = 0;
        }

        public static Deferral GetExecutionDeferral()
        {
            if (session == null)
            {
                throw new InvalidOperationException("No extended execution session is active");
            }

            taskCount++;
            return new Deferral(OnTaskCompleted);
        }

        private static void OnTaskCompleted()
        {
            if (taskCount > 0)
            {
                taskCount--;
            }

            if (taskCount == 0 && session != null)
            {
                ClearSession();
            }
        }

        private static void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            if (session != null)
            {
                session.Dispose();
                session = null;
            }

            taskCount = 0;
        }
    }
}

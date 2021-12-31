using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Windows.ApplicationModel;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpStartupHelper : IStartupHelper
    {
        private readonly string _taskId;

        public UwpStartupHelper()
        {
#if DEBUG
            _taskId = "Ghostly_Dev";
#else
            _taskId = "Ghostly";
#endif
        }

        public async Task<StartupState> GetState()
        {
            try
            {
                var task = await StartupTask.GetAsync(_taskId);
                switch (task.State)
                {
                    case StartupTaskState.Disabled:
                        return StartupState.Disabled;
                    case StartupTaskState.DisabledByPolicy:
                        return StartupState.DisabledByPolicy;
                    case StartupTaskState.DisabledByUser:
                        return StartupState.DisabledByUser;
                    case StartupTaskState.Enabled:
                        return StartupState.Enabled;
                    case StartupTaskState.EnabledByPolicy:
                        return StartupState.EnabledByPolicy;
                    default:
                        return StartupState.Error;
                }
            }
            catch
            {
                return StartupState.Error;
            }
        }

        public async Task<bool> Enable()
        {
            var task = await StartupTask.GetAsync(_taskId);
            if (task.State == StartupTaskState.Disabled)
            {
                var newState = await task.RequestEnableAsync();
                return newState == StartupTaskState.Enabled;
            }

            return false;
        }

        public async Task<bool> Disable()
        {
            var task = await StartupTask.GetAsync(_taskId);
            if (task.State == StartupTaskState.Enabled)
            {
                task.Disable();
                return (await GetState()) == StartupState.Disabled;
            }

            return false;
        }
    }
}

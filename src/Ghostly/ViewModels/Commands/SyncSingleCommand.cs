using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels.Commands
{
    public sealed class SyncSingleCommand : AsyncCommand<Notification>
    {
        private readonly ISynchronizationService _synchronizer;

        public SyncSingleCommand(ISynchronizationService synchronizer)
        {
            _synchronizer = synchronizer;
        }

        protected override bool CanExecuteCommand(Notification parameter)
        {
            return _synchronizer.CanSynchronize && !_synchronizer.IsSynchronizing;
        }

        protected override async Task ExecuteCommand(Notification parameter)
        {
            if (parameter != null)
            {
                await _synchronizer.Trigger(parameter, CancellationToken.None);
            }
        }
    }
}

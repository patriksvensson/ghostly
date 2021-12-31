using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Services;

namespace Ghostly.ViewModels.Commands
{
    public sealed class SyncCommand : AsyncCommand
    {
        private readonly ISynchronizationService _synchronizer;

        public SyncCommand(ISynchronizationService synchronizer)
        {
            _synchronizer = synchronizer;
        }

        protected override bool CanExecuteCommand()
        {
            return _synchronizer.CanSynchronize && !_synchronizer.IsSynchronizing;
        }

        protected override async Task ExecuteCommand()
        {
            await _synchronizer.Trigger();
        }
    }
}

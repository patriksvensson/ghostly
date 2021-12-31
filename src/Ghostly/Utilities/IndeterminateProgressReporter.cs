using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;

namespace Ghostly.Utilities
{
    public sealed class IndeterminateProgressReporter : ProgressReporter
    {
        public IndeterminateProgressReporter(IMessageService messenger)
            : base(messenger)
        {
        }

        public override async Task ShowProgress(string message)
        {
            await Messenger.PublishAsync(new ShowProgress(message, null)).ConfigureAwait(false);
        }
    }
}

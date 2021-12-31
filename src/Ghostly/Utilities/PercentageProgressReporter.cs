using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain.Messages;

namespace Ghostly.Utilities
{
    public sealed class PercentageProgressReporter : ProgressReporter
    {
        public int Count { get; }
        public int Processed { get; private set; }
        public int Percent { get; private set; }

        public PercentageProgressReporter(IMessageService messenger, int count)
            : base(messenger)
        {
            Count = count;
            Processed = 0;
            Percent = 0;
        }

        public void Increment(int count = 1)
        {
            Processed = Math.Min(Processed++, Count);
            Percent = (int)(((double)Processed / count) * 100D);
        }

        public override async Task ShowProgress(string message)
        {
            await Messenger.PublishAsync(new ShowProgress(message, Percent));
        }
    }
}

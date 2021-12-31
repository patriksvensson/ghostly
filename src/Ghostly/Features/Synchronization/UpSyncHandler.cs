using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain.Messages;
using Ghostly.Features.Activities;
using MediatR;

namespace Ghostly.Features.Synchronization
{
    public sealed class UpSyncHandler : GhostlyRequestHandler<UpSyncHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ActivityHelper _activityHelper;
        private readonly ILocalizer _localizer;

        public UpSyncHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ActivityHelper activityHelper,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
            _activityHelper = activityHelper ?? throw new System.ArgumentNullException(nameof(activityHelper));
            _localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));
        }

        public sealed class Request : IRequest
        {
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var shouldSave = false;

                try
                {
                    var activities = GetActivities(context).ToList();
                    if (activities.Count > 0)
                    {
                        await _messenger.PublishAsync(new StatusMessage(_localizer.GetString("Synchronization_Local_Status"), null));
                    }

                    var processed = 0;
                    foreach (var activity in activities.Take(100))
                    {
                        var payload = _activityHelper.Deserialize(activity.Kind, activity.Payload);
                        if (payload != null)
                        {
                            if (await _activityHelper.Process(payload, cancellationToken))
                            {
                                // Remove the activity.
                                shouldSave = true;
                                context.Activities.Remove(activity);
                            }
                        }

                        // Wait for a little bit.
                        await Task.Delay(150, cancellationToken);

                        // Update the status.
                        processed += 1;
                        var percentage = (int)(((double)processed / (double)activities.Count) * 100D);
                        await _messenger.PublishAsync(new StatusMessage(_localizer.GetString("Synchronization_Local_Status"), percentage));
                    }
                }
                finally
                {
                    if (shouldSave)
                    {
                        // Save changes.
                        context.SaveChanges();
                    }
                }
            }
        }

        private IEnumerable<ActivityData> GetActivities(GhostlyContext context)
        {
            return context.Activities
                .Where(x => x.Category == ActivityCategory.Synchronization)
                .OrderBy(x => x.Timestamp);
        }
    }
}

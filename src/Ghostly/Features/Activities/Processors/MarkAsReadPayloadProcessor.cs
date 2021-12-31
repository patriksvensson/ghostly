using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Activities.Payloads;
using Ghostly.Features.Notifications;
using MediatR;

namespace Ghostly.Features.Activities.Processors
{
    public sealed class MarkAsReadPayloadProcessor : ActivityPayloadProcessor<MarkAsReadPayload>
    {
        private readonly List<IVendor> _vendors;
        private readonly IGhostlyContextFactory _factory;
        private readonly IMediator _mediator;
        private readonly IGhostlyLog _log;

        public override ActivityKind Kind => ActivityKind.MarkAsRead;

        public MarkAsReadPayloadProcessor(
            IGhostlyContextFactory factory,
            IMediator mediator,
            IEnumerable<IVendor> vendors,
            IGhostlyLog log)
        {
            _vendors = new List<IVendor>(vendors);
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _log = log ?? throw new System.ArgumentNullException(nameof(log));
        }

        protected override async Task<bool> Process(MarkAsReadPayload payload)
        {
            using (var context = _factory.Create())
            {
                var notification = await _mediator.Send(new GetNotificationHandler.Request(context, payload.NotificationId));
                if (notification == null)
                {
                    _log.Error("Could not find notification #{NotificationId}.", payload.NotificationId);
                    return false;
                }

                // Call the vendor.
                var vendor = _vendors.SingleOrDefault(v => v.Matches(payload.Vendor));
                if (vendor != null)
                {
                    await vendor.MarkAsRead(notification);
                }

                return true;
            }
        }
    }
}

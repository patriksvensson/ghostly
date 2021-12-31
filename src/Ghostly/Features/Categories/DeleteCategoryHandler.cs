using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Domain;
using Ghostly.Features.Notifications;
using MediatR;

namespace Ghostly.Features.Categories
{
    public sealed class DeleteCategoryHandler : GhostlyRequestHandler<DeleteCategoryHandler.Request, bool>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;

        public class Request : IRequest<bool>
        {
            public Category Category { get; }

            public Request(Category category)
            {
                Category = category ?? throw new ArgumentNullException(nameof(category));
            }
        }

        public DeleteCategoryHandler(
            IGhostlyContextFactory factory,
            IMediator mediator,
            ITelemetry telemetry)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        }

        public override async Task<bool> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                // Move notifications in the category
                // we're updating to the inbox.
                await _mediator.Send(new MoveNotificationsToInboxHandler.Request(request.Category.Id)
                {
                    Context = context,
                    SaveChanges = false,
                }, cancellationToken);

                // Delete the category.
                var categoryToDelete = context.Categories.Find(request.Category.Id);
                context.Categories.Remove(categoryToDelete);

                // Save changes.
                await context.SaveChangesAsync(cancellationToken);

                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.DeletedCategory);
            }

            return true;
        }
    }
}

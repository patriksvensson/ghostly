using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Notifications;
using Ghostly.Features.Querying;
using MediatR;

namespace Ghostly.Features.Categories
{
    public sealed class UpdateCategoryHandler : GhostlyRequestHandler<UpdateCategoryHandler.Request, Category>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        public UpdateCategoryHandler(
            IGhostlyContextFactory factory,
            IMediator mediator,
            ITelemetry telemetry,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public sealed class Request : IRequest<Category>
        {
            public int CategoryId { get; }
            public string Name { get; }
            public string Expression { get; }
            public string Emoji { get; }
            public bool ShowTotal { get; set; }
            public bool Muted { get; set; }
            public bool IncludeMuted { get; set; }

            public int? SortOrder { get; set; }
            public string ImportedFrom { get; set; }

            public Request(
                int categoryId, string name, string expression,
                string emoji, bool showTotal, bool muted, bool includeMuted)
            {
                CategoryId = categoryId;
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Expression = expression;
                Emoji = emoji;
                ShowTotal = showTotal;
                Muted = muted;
                IncludeMuted = includeMuted;

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Name cannot be empty or null.", nameof(name));
                }
            }
        }

        public override async Task<Category> Handle(Request request, CancellationToken cancellationToken)
        {
            var categoryId = request.CategoryId;
            var name = request.Name;
            var expression = request.Expression;
            var emoji = request.Emoji;

            // Validate the expression if there is one.
            Expression<Func<NotificationData, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(expression))
            {
                if (!GhostlyQueryLanguage.TryCompile(expression, out filter, out var error))
                {
                    _log.Error("Could not compile expression: {ExpressionError}", error);
                    return null;
                }
            }

            // Update the category.
            using (var context = _factory.Create())
            {
                var data = context.Find<CategoryData>(categoryId);
                if (data == null)
                {
                    throw new InvalidOperationException("Could not find category.");
                }

                // Was this a catgory but now it's a filter?
                if (data.Kind == CategoryKind.Category && !string.IsNullOrWhiteSpace(expression))
                {
                    // Move notifications in the category
                    // we're updating to the inbox.
                    await _mediator.Send(new MoveNotificationsToInboxHandler.Request(categoryId)
                    {
                        Context = context,
                        SaveChanges = false,
                    }, cancellationToken);
                }

                data.Name = name;
                data.Expression = expression;
                data.Glyph = string.IsNullOrWhiteSpace(expression) ? Constants.Glyphs.Category : Constants.Glyphs.Filter;
                data.Emoji = emoji;
                data.ShowTotal = request.ShowTotal;
                data.Muted = request.Muted;
                data.IncludeMuted = request.IncludeMuted;
                data.Kind = string.IsNullOrWhiteSpace(expression) ? CategoryKind.Category : CategoryKind.Filter;
                data.SortOrder = request.SortOrder ?? data.SortOrder;

                if (string.IsNullOrWhiteSpace(request.ImportedFrom))
                {
                    data.ImportedFrom = request.ImportedFrom;
                }

                context.Categories.Update(data);
                await context.SaveChangesAsync(cancellationToken);

                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.UpdatedCategory);

                return CategoryMapper.Map(data, _localizer);
            }
        }
    }
}

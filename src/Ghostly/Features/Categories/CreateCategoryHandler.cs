using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Querying;
using MediatR;

namespace Ghostly.Features.Categories
{
    public sealed class CreateCategoryHandler : GhostlyRequestHandler<CreateCategoryHandler.Request, Category>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        public CreateCategoryHandler(
            IGhostlyContextFactory factory,
            ITelemetry telemetry,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _factory = factory;
            _telemetry = telemetry;
            _localizer = localizer;
            _log = log;
        }

        public sealed class Request : IRequest<Category>
        {
            public string Name { get; }
            public string Expression { get; }
            public string Emoji { get; }
            public bool ShowTotal { get; set; }
            public bool Muted { get; set; }
            public bool IncludeMuted { get; set; }

            public string Identifier { get; set; }
            public int? SortOrder { get; set; }
            public string ImportedFrom { get; set; }

            public Request(
                string name, string expression, string emoji,
                bool showTotal, bool muted, bool includeMuted)
            {
                Name = name;
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
            var name = request.Name;
            var expression = request.Expression;
            var emoji = request.Emoji;
            var showTotal = request.ShowTotal;
            var muted = request.Muted;
            var includeMuted = request.IncludeMuted;

            // Validate the expression if there is one.
            if (!string.IsNullOrWhiteSpace(expression))
            {
                if (!GhostlyQueryLanguage.TryCompile(expression, out var _, out var error))
                {
                    _log.Error("Could not compile expression: {ExpressionError}", error);
                    return null;
                }
            }

            using (var context = _factory.Create())
            {
                var data = new CategoryData
                {
                    Archive = false,
                    Inbox = false,
                    Identifier = request.Identifier.OrIfNullOrWhiteSpace(Guid.NewGuid().ToGhostlyFormat()),
                    Deletable = true,
                    Glyph = string.IsNullOrWhiteSpace(expression) ? Constants.Glyphs.Category : Constants.Glyphs.Filter,
                    Emoji = emoji,
                    Expression = expression,
                    ShowTotal = showTotal,
                    Muted = muted,
                    IncludeMuted = includeMuted,
                    Kind = string.IsNullOrWhiteSpace(expression) ? CategoryKind.Category : CategoryKind.Filter,
                    Name = name.Trim(),
                    SortOrder = request.SortOrder ?? int.MaxValue,
                    ImportedFrom = request.ImportedFrom,
                    ImportedAt = !string.IsNullOrWhiteSpace(request.ImportedFrom) ? DateTime.UtcNow : (DateTime?)null,
                };

                context.Categories.Add(data);
                await context.SaveChangesAsync(cancellationToken);

                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.CreatedCategory);

                return CategoryMapper.Map(data, _localizer);
            }
        }
    }
}

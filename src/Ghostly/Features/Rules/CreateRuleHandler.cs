using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Querying;
using MediatR;

namespace Ghostly.Features.Rules
{
    public sealed class CreateRuleHandler : GhostlyRequestHandler<CreateRuleHandler.Request, Rule>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        public sealed class Request : IRequest<Rule>
        {
            public NewRuleModel Model { get; set; }

            public Request(NewRuleModel model)
            {
                Model = model ?? throw new ArgumentNullException(nameof(model));
            }
        }

        public CreateRuleHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ITelemetry telemetry,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _factory = factory;
            _messenger = messenger;
            _telemetry = telemetry;
            _localizer = localizer;
            _log = log;
        }

        public override async Task<Rule> Handle(Request request, CancellationToken cancellationToken)
        {
            var model = request.Model;

            // Validate the expression if there is one.
            if (!GhostlyQueryLanguage.TryCompile(model.Expression, out var filter, out var error))
            {
                throw new InvalidOperationException($"Could not compile expression: {error}");
            }

            using (var context = _factory.Create())
            {
                var highestSortOrder = 0;
                if (context.Rules.Any())
                {
                    highestSortOrder = context.Rules.Max(s => s.SortOrder);
                }

                var data = new RuleData
                {
                    Name = model.Name,
                    Identifier = model.Identifier.OrIfNullOrWhiteSpace(Guid.NewGuid().ToGhostlyFormat()),
                    Enabled = true,
                    SortOrder = highestSortOrder + 1,
                    Expression = model.Expression,
                    Star = model.Star,
                    Mute = model.Mute,
                    MarkAsRead = model.MarkAsRead,
                    StopProcessing = model.StopProcessing,
                    ImportedFrom = model.ImportedFrom,
                    ImportedAt = !string.IsNullOrWhiteSpace(model.ImportedFrom) ? DateTime.UtcNow : (DateTime?)null,
                };

                if (model.CategoryId != null)
                {
                    var category = context.Categories.Find(model.CategoryId.Value);
                    if (category != null)
                    {
                        data.Category = category;
                    }
                }
                else if (model.CategoryIdentifier != null)
                {
                    var category = context.Categories.FirstOrDefault(c => c.Identifier == model.CategoryIdentifier);
                    if (category != null)
                    {
                        data.Category = category;
                    }
                }

                context.Rules.Add(data);
                await context.SaveChangesAsync(cancellationToken);

                // Notify subscribers that there is a new rule.
                var rule = RuleMapper.Map(data, _localizer);
                await _messenger.PublishAsync(new RuleCreated(rule));

                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.CreatedRule);

                // Return the rule
                return rule;
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Querying;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Rules
{
    public sealed class UpdateRuleHandler : GhostlyRequestHandler<UpdateRuleHandler.Request, Rule>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ITelemetry _telemetry;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;

        public sealed class Request : IRequest<Rule>
        {
            public EditRuleModel Model { get; }

            public Request(EditRuleModel model)
            {
                Model = model ?? throw new ArgumentNullException(nameof(model));
            }
        }

        public UpdateRuleHandler(
            IGhostlyContextFactory factory,
            ITelemetry telemetry,
            IMessageService messenger,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
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
                var data = await context.Rules.Include(r => r.Category).SingleOrDefaultAsync(r => r.Id == model.Id, cancellationToken);
                if (data == null)
                {
                    throw new InvalidOperationException("Could not find rule.");
                }

                data.Name = model.Name;
                data.Expression = model.Expression;
                data.Star = model.Star;
                data.Mute = model.Mute;
                data.MarkAsRead = model.MarkAsRead;
                data.StopProcessing = model.StopProcessing;

                if (!string.IsNullOrWhiteSpace(model.ImportedFrom))
                {
                    data.ImportedFrom = model.ImportedFrom;
                    data.ImportedAt = DateTime.UtcNow;
                }

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
                else
                {
                    data.Category = null;
                }

                context.Rules.Update(data);
                await context.SaveChangesAsync(cancellationToken);

                // Notify subscribers that there is a new rule.
                var rule = RuleMapper.Map(data, _localizer);
                await _messenger.PublishAsync(new RuleUpdated(rule));

                // Track event.
                _telemetry.TrackEvent(Constants.TrackingEvents.UpdatedRule);

                // Return the rule
                return rule;
            }
        }
    }
}
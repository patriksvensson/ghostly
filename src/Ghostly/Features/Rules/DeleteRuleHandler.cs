using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Domain.Messages;
using MediatR;

namespace Ghostly.Features.Rules
{
    public sealed class DeleteRuleHandler : GhostlyRequestHandler<DeleteRuleHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public sealed class Request : IRequest
        {
            public int RuleId { get; }

            public Request(int ruleId)
            {
                RuleId = ruleId;
            }
        }

        public DeleteRuleHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
            _telemetry = telemetry ?? throw new System.ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new System.ArgumentNullException(nameof(log));
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                // Delete the rule.
                var rule = context.Rules.Find(request.RuleId);
                if (rule == null)
                {
                    _log.Warning("Could not delete rule with ID {RuleId} since it could not be found.", request.RuleId);
                    return;
                }

                context.Rules.Remove(rule);
                await context.SaveChangesAsync(cancellationToken);

                _log.Verbose("Deleted rule {RuleName} with ID {RuleId}.", rule.Name, rule.Id);
            }

            // Notify subscribers about the rule being deleted.
            await _messenger.PublishAsync(new RuleDeleted(request.RuleId));

            // Track event.
            _telemetry.TrackEvent(Constants.TrackingEvents.DeletedRule);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Rules
{
    public sealed class ProcessCategoryRuleHandler : GhostlyRequestHandler<ProcessCategoryRuleHandler.Request>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        public sealed class Request : IRequest
        {
            public Category Category { get; }
            public Rule Rule { get; set; }

            public Request(Category category)
            {
                Category = category;
            }

            public Request(Category category, Rule rule)
            {
                Category = category;
                Rule = rule;
            }
        }

        public ProcessCategoryRuleHandler(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ITelemetry telemetry,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new System.ArgumentNullException(nameof(messenger));
            _telemetry = telemetry ?? throw new System.ArgumentNullException(nameof(telemetry));
            _localizer = localizer ?? throw new System.ArgumentNullException(nameof(localizer));
            _log = log ?? throw new System.ArgumentNullException(nameof(log));
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            await _messenger.PublishAsync(new RuleStateChanged(false));

            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                // Track event.
                _telemetry.TrackEvent(request.Rule != null
                    ? Constants.TrackingEvents.ExecutedSingleRule
                    : Constants.TrackingEvents.ExecutedRules);

                using (var context = _factory.Create())
                {
                    var rules = RuleMapper.Map(context.Rules.Include(r => r.Category), _localizer);

                    try
                    {
                        if (request.Rule != null)
                        {
                            _log.Verbose("Running rule {RuleName} for category {CategoryName}...", request.Rule.Name, request.Category.Name);
                            await progress.ShowProgress(_localizer.Format("RuleProcessor_Progress_Single", request.Rule.Name, request.Category.Name));
                        }
                        else
                        {
                            _log.Verbose("Running rules for category {CategoryName}...", request.Category.Name);
                            await progress.ShowProgress(_localizer.Format("RuleProcessor_Progress", request.Category.Name));
                        }

                        await Task.Delay(1500, cancellationToken);
                        var doNotProcess = new HashSet<int>();

                        foreach (var rule in rules.OrderBy(r => r.SortOrder))
                        {
                            // Skip this rule?
                            if (request.Rule != null && request.Rule.Id != rule.Id)
                            {
                                continue;
                            }

                            _log.Verbose("Running rule {RuleName}...", rule.Name);

                            var candidates = context.GetNotificationQuery()
                                .Where(n => n.Category.Id == request.Category.Id)
                                .Where(rule.Filter);

                            var hasChanges = false;
                            foreach (var candidate in candidates)
                            {
                                if (doNotProcess.Contains(candidate.Id))
                                {
                                    continue;
                                }

                                if (rule.StopProcessing)
                                {
                                    doNotProcess.Add(candidate.Id);
                                }

                                if (UpdateNotification(context, rule, candidate))
                                {
                                    _log.Debug("Rule {RuleName} matched notification {NotificationId}", rule.Name, candidate.Id);
                                    hasChanges = true;
                                }
                            }

                            if (hasChanges)
                            {
                                // Save changes and update unread count.
                                await context.SaveChangesAsync(true, cancellationToken);
                                _messenger.Publish(new UpdateUnreadCount());
                            }
                        }
                    }
                    finally
                    {
                        _log.Verbose("Done running rules.");
                        await _messenger.PublishAsync(new RuleStateChanged(true));
                    }

                    // Wait a little bit longer.
                    await Task.Delay(500, cancellationToken);
                }
            }
        }

        private static bool UpdateNotification(GhostlyContext context, Rule rule, NotificationData notification)
        {
            var changed = false;

            if (rule.MarkAsRead && notification.Unread)
            {
                notification.Unread = false;
                changed = true;
            }

            if (rule.Mute && !notification.Muted)
            {
                notification.Muted = true;
                changed = true;
            }

            if (rule.Star && !notification.Starred)
            {
                notification.Starred = true;
                changed = true;
            }

            if (rule.Category != null)
            {
                if (notification.Category == null || notification.Category.Id != rule.Category.Id)
                {
                    notification.Category = context.Categories.Find(rule.Category.Id);
                    changed = true;
                }
            }

            return changed;
        }
    }
}

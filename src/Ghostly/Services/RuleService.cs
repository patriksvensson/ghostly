using System;
using System.Collections.Generic;
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
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Services
{
    public interface IRuleService
    {
        Task<(NotificationData Notification, bool Success)> Process(GhostlyContext context, NotificationData notification);
    }

    [DependentOn(typeof(DatabaseInitializer))]
    public sealed class RuleService : IRuleService, IInitializable, IDisposable
    {
        private readonly List<Rule> _rules;
        private readonly SemaphoreSlim _semaphore;
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;

        private sealed class CompiledRule
        {
            public Rule Rule { get; }
            public int SortOrder => Rule.SortOrder;
            public Func<NotificationData, bool> Predicate { get; }

            public CompiledRule(Rule rule, Func<NotificationData, bool> predicate)
            {
                Rule = rule;
                Predicate = predicate;
            }
        }

        public RuleService(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            ILocalizer localizer,
            IGhostlyLog log)
        {
            _rules = new List<Rule>();
            _semaphore = new SemaphoreSlim(1, 1);
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public async Task<bool> Initialize(bool background)
        {
            await UpdateRules();

            _messenger.SubscribeAsync<RuleCreated>(async message => await UpdateRules());
            _messenger.SubscribeAsync<RuleUpdated>(async message => await UpdateRules());
            _messenger.SubscribeAsync<RuleDeleted>(async message => await UpdateRules());

            return true;
        }

        public async Task<(NotificationData Notification, bool Success)> Process(GhostlyContext context, NotificationData notification)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            try
            {
                await _semaphore.WaitAsync();
                foreach (var rule in _rules.OrderBy(r => r.SortOrder))
                {
                    var foundNotification = context
                        .Notifications.Where(n => n.Id == notification.Id)
                        .Where(rule.Filter).FirstOrDefault();

                    // Match?
                    if (foundNotification != null)
                    {
                        if (UpdateNotification(context, rule, foundNotification))
                        {
                            context.Update(foundNotification);
                            await context.SaveChangesAsync();
                        }

                        return (foundNotification, true);
                    }
                }

                return (notification, false);
            }
            finally
            {
                _semaphore.Release();
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

        private async Task UpdateRules()
        {
            try
            {
                await _semaphore.WaitAsync();

                _rules.Clear();
                using (var context = _factory.Create())
                {
                    _rules.AddRange(RuleMapper.Map(context.Rules.Include(r => r.Category), _localizer));
                }
            }
            finally
            {
                _log.Debug("Rule cache was updated.");
                _semaphore.Release();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Querying;
using Ghostly.Features.Rules;

namespace Ghostly.Data.Mapping
{
    public static class RuleMapper
    {
        public static List<Rule> Map(IEnumerable<RuleData> data, ILocalizer localizer)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var result = new List<Rule>();
            foreach (var category in data)
            {
                result.Add(Map(category, localizer));
            }

            return result;
        }

        public static Rule Map(RuleData rule, ILocalizer localizer)
        {
            if (rule is null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var description = string.Empty;
            var filter = default(Expression<Func<NotificationData, bool>>);
            if (rule.Expression != null)
            {
                if (!GhostlyQueryLanguage.TryParse(rule.Expression, out var expression, out var error))
                {
                    // TODO: Handle unparsable filters. For now throw.
                    throw new InvalidOperationException(error);
                }

                if (!GhostlyQueryLanguage.TryCompile(expression, out filter, out error))
                {
                    // TODO: Handle uncompiled filter. For now throw.
                    throw new InvalidOperationException(error);
                }

                description = RuleDescriber.Describe(expression, rule, localizer);
            }

            return new Rule
            {
                Id = rule.Id,
                SortOrder = rule.SortOrder,
                Enabled = rule.Enabled,
                Name = rule.Name,
                Description = description,
                Expression = rule.Expression,
                Star = rule.Star,
                Mute = rule.Mute,
                MarkAsRead = rule.MarkAsRead,
                StopProcessing = rule.StopProcessing,
                Category = rule.Category != null ? CategoryMapper.Map(rule.Category, localizer) : null,
                Filter = filter,
                ImportedFrom = rule.ImportedFrom,
                ImportedAt = rule.ImportedAt?.EnsureUniversalTime(),
            };
        }
    }
}

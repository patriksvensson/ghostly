using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ghostly.Core.Services;
using Ghostly.Core.Text;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Querying;

namespace Ghostly.Data.Mapping
{
    public static class CategoryMapper
    {
        public static List<Category> Map(IEnumerable<CategoryData> data, ILocalizer localizer)
        {
            if (data == null)
            {
                return new List<Category>();
            }

            var result = new List<Category>();
            foreach (var category in data)
            {
                result.Add(Map(category, localizer));
            }

            return result;
        }

        public static Category Map(CategoryData category, ILocalizer localizer)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            var filter = default(Expression<Func<NotificationData, bool>>);
            if (category.Expression != null)
            {
                if (!GhostlyQueryLanguage.TryCompile(category.Expression, out filter, out var error))
                {
                    // TODO: Handle uncompiled filter. For now throw.
                    throw new InvalidOperationException(error);
                }

                if (!category.IncludeMuted)
                {
                    // Filter muted categories
                    filter = filter.AndAlso(notification => !notification.Category.Muted);
                }
            }

            return new Category
            {
                Id = category.Id,
                Identifier = category.Identifier,
                Inbox = category.Inbox,
                Archive = category.Archive,
                Deletable = category.Deletable,
                Kind = category.Kind,
                Expression = category.Expression,
                Filter = filter,
                Glyph = GetGlyph(category),
                Emoji = EmojiHelper.GetEmoji(category.Emoji),
                Name = GetName(category, localizer),
                SortOrder = category.SortOrder,
                ShowTotal = category.ShowTotal,
                Muted = category.Muted,
                IncludeMuted = category.IncludeMuted,
                ImportedFrom = category.ImportedFrom,
                ImportedAt = category.ImportedAt?.EnsureUniversalTime(),
            };
        }

        public static string GetGlyph(CategoryData category)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (category.Deletable)
            {
                if (!string.IsNullOrWhiteSpace(category.Expression))
                {
                    return Constants.Glyphs.Filter;
                }
                else
                {
                    return Constants.Glyphs.Category;
                }
            }

            return category.Glyph;
        }

        public static string GetName(CategoryData category, ILocalizer localizer)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            if (category.Inbox)
            {
                return localizer.GetString("Shell_Inbox");
            }

            if (category.Archive)
            {
                return localizer.GetString("Shell_Archive");
            }

            return category.Name;
        }
    }
}

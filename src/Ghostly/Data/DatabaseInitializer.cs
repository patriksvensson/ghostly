using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Core.Text;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Data
{
    [DependentOn(typeof(ISystemService))]
    public sealed class DatabaseInitializer : IInitializable
    {
        private const string InboxIdentifier = "144e5963c7b94fb68527f891a42fa7c9";
        private const string ArchivedIdentifier = "5d65d106899745c7bde11faa5b05f2a8";
        private const string MutedIdentifier = "81bcc643505447febb5147ba6ce7968b";
        private const string StarredIdentifier = "7095e83965a447b6a2093ecda9ba5743";

        private readonly IMarketHelper _market;
        private readonly IGhostlyLog _log;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;

        public DatabaseInitializer(
            IMarketHelper market,
            IGhostlyLog log,
            ITelemetry telemetry,
            ILocalizer localizer)
        {
            _market = market;
            _log = log;
            _telemetry = telemetry;
            _localizer = localizer;
        }

        public async Task<bool> Initialize(bool background)
        {
            try
            {
                // Perform migration
                using (var context = new GhostlyContext())
                {
                    // Migrate database if necessary
                    await context.Database.MigrateAsync();

                    // Run fixes for things.
                    await EnsureCategoriesExist(context);
                    await EnsureCategoriesHaveIdentifier(context);
                    await EnsureRulesHaveIdentifier(context);
                    await FixCategoryEmojis(context);
                }
            }
            catch (Exception ex)
            {
                // Nothing we can do here really...
                _telemetry.TrackException(ex, nameof(DatabaseInitializer));
            }

            return true;
        }

        private ISet<string> GetAppliedMigrations()
        {
            try
            {
                using (var context = new GhostlyContext())
                {
                    return new HashSet<string>(
                        context.Database.GetAppliedMigrations(),
                        StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                _telemetry.TrackException(ex, nameof(DatabaseInitializer));
                return null;
            }
        }

        private async Task EnsureCategoriesExist(GhostlyContext context)
        {
            if (!(await context.Categories.AnyAsync()))
            {
                // Name is not important since this is set dynamically
                context.Categories.Add(new CategoryData
                {
                    Name = "Inbox",
                    Identifier = InboxIdentifier,
                    Glyph = Constants.Glyphs.Inbox,
                    Inbox = true,
                    SortOrder = 0,
                    Kind = CategoryKind.Category,
                    Deletable = false,
                });

                // Name is not important since this is set dynamically
                context.Categories.Add(new CategoryData
                {
                    Name = "Archived",
                    Identifier = ArchivedIdentifier,
                    Glyph = Constants.Glyphs.Archive,
                    Archive = true,
                    SortOrder = 10,
                    Kind = CategoryKind.Category,
                    Deletable = false,
                });

                context.Categories.Add(new CategoryData
                {
                    Name = _localizer["Shell_Muted"],
                    Identifier = MutedIdentifier,
                    Glyph = Constants.Glyphs.Filter,
                    SortOrder = 20,
                    Expression = "@muted",
                    Kind = CategoryKind.Filter,
                    Deletable = true,
                });

                context.Categories.Add(new CategoryData
                {
                    Name = _localizer["Shell_Starred"],
                    Identifier = StarredIdentifier,
                    Glyph = Constants.Glyphs.Filter,
                    SortOrder = 30,
                    Expression = "@starred",
                    Kind = CategoryKind.Filter,
                    Deletable = true,
                });

                _log.Debug("Creating categories...");
                await context.SaveChangesAsync(true);
            }
        }

        private async Task EnsureCategoriesHaveIdentifier(GhostlyContext context)
        {
            if (await context.Categories.AnyAsync(x => x.Identifier == null))
            {
                foreach (var category in context.Categories)
                {
                    if (category.Identifier == null)
                    {
                        if (category.Name == "Inbox")
                        {
                            _log.Debug("Adding known identifier for category '{CategoryName}'...", category.Name);
                            category.Identifier = InboxIdentifier;
                        }
                        else if (category.Name == "Archived")
                        {
                            _log.Debug("Adding known identifier for category '{CategoryName}'...", category.Name);
                            category.Identifier = ArchivedIdentifier;
                        }
                        else if (category.Name == _localizer["Shell_Starred"])
                        {
                            _log.Debug("Adding known identifier for category '{CategoryName}'...", category.Name);
                            category.Identifier = StarredIdentifier;
                        }
                        else if (category.Name == _localizer["Shell_Muted"])
                        {
                            _log.Debug("Adding known identifier for category '{CategoryName}'...", category.Name);
                            category.Identifier = MutedIdentifier;
                        }
                        else
                        {
                            // Generate an identifier for the category.
                            _log.Debug("Adding identifier for category '{CategoryName}'...", category.Name);
                            category.Identifier = Guid.NewGuid().ToGhostlyFormat();
                        }
                    }
                }

                _log.Debug("Migrating categories...");
                await context.SaveChangesAsync(true);
            }
        }

        private async Task EnsureRulesHaveIdentifier(GhostlyContext context)
        {
            if (await context.Rules.AnyAsync(x => x.Identifier == null))
            {
                foreach (var rule in context.Rules.Where(r => r.Identifier == null))
                {
                    // Generate an identifier for the rule.
                    _log.Debug("Adding identifier for rule '{RuleName}'...", rule.Name);
                    rule.Identifier = Guid.NewGuid().ToGhostlyFormat();
                }

                await context.SaveChangesAsync(true);
            }
        }

        private async Task FixCategoryEmojis(GhostlyContext context)
        {
            if (context.Categories.ToList().Any(x => x.Emoji != null && !x.Emoji.StartsWith(":", StringComparison.Ordinal)))
            {
                foreach (var category in context.Categories.Where(x => x.Emoji != null && !x.Emoji.StartsWith(":", StringComparison.Ordinal)))
                {
                    var shortcode = EmojiHelper.GetEmojiShortcode(category.Emoji);
                    if (!string.IsNullOrWhiteSpace(shortcode))
                    {
                        category.Emoji = shortcode;
                    }
                }

                await context.SaveChangesAsync(true);
            }
        }
    }
}

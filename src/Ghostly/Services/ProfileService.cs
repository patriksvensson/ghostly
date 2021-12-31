using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Services
{
    public interface IProfileService
    {
        Task Import(SettingsProfile profile);
        Task<SettingsProfile> Export(string name);
    }

    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(ICategoryService))]
    public sealed class ProfileService : IProfileService
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly ICategoryService _categories;
        private readonly IMediator _mediator;
        private readonly ITelemetry _telemetry;
        private readonly IGhostlyLog _log;

        public ProfileService(
            IGhostlyContextFactory factory,
            ICategoryService categories,
            IMediator mediator,
            ITelemetry telemetry,
            IGhostlyLog log)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task Import(SettingsProfile profile)
        {
            if (profile is null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            try
            {
                _log.Information("Importing profile {ProfileName}...", profile.Name);
                _telemetry.TrackEvent(Constants.TrackingEvents.ImportedProfile);

                using (var context = _factory.Create())
                {
                    // Importing categories
                    _log.Verbose("Importing categories...");
                    foreach (var profileCategory in profile.Categories)
                    {
                        var category = context.Categories.FirstOrDefault(x => x.Identifier == profileCategory.Identifier);
                        if (category == null)
                        {
                            // Create category
                            await _categories.CreateCategory(
                                profileCategory.GetCreateRequest(profile.Name));
                        }
                        else
                        {
                            // Update category
                            await _categories.EditCategory(
                                profileCategory.GetUpdateRequest(profile.Name, category.Id));
                        }
                    }

                    _log.Debug("Importing rules...");
                    foreach (var profileRule in profile.Rules)
                    {
                        var rule = context.Rules.FirstOrDefault(x => x.Identifier == profileRule.Identifier);
                        if (rule == null)
                        {
                            // Create rule
                            var createdRule = _mediator.Send(new CreateRuleHandler.Request(new NewRuleModel
                            {
                                Name = profileRule.Name,
                                Identifier = profileRule.Identifier,
                                CategoryIdentifier = profileRule.CategoryIdentifier,
                                Expression = profileRule.Expression,
                                MarkAsRead = profileRule.MarkAsRead,
                                Mute = profileRule.Mute,
                                Star = profileRule.Star,
                                StopProcessing = profileRule.StopProcessing,
                                ImportedFrom = profile.Name,
                            }));
                        }
                        else
                        {
                            // Update rule
                            var updatedRule = _mediator.Send(new UpdateRuleHandler.Request(new EditRuleModel
                            {
                                Id = rule.Id,
                                Name = profileRule.Name,
                                CategoryIdentifier = profileRule.CategoryIdentifier,
                                Expression = profileRule.Expression,
                                MarkAsRead = profileRule.MarkAsRead,
                                Mute = profileRule.Mute,
                                Star = profileRule.Star,
                                StopProcessing = profileRule.StopProcessing,
                                ImportedFrom = profile.Name,
                            }));
                        }
                    }
                }

                _log.Information("Profile {ProfileName} was imported.", profile.Name);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while importing profile {ProfileName}.", profile.Name);
                throw;
            }
        }

        public Task<SettingsProfile> Export(string name)
        {
            try
            {
                _log.Information("Exporting profile {ProfileName}...", name);
                _telemetry.TrackEvent(Constants.TrackingEvents.ExportedProfile);

                var profile = new SettingsProfile
                {
                    Name = name,
                };

                using (var context = _factory.Create())
                {
                    foreach (var category in GetCategoriesForExport(context).OrderBy(x => x.SortOrder))
                    {
                        profile.Categories.Add(new SettingsProfileCategory
                        {
                            Identifier = category.Identifier,
                            Name = category.Name,
                            SortOrder = category.SortOrder,
                            Emoji = category.Emoji,
                            Filter = category.Expression,
                            ShowTotal = category.ShowTotal,
                        });
                    }

                    foreach (var rule in GetRulesForExport(context).OrderBy(x => x.SortOrder))
                    {
                        profile.Rules.Add(new SettingsProfileRule
                        {
                            Identifier = rule.Identifier,
                            Name = rule.Name,
                            Enabled = rule.Enabled,
                            Expression = rule.Expression,
                            MarkAsRead = rule.MarkAsRead,
                            Mute = rule.Mute,
                            SortOrder = rule.SortOrder,
                            Star = rule.Star,
                            StopProcessing = rule.StopProcessing,
                            CategoryIdentifier = rule.Category?.Identifier,
                        });
                    }
                }

                return Task.FromResult(profile);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "An error occured while exporting profile {ProfileName}.", name);
                throw;
            }
        }

        private IQueryable<CategoryData> GetCategoriesForExport(GhostlyContext context)
        {
            return context.Categories.Where(x =>
                x.Deletable && !x.Inbox && !x.Archive && x.Identifier != null);
        }

        private IQueryable<RuleData> GetRulesForExport(GhostlyContext context)
        {
            return context.Rules.Include(c => c.Category).Where(x => x.Identifier != null);
        }
    }
}

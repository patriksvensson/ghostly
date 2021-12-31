using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Categories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Services
{
    public interface ICategoryService
    {
        ReadOnlyObservableCollection<Category> Categories { get; }

        Task CreateCategory(CreateCategoryHandler.Request request);
        Task EditCategory(UpdateCategoryHandler.Request request);
        Task DeleteCategory(Category category);
        Task ReorderCategories(IEnumerable<CategoryOrder> ordering);
    }

    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(ICultureService))]
    public sealed class CategoryService : ICategoryService, IInitializable
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IMessageService _messenger;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly ObservableCollection<Category> _categories;

        public ReadOnlyObservableCollection<Category> Categories { get; }

        public CategoryService(
            IGhostlyContextFactory factory,
            IMessageService messenger,
            IMediator mediator,
            ILocalizer localizer)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _categories = new ObservableCollection<Category>();

            Categories = new ReadOnlyObservableCollection<Category>(_categories);

            messenger.SubscribeAsync<ProfileImported>(OnProfileImported);
        }

        public async Task<bool> Initialize(bool background)
        {
            using (var context = _factory.Create())
            {
                var data = await context.Categories.OrderBy(c => c.SortOrder).ToListAsync();
                _categories.AddRange(CategoryMapper.Map(data, _localizer));
            }

            return true;
        }

        public async Task CreateCategory(CreateCategoryHandler.Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var category = await _mediator.Send(request);
            if (category != null)
            {
                _categories.Add(category); // Add to cache
                await _messenger.PublishAsync(new CategoryCreated(category));
            }
        }

        public async Task EditCategory(UpdateCategoryHandler.Request request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var updatedCategory = await _mediator.Send(request);
            if (updatedCategory != null)
            {
                // Update cache representation of the category.
                var categoryInCache = _categories.SingleOrDefault(c => c.Id == request.CategoryId);
                if (categoryInCache != null)
                {
                    categoryInCache.Name = updatedCategory.Name;
                    categoryInCache.Glyph = string.IsNullOrWhiteSpace(updatedCategory.Expression) ? Constants.Glyphs.Category : Constants.Glyphs.Filter;
                    categoryInCache.Emoji = updatedCategory.Emoji;
                    categoryInCache.ShowTotal = updatedCategory.ShowTotal;
                    categoryInCache.Muted = updatedCategory.Muted;
                    categoryInCache.IncludeMuted = updatedCategory.IncludeMuted;
                    categoryInCache.Kind = string.IsNullOrWhiteSpace(updatedCategory.Expression) ? CategoryKind.Category : CategoryKind.Filter;
                    categoryInCache.Expression = updatedCategory.Expression;
                    categoryInCache.Filter = updatedCategory.Filter;
                }

                // Notify subscribers that the category was created.
                await _messenger.PublishAsync(new CategoryEdited(categoryInCache));
            }
        }

        public async Task DeleteCategory(Category category)
        {
            if (await _mediator.Send(new DeleteCategoryHandler.Request(category)))
            {
                // Remove the category from the cache.
                var categoryInCache = _categories.SingleOrDefault(c => c.Id == category.Id);
                if (categoryInCache != null)
                {
                    _categories.Remove(categoryInCache);
                }

                // Notify subscribers about the category being deleted.
                await _messenger.PublishAsync(new CategoryDeleted(category));
            }
        }

        public async Task ReorderCategories(IEnumerable<CategoryOrder> ordering)
        {
            await _mediator.Send(new ReorderCategoriesHandler.Request(ordering));
        }

        public Task OnProfileImported(ProfileImported message)
        {
            return Task.CompletedTask;
        }
    }
}

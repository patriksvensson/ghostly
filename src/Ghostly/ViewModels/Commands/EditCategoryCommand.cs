using System;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Categories;
using Ghostly.Services;
using Ghostly.ViewModels.Dialogs;

namespace Ghostly.ViewModels.Commands
{
    public sealed class EditCategoryCommand : AsyncCommand<Category>
    {
        private readonly IDialogService _dialogs;
        private readonly ICategoryService _categories;

        public EditCategoryCommand(
            IDialogService dialogs, ICategoryService categories)
        {
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }

        protected override async Task ExecuteCommand(Category category)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var result = await _dialogs.ShowDialog(new CreateCategoryViewModel.Request { Category = category });
            if (result != null)
            {
                await _categories.EditCategory(
                    new UpdateCategoryHandler.Request(
                        category.Id, result.Name,
                        result.Expression, result.Emoji,
                        result.ShowTotal,
                        result.Muted, result.IncludeMuted));
            }
        }
    }
}

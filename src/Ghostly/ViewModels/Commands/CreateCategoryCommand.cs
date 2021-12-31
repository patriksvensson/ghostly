using System;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Features.Categories;
using Ghostly.Services;
using Ghostly.ViewModels.Dialogs;

namespace Ghostly.ViewModels.Commands
{
    public sealed class CreateCategoryCommand : AsyncCommand
    {
        private readonly IDialogService _dialogs;
        private readonly ICategoryService _categories;

        public CreateCategoryCommand(IDialogService dialogs, ICategoryService categories)
        {
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }

        protected override async Task ExecuteCommand()
        {
            var result = await _dialogs.ShowDialog(new CreateCategoryViewModel.Request());
            if (result != null)
            {
                await _categories.CreateCategory(
                    new CreateCategoryHandler.Request(
                        result.Name, result.Expression,
                        result.Emoji, result.ShowTotal,
                        result.Muted, result.IncludeMuted));
            }
        }
    }
}

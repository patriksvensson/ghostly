using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using Ghostly.Services;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class DeleteCategoryCommand : AsyncCommand<Category>
    {
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly IDialogService _dialogs;
        private readonly ICategoryService _categories;

        public DeleteCategoryCommand(
            IMediator mediator, ILocalizer localizer,
            IDialogService dialogs, ICategoryService categories)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }

        protected override async Task ExecuteCommand(Category category)
        {
            var rules = await _mediator.Send(new GetRulesForCategory.Request(category));
            if (rules.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendLine(_localizer.Format("DeleteCategory_CannotDelete_UsedInFollowingRules", category.Name));
                builder.AppendLine();
                builder.AppendLine(string.Join("\r\n", rules.Select(r => "• " + r.Name)));
                builder.AppendLine();
                builder.Append(_localizer.GetString("DeleteCategory_CannotDelete_ChangeOrDeleteRule"));

                await _dialogs.ShowDialog(new MessageBoxViewModel.Request
                {
                    PrimaryText = _localizer.GetString("General_Ok"),
                    Glyph = "\uE74D",
                    Title = _localizer.GetString("DeleteCategory_CannotDelete_Title"),
                    Body = builder.ToString(),
                });

                return;
            }

            var kind = category.Kind == CategoryKind.Filter ? "filter" : "category";
            var result = await _dialogs.ShowDialog(new ConfirmActionViewModel.Request
            {
                Title = _localizer.GetString($"DeleteCategory_{category.Kind}_Title"),
                Body = _localizer.Format($"DeleteCategory_{category.Kind}_Confirmation", category.Name),
                PrimaryText = _localizer.GetString("General_Delete"),
                SecondaryText = _localizer.GetString("General_Cancel"),
                Glyph = "\uE74D",
            });

            if (result == ConfirmResult.Ok)
            {
                await _categories.DeleteCategory(category);
            }
        }
    }
}

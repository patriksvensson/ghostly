using System;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Notifications;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class ArchiveCategoryCommand : AsyncCommand<Category>
    {
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly IDialogService _dialogs;

        public ArchiveCategoryCommand(
            IMediator mediator, ILocalizer localizer,
            IDialogService dialogs)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        protected override async Task ExecuteCommand(Category category)
        {
            if (category is null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var result = await _dialogs.ShowDialog(new ConfirmActionViewModel.Request
            {
                Title = _localizer.GetString($"ArchiveAll_Title"),
                Body = _localizer.Format("ArchiveAll_Confirmation", category.Name),
                PrimaryText = _localizer.GetString("ArchiveAll_Ok"),
                SecondaryText = _localizer.GetString("General_Cancel"),
                Glyph = "\uE8C3",
            });

            if (result == ConfirmResult.Ok)
            {
                await _mediator.Send(new ArchiveAllHandler.Request(category));
            }
        }
    }
}

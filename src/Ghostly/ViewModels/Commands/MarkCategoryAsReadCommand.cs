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
    public sealed class MarkCategoryAsReadCommand : AsyncCommand<Category>
    {
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly IDialogService _dialogs;

        public MarkCategoryAsReadCommand(
            IMediator mediator, ILocalizer localizer,
            IDialogService dialogs)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        }

        protected override async Task ExecuteCommand(Category parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = await _dialogs.ShowDialog(new ConfirmActionViewModel.Request
            {
                Title = _localizer.GetString("MarkAllAsRead_Title"),
                Body = _localizer.Format("MarkAllAsRead_Confirmation", parameter.Name),
                PrimaryText = _localizer.GetString("MarkAllAsRead_Title"),
                SecondaryText = _localizer.GetString("General_Cancel"),
                Glyph = "\uE8C3",
            });

            if (result == ConfirmResult.Ok)
            {
                await _mediator.Send(new MarkAllAsReadHandler.Request(parameter));
            }
        }
    }
}

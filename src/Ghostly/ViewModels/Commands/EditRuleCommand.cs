using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class EditRuleCommand : RuleCommand.WithRule
    {
        private readonly IDialogService _dialog;
        private readonly IMediator _mediator;

        public EditRuleCommand(
            IDialogService dialog,
            IMediator mediator,
            Func<bool> enabled)
            : base(enabled)
        {
            _dialog = dialog;
            _mediator = mediator;
        }

        protected override async Task ExecuteCommand(Rule parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = await _dialog.ShowDialog(new CreateRuleViewModel.Request(parameter.Id));
            if (result != null)
            {
                await _mediator.Send(
                    new UpdateRuleHandler.Request(
                        new EditRuleModel
                        {
                            Id = result.Id,
                            Name = result.Name,
                            Expression = result.Expression,
                            Star = result.Star,
                            Mute = result.Mute,
                            MarkAsRead = result.MarkAsRead,
                            StopProcessing = result.StopProcessing,
                            CategoryId = result.CategoryId,
                        }));
            }
        }
    }
}

using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class CreateRuleCommand : RuleCommand
    {
        private readonly IDialogService _dialog;
        private readonly IMediator _mediator;

        public CreateRuleCommand(
            IDialogService dialog,
            IMediator mediator,
            Func<bool> enabled)
            : base(enabled)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override async Task ExecuteCommand()
        {
            var result = await _dialog.ShowDialog(new CreateRuleViewModel.Request());
            if (result != null)
            {
                await _mediator.Send(
                    new CreateRuleHandler.Request(
                        new NewRuleModel
                        {
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

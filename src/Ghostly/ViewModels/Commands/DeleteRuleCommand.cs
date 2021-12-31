using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class DeleteRuleCommand : RuleCommand.WithRule
    {
        private readonly IDialogService _dialog;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;

        public DeleteRuleCommand(
            IDialogService dialog,
            IMediator mediator,
            ILocalizer localizer,
            Func<bool> enabled)
            : base(enabled)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer;
        }

        protected override async Task ExecuteCommand(Rule rule)
        {
            if (rule is null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            var result = await _dialog.ShowDialog(new ConfirmActionViewModel.Request
            {
                Title = _localizer.GetString("DeleteRule_Title"),
                Body = _localizer.Format("DeleteRule_Confirmation", rule.Name),
                PrimaryText = _localizer.GetString("DeleteRule_Ok"),
                SecondaryText = _localizer.GetString("General_Cancel"),
                Glyph = "\uE74D",
            });

            if (result == ConfirmResult.Ok)
            {
                await _mediator.Send(new DeleteRuleHandler.Request(rule.Id));
            }
        }
    }
}

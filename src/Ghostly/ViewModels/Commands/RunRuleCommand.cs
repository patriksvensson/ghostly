using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Features.Rules;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class RunRuleCommand : RuleCommand.WithRule
    {
        private readonly IDialogService _dialog;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;

        public RunRuleCommand(
            IDialogService dialog,
            IMediator mediator,
            ILocalizer localizer,
            Func<bool> enabled)
            : base(enabled)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _mediator = mediator;
            _localizer = localizer;
        }

        protected override async Task ExecuteCommand(Rule parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var result = await _dialog.ShowDialog(new SelectCategoryViewModel.Request
            {
                IncludeFilters = false,
                Title = _localizer.Format("Rules_RunRule_Title", parameter.Name),
                PrimaryButtonTitle = _localizer.GetString("Rules_RunRules_Ok"),
            });

            if (result != null)
            {
                await _mediator.Send(new ProcessCategoryRuleHandler.Request(result, parameter));
            }
        }
    }
}

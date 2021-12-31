using System;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Features.Rules;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.ViewModels.Commands
{
    public sealed class RunRulesCommand : RuleCommand
    {
        private readonly IDialogService _dialog;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;
        private readonly Func<bool> _canRunRules;

        public RunRulesCommand(
            IDialogService dialog,
            IMediator mediator,
            ILocalizer localizer,
            Func<bool> canRunRules,
            Func<bool> enabled)
            : base(enabled)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _mediator = mediator;
            _localizer = localizer;
            _canRunRules = canRunRules;
        }

        protected override bool CanExecuteCommand()
        {
            return base.CanExecuteCommand() && _canRunRules();
        }

        protected override async Task ExecuteCommand()
        {
            var result = await _dialog.ShowDialog(new SelectCategoryViewModel.Request
            {
                IncludeFilters = false,
                Title = _localizer.GetString("Rules_RunRules_Title"),
                PrimaryButtonTitle = _localizer.GetString("Rules_RunRules_Ok"),
            });

            if (result != null)
            {
                await _mediator.Send(new ProcessCategoryRuleHandler.Request(result));
            }
        }
    }
}

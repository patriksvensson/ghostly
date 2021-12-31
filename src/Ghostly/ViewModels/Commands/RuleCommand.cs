using System;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Domain;

namespace Ghostly.ViewModels.Commands
{
    public abstract class RuleCommand : AsyncCommand
    {
        private readonly Func<bool> _enabled;

        protected RuleCommand(Func<bool> enabled)
            : base()
        {
            _enabled = enabled;
        }

        public abstract class WithRule : AsyncCommand<Rule>
        {
            private readonly Func<bool> _enabled;

            protected WithRule(Func<bool> enabled)
                : base()
            {
                _enabled = enabled;
            }

            protected override bool CanExecuteCommand(Rule parameter)
            {
                return _enabled();
            }
        }

        protected override bool CanExecuteCommand()
        {
            return _enabled();
        }
    }
}

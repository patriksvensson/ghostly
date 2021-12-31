using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm.Commands;
using Ghostly.Domain;

namespace Ghostly.ViewModels.Commands
{
    public abstract class NotificationCommand : AsyncCommand<Notification>
    {
        private readonly MainViewModel _model;

        protected abstract bool IsCandidate(Notification notification);
        protected abstract Task Execute(IEnumerable<Notification> notifications);

        protected NotificationCommand(MainViewModel model)
        {
            _model = model;
        }

        protected override bool CanExecuteCommand(Notification parameter)
        {
            if (IsMultipleSelected())
            {
                return true;
            }

            if (parameter == null)
            {
                return false;
            }

            return parameter != null ? IsCandidate(parameter) : false;
        }

        protected override async Task ExecuteCommand(Notification parameter)
        {
            if (IsMultipleSelected())
            {
                await Execute(_model.SelectedItems);
                return;
            }

            if (parameter != null)
            {
                await Execute(new[] { parameter });
            }
        }

        private bool IsMultipleSelected()
        {
            return _model.IsMultipleSelected && (_model.SelectedItems?.Any(IsCandidate) ?? false);
        }
    }
}

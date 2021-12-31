using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels.Commands
{
    public sealed class ArchiveCommand : NotificationCommand
    {
        private readonly NotificationSource _source;
        private readonly INotificationService _service;

        public ArchiveCommand(
            MainViewModel model,
            NotificationSource source,
            INotificationService service)
            : base(model)
        {
            _source = source;
            _service = service;
        }

        protected override bool IsCandidate(Notification notification)
        {
            if (_source.Category == null)
            {
                return true;
            }

            return !_source.Category.Archive && !notification.Category.Archive;
        }

        protected override async Task Execute(IEnumerable<Notification> notifications)
        {
            await _service.Archive(notifications);
        }
    }
}

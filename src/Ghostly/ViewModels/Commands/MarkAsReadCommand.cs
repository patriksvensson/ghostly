using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels.Commands
{
    public sealed class MarkAsReadCommand : NotificationCommand
    {
        private readonly INotificationService _notifications;

        public MarkAsReadCommand(MainViewModel model, INotificationService notifications)
            : base(model)
        {
            _notifications = notifications;
        }

        protected override bool IsCandidate(Notification notification)
        {
            return notification.Unread;
        }

        protected override async Task Execute(IEnumerable<Notification> notifications)
        {
            await _notifications.MarkAsRead(notifications).ConfigureAwait(false);
        }
    }
}

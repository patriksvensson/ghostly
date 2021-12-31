using System.Collections.Generic;
using System.Threading.Tasks;
using Ghostly.Domain;
using Ghostly.Services;

namespace Ghostly.ViewModels.Commands
{
    public sealed class MuteCommand : NotificationCommand
    {
        private readonly INotificationService _notifications;

        public MuteCommand(MainViewModel model, INotificationService notifications)
            : base(model)
        {
            _notifications = notifications;
        }

        protected override bool IsCandidate(Notification notification)
        {
            return !notification.Muted;
        }

        protected override async Task Execute(IEnumerable<Notification> notifications)
        {
            await _notifications.Mute(notifications);
        }
    }
}

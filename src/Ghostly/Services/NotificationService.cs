using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ghostly.Core.Services;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using Ghostly.Features.Notifications;
using Ghostly.Utilities;
using Ghostly.ViewModels.Dialogs;
using MediatR;

namespace Ghostly.Services
{
    public interface INotificationService
    {
        Task Archive(Notification notification);
        Task Archive(IEnumerable<Notification> notifications);
        Task MarkAsRead(Notification notification);
        Task MarkAsRead(IEnumerable<Notification> notifications);
        Task Move(Notification notification);
        Task Move(IEnumerable<Notification> notifications);

        Task Star(Notification notification);
        Task Star(IEnumerable<Notification> notifications);
        Task Unstar(Notification notification);
        Task Unstar(IEnumerable<Notification> notifications);
        Task Mute(Notification notification);
        Task Mute(IEnumerable<Notification> notifications);
        Task Unmute(Notification notification);
        Task Unmute(IEnumerable<Notification> notifications);
    }

    public sealed class NotificationService : INotificationService
    {
        private readonly ICategoryService _categories;
        private readonly IDialogService _dialogs;
        private readonly IMessageService _messenger;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;

        public NotificationService(
            ICategoryService categories,
            IDialogService dialogs,
            IMessageService messenger,
            IMediator mediator,
            ILocalizer localizer)
        {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        public async Task Archive(Notification notification)
        {
            await Archive(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task Archive(IEnumerable<Notification> notifications)
        {
            var archiveCategory = _categories.Categories.SingleOrDefault(c => c.Archive);
            Debug.Assert(archiveCategory != null, "Could not find an archive category.");

            using (var progress = new IndeterminateProgressReporter(_messenger))
            {
                await progress.ShowProgress(notifications.SafeCount() > 3, _localizer.GetString("Archive_Progress_MarkingAsRead")).ConfigureAwait(false);
                await _mediator.Send(new MarkAsReadHandler.Request(notifications.Where(n => n.Unread), broadcast: false)).ConfigureAwait(false);

                await progress.ShowProgress(notifications.SafeCount() > 3, _localizer.GetString("Archive_Progress_Archiving")).ConfigureAwait(false);
                await _mediator.Send(new MoveNotificationHandler.Request(notifications, archiveCategory)).ConfigureAwait(false);
            }
        }

        public async Task Move(Notification notification)
        {
            await Move(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task Move(IEnumerable<Notification> notifications)
        {
            var categoriesToExclude = new HashSet<int>();

            // If all notifications belong to the same category, remove it.
            var categories = new HashSet<int>(notifications.Select(x => x.CategoryId));
            if (categories.Count == 1)
            {
                categoriesToExclude.AddRange(categories);
            }

            // Show the dialog.
            var category = await _dialogs.ShowDialog(new SelectCategoryViewModel.Request()
            {
                Title = _localizer.GetString(notifications.Count() > 1, "Move_Title_Plural", "Move_Title_Singular"),
                IncludeFilters = false,
                ExcludedCategories = categoriesToExclude,
                PrimaryButtonTitle = _localizer.GetString("Move_Ok"),
            }).ConfigureAwait(false);

            if (category != null)
            {
                await _mediator.Send(new MoveNotificationHandler.Request(notifications, category))
                    .ConfigureAwait(false);
            }
        }

        public async Task Star(Notification notification)
        {
            await Star(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task Star(IEnumerable<Notification> notifications)
        {
            await _mediator.Send(new ChangeNotificationStateHandler.Request(notifications, UpdateNotificationState.Star))
                .ConfigureAwait(false);
        }

        public async Task Unstar(Notification notification)
        {
            await Unstar(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task Unstar(IEnumerable<Notification> notifications)
        {
            await _mediator.Send(new ChangeNotificationStateHandler.Request(notifications, UpdateNotificationState.Unstar))
                .ConfigureAwait(false);
        }

        public async Task Mute(Notification notification)
        {
            await Mute(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task Mute(IEnumerable<Notification> notifications)
        {
            await _mediator.Send(new ChangeNotificationStateHandler.Request(notifications, UpdateNotificationState.Mute))
                .ConfigureAwait(false);
        }

        public async Task Unmute(Notification notification)
        {
            await _mediator.Send(new ChangeNotificationStateHandler.Request(new[] { notification }, UpdateNotificationState.Unmute))
                .ConfigureAwait(false);
        }

        public async Task Unmute(IEnumerable<Notification> notifications)
        {
            await _mediator.Send(new ChangeNotificationStateHandler.Request(notifications, UpdateNotificationState.Unmute))
                .ConfigureAwait(false);
        }

        public async Task MarkAsRead(Notification notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (notification.Read)
            {
                return;
            }

            await MarkAsRead(new[] { notification })
                .ConfigureAwait(false);
        }

        public async Task MarkAsRead(IEnumerable<Notification> notifications)
        {
            var unread = notifications.Where(n => n.Unread);
            await _mediator.Send(new MarkAsReadHandler.Request(unread))
                .ConfigureAwait(false);
        }
    }
}

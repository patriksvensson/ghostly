using System;
using System.Collections.Generic;
using Ghostly.Core.Services;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.GitHub;

namespace Ghostly.Data.Mapping
{
    public static class NotificationMapper
    {
        public static List<Notification> Map(IEnumerable<NotificationData> data, ILocalizer localizer)
        {
            var result = new List<Notification>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    result.Add(Map(item, localizer));
                }
            }

            return result;
        }

        public static Notification Map(NotificationData data, ILocalizer localizer)
        {
            if (data == null)
            {
                return null;
            }

            if (data.Discriminator == Discriminator.GitHub)
            {
                return new GitHubNotification
                {
                    Id = data.Id,

                    AccountId = data.AccountId,
                    Muted = data.Muted,
                    Starred = data.Starred,
                    Timestamp = data.Timestamp.EnsureUniversalTime(),
                    Unread = !data.Muted && data.Unread,
                    GitHubId = data.GitHubId.Value,

                    WorkItemId = data.WorkItem.Id,
                    ExternalId = GetExternalId(data.WorkItem),
                    Category = GetCategory(data, localizer),
                    Kind = WorkItemMapper.GetKind(data.WorkItem),
                    Origin = GetOrigin(data.WorkItem),
                    Merged = data.WorkItem.Merged,
                    Locked = data.WorkItem.Locked,
                    Draft = data.WorkItem.IsDraft,
                    Preamble = data.WorkItem.Preamble,
                    State = WorkItemMapper.GetState(data.WorkItem),
                    Milestone = MilestoneMapper.Map(data.WorkItem.Milestone),
                    Title = data.WorkItem.Title,
                    Url = new Uri(data.WorkItem.Url, UriKind.RelativeOrAbsolute),
                    Tags = TagMapper.Map(data.WorkItem.Tags),
                };
            }

            throw new InvalidOperationException("Do not know how to map notification.");
        }

        private static NotificationCategory GetCategory(NotificationData notification, ILocalizer localizer)
        {
            return new NotificationCategory
            {
                Id = notification.Category.Id,
                Name = CategoryMapper.GetName(notification.Category, localizer),
                Glyph = notification.Category.Glyph,
                Archive = notification.Category.Archive,
            };
        }

        private static long GetExternalId(WorkItemData workitem)
        {
            return workitem.GitHubLocalId ?? workitem.GitHubId.Value;
        }

        private static string GetOrigin(WorkItemData workitem)
        {
            return $"{workitem.Repository.Owner}/{workitem.Repository.Name}";
        }
    }
}

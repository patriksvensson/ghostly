using System;
using System.Linq;
using Ghostly.Data;
using Ghostly.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ghostly
{
    public static class GhostlyContextExtensions
    {
        public static IQueryable<NotificationData> GetNotificationQuery(this GhostlyContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Notifications
                .Include(e => e.Category)
                .Include(e => e.WorkItem)
                .Include(e => e.WorkItem.Repository)
                .Include(e => e.WorkItem.Author)
                .Include(e => e.WorkItem.Milestone)
                .Include(e => e.WorkItem.Tags)
                    .ThenInclude(e => e.Tag)
                .Include(e => e.WorkItem.Comments)
                    .ThenInclude(e => e.Author)
                .Include(e => e.WorkItem.Assignees)
                    .ThenInclude(e => e.User);
        }

        public static IQueryable<AccountData> GetActiveAccountQuery(this GhostlyContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Accounts
                .Where(x => x.State == AccountState.Active);
        }
    }
}

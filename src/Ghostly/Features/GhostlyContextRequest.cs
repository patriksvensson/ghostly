using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features
{
    public abstract class GhostlyContextRequest : IRequest
    {
        public GhostlyContext Context { get; set; }

        private sealed class GhostlyContextWrapper : IGhostlyContext
        {
            private readonly IGhostlyContext _context;
            private readonly bool _dispose;

            public DbSet<AccountData> Accounts { get => _context.Accounts; set => _context.Accounts = value; }
            public DbSet<ActivityData> Activities { get => _context.Activities; set => _context.Activities = value; }
            public DbSet<AssigneeData> Assignees { get => _context.Assignees; set => _context.Assignees = value; }
            public DbSet<CategoryData> Categories { get => _context.Categories; set => _context.Categories = value; }
            public DbSet<CommentData> Comments { get => _context.Comments; set => _context.Comments = value; }
            public DbSet<NotificationData> Notifications { get => _context.Notifications; set => _context.Notifications = value; }
            public DbSet<RepositoryData> Repositories { get => _context.Repositories; set => _context.Repositories = value; }
            public DbSet<ReviewData> Reviews { get => _context.Reviews; set => _context.Reviews = value; }
            public DbSet<ReviewCommentData> ReviewComments { get => _context.ReviewComments; set => _context.ReviewComments = value; }
            public DbSet<RuleData> Rules { get => _context.Rules; set => _context.Rules = value; }
            public DbSet<SyncItemData> SyncItems { get => _context.SyncItems; set => _context.SyncItems = value; }
            public DbSet<TagData> Tags { get => _context.Tags; set => _context.Tags = value; }
            public DbSet<UserData> Users { get => _context.Users; set => _context.Users = value; }
            public DbSet<WorkItemData> WorkItems { get => _context.WorkItems; set => _context.WorkItems = value; }

            public GhostlyContextWrapper(IGhostlyContext context, bool dispose)
            {
                _context = context;
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (_dispose)
                {
                    _context?.Dispose();
                }
            }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                return _context.SaveChangesAsync(cancellationToken);
            }
        }

        public IGhostlyContext GetOrCreateContext(IGhostlyContextFactory factory)
        {
            if (factory is null)
            {
                throw new System.ArgumentNullException(nameof(factory));
            }

            if (Context != null)
            {
                return new GhostlyContextWrapper(Context, false);
            }

            return new GhostlyContextWrapper(factory.Create(), true);
        }
    }
}

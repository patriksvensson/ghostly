using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Data
{
    public interface IGhostlyContext : IDisposable
    {
        DbSet<AccountData> Accounts { get; set; }
        DbSet<ActivityData> Activities { get; set; }
        DbSet<AssigneeData> Assignees { get; set; }
        DbSet<CategoryData> Categories { get; set; }
        DbSet<CommentData> Comments { get; set; }
        DbSet<NotificationData> Notifications { get; set; }
        DbSet<RepositoryData> Repositories { get; set; }
        DbSet<ReviewData> Reviews { get; set; }
        DbSet<ReviewCommentData> ReviewComments { get; set; }
        DbSet<RuleData> Rules { get; set; }
        DbSet<SyncItemData> SyncItems { get; set; }
        DbSet<TagData> Tags { get; set; }
        DbSet<UserData> Users { get; set; }
        DbSet<WorkItemData> WorkItems { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public sealed class GhostlyContext : DbContext, IGhostlyContext
    {
        public DbSet<AccountData> Accounts { get; set; }
        public DbSet<ActivityData> Activities { get; set; }
        public DbSet<AssigneeData> Assignees { get; set; }
        public DbSet<CategoryData> Categories { get; set; }
        public DbSet<CommentData> Comments { get; set; }
        public DbSet<NotificationData> Notifications { get; set; }
        public DbSet<RepositoryData> Repositories { get; set; }
        public DbSet<ReviewData> Reviews { get; set; }
        public DbSet<ReviewCommentData> ReviewComments { get; set; }
        public DbSet<RuleData> Rules { get; set; }
        public DbSet<SyncItemData> SyncItems { get; set; }
        public DbSet<TagData> Tags { get; set; }
        public DbSet<UserData> Users { get; set; }
        public DbSet<WorkItemData> WorkItems { get; set; }

        public delegate GhostlyContext Factory();

        public GhostlyContext()
        {
        }

        public GhostlyContext(DbContextOptions<GhostlyContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder is null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=ghostly.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Case insensitivity
            builder.Entity<RepositoryData>().CaseInsensitive(p => p.Name, p => p.Owner);
            builder.Entity<UserData>().CaseInsensitive(p => p.Login, p => p.Name);
            builder.Entity<WorkItemData>().CaseInsensitive(p => p.Title, p => p.Preamble, p => p.Body);
            builder.Entity<CategoryData>().CaseInsensitive(p => p.Name);
            builder.Entity<TagData>().CaseInsensitive(p => p.Name, p => p.Description);
            builder.Entity<NotificationData>().CaseInsensitive(p => p.Reason);
            builder.Entity<CommentData>().CaseInsensitive(p => p.Body);
            builder.Entity<AccountData>().CaseInsensitive(p => p.Username);
            builder.Entity<MilestoneData>().CaseInsensitive(p => p.Name);

            // WorkItemTags
            builder.Entity<WorkItemTagData>()
                .ToTable("WorkItemTags");

            // Milestones
            builder.Entity<MilestoneData>()
                .ToTable("Milestones");

            // WorkItemTag -> WorkItem
            builder.Entity<WorkItemTagData>()
                .HasOne(wt => wt.WorkItem)
                .WithMany(wt => wt.Tags)
                .HasForeignKey(wt => wt.WorkItemId);

            // WorkItemTag -> Tag
            builder.Entity<WorkItemTagData>()
                .HasOne(wt => wt.Tag);
        }
    }
}

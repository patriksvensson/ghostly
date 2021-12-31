using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Text;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.GitHub.Octokit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.GitHub.Actions
{
    internal sealed class UpdateGitHubWorkItemLabels : GitHubRequestHandler<UpdateGitHubWorkItemLabels.Request>
    {
        private readonly IGhostlyLog _log;

        public UpdateGitHubWorkItemLabels(IGhostlyLog log)
        {
            _log = log;
        }

        public sealed class Request : IRequest<GitHubResult>
        {
            public IGitHubGateway Gateway { get; }
            public GhostlyContext Context { get; }
            public WorkItemData Workitem { get; }
            public IReadOnlyList<GitHubLabelInfo> Labels { get; }

            public Request(IGitHubGateway gateway, GhostlyContext context, WorkItemData workitem, IEnumerable<GitHubLabelInfo> labels)
            {
                Gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
                Context = context ?? throw new ArgumentNullException(nameof(context));
                Workitem = workitem ?? throw new ArgumentNullException(nameof(workitem));
                Labels = labels?.ToReadOnlyList() ?? throw new ArgumentNullException(nameof(labels));
            }
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            var workitem = request.Workitem;

            foreach (var label in request.Labels)
            {
                // TODO: Include local stuff to avoid duplicates.
                var workitemTag = workitem.Tags?.SingleOrDefault(x => x.Tag.Discriminator == Discriminator.GitHub && x.Tag.GitHubId == label.Id);
                if (workitemTag == null)
                {
                    // Create label
                    await CreateLabel(request, workitem, label);
                }
                else
                {
                    // Update label
                    UpdateLabel(request, label, workitemTag);
                }
            }

            // Remove removed labels.
            RemoveLabels(request, workitem);

            return GitHubResult.Ok();
        }

        private void RemoveLabels(Request request, WorkItemData workitem)
        {
            // TODO: Not very optimized...
            var missing = workitem?.Tags?.Where(
                x => x.Tag.Discriminator == Discriminator.GitHub &&
                    !request.Labels.Any(y => y.Id == x.Tag.GitHubId));

            if (missing?.Any() ?? false)
            {
                _log.Debug("Labels to remove: {@GitHubLabelsToRemove}", missing.Select(x => x.TagId));
                workitem.Tags.RemoveAll(x => missing.Any(y => x.TagId == y.TagId));
            }
        }

        private static async Task CreateLabel(Request request, WorkItemData workitem, GitHubLabelInfo label)
        {
            // Try finding the tag.
            var tag = await request.Context.Tags.FirstOrDefaultAsync(
                x => x.Discriminator == Discriminator.GitHub
                    && x.GitHubId == label.Id);

            if (tag == null)
            {
                tag = new TagData
                {
                    Discriminator = Discriminator.GitHub,
                    GitHubId = label.Id,
                    Name = EmojiHelper.Replace(label.Name),
                    Description = EmojiHelper.Replace(label.Description),
                    Color = ColorRepresentation.ParseHex(label.Color).ToHex(),
                };
            }

            // Add tag to work item.
            workitem.Tags = workitem.Tags ?? new List<WorkItemTagData>();
            workitem.Tags.Add(new WorkItemTagData
            {
                WorkItem = workitem,
                Tag = tag,
            });
        }

        private static void UpdateLabel(Request request, GitHubLabelInfo label, WorkItemTagData workitemTag)
        {
            if (workitemTag.Tag.Discriminator == Discriminator.GitHub)
            {
                workitemTag.Tag.Name = EmojiHelper.Replace(label.Name);
                workitemTag.Tag.Description = EmojiHelper.Replace(label.Description);
                workitemTag.Tag.Color = ColorRepresentation.ParseHex(label.Color).ToHex();

                request.Context.Tags.Update(workitemTag.Tag);
            }
        }
    }
}

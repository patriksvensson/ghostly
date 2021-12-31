using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class GetWorkItemHandler : GhostlyRequestHandler<GetWorkItemHandler.Request, WorkItem>
    {
        private readonly IGhostlyContextFactory _factory;

        public GetWorkItemHandler(IGhostlyContextFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public class Request : IRequest<WorkItem>
        {
            public int WorkItemId { get; }
            public bool Track { get; set; }

            public Request(int workitemId, bool track = true)
            {
                WorkItemId = workitemId;
                Track = track;
            }
        }

        public override async Task<WorkItem> Handle(Request request, CancellationToken cancellationToken)
        {
            using (var context = _factory.Create())
            {
                var query = request.Track
                    ? context.WorkItems
                    : context.WorkItems.AsNoTracking();

                query = query
                    .Include(e => e.Repository)
                    .Include(e => e.Author)
                    .Include(e => e.MergedBy)
                    .Include(e => e.Tags)
                        .ThenInclude(e => e.Tag)
                    .Include(e => e.Comments)
                        .ThenInclude(e => e.Author)
                    .Include(e => e.Reviews)
                        .ThenInclude(e => e.Author)
                    .Include(e => e.Reviews)
                        .ThenInclude(e => e.Comments)
                        .ThenInclude(e => e.Author);

                var workitem = await query.FirstOrDefaultAsync(x => x.Id == request.WorkItemId, cancellationToken);
                if (workitem == null)
                {
                    return null;
                }

                return WorkItemMapper.Map(workitem);
            }
        }
    }
}

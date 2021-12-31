using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Mapping;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Features.Querying;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ghostly.Features.Notifications
{
    public sealed class GetNotificationsHandler
        : GhostlyRequestHandler<GetNotificationsHandler.Request, IReadOnlyList<Notification>>
    {
        private readonly IGhostlyContextFactory _factory;
        private readonly IGhostlyLog _log;
        private readonly IMediator _mediator;
        private readonly ILocalizer _localizer;

        public GetNotificationsHandler(
            IGhostlyContextFactory factory,
            IGhostlyLog log,
            IMediator mediator,
            ILocalizer localizer)
        {
            _factory = factory;
            _log = log;
            _mediator = mediator;
            _localizer = localizer;
        }

        public sealed class Request : IRequest<IReadOnlyList<Notification>>
        {
            public Category Category { get; }
            public bool SearchMode { get; }
            public NotificationReadFilter Filter { get; }
            public string Query { get; }
            public int Index { get; }
            public int Size { get; }

            public Request(Category category, NotificationReadFilter filter, string query, int index, int size, bool search)
            {
                Category = category;
                Filter = filter;
                Query = query;
                Index = index;
                Size = size;
                SearchMode = search;
            }
        }

        public override async Task<IReadOnlyList<Notification>> Handle(
            Request request, CancellationToken cancellationToken)
        {
            var result = new List<Notification>();

            // Are we currently searching for something, but no criteria have been entered?
            if (request.SearchMode && string.IsNullOrWhiteSpace(request.Query))
            {
                // Return an empty result.
                return result;
            }

            using (var context = _factory.Create())
            {
                var accounts = (await _mediator.GetAccounts()).Where(a => a.State == AccountState.Active);
                foreach (var account in accounts)
                {
                    // Get all notifications for the account.
                    var query = context.GetNotificationQuery()
                        .Where(x => x.AccountId == account.Id);

                    // Search filter
                    if (!string.IsNullOrWhiteSpace(request.Query))
                    {
                        _log.Information("Query: {Query}", request.Query);
                        if (GhostlyQueryLanguage.TryCompile(request.Query, out var expression, out var _))
                        {
                            query = query.Where(expression);
                        }
                        else
                        {
                            // Fall back to text search
                            query = query.Where(x =>
                                (x.Discriminator == Discriminator.GitHub && EF.Functions.Like(x.WorkItem.Body, $"%{request.Query}%")) ||
                                (x.Discriminator == Discriminator.GitHub && EF.Functions.Like(x.WorkItem.Title, $"%{request.Query}%")) ||
                                (x.Discriminator == Discriminator.GitHub && EF.Functions.Like(x.WorkItem.Repository.Owner, $"%{request.Query}%")) ||
                                (x.Discriminator == Discriminator.GitHub && EF.Functions.Like(x.WorkItem.Repository.Name, $"%{request.Query}%")) ||
                                (x.Discriminator == Discriminator.GitHub && x.WorkItem.Comments.Any(y => EF.Functions.Like(y.Body, $"%{request.Query}%"))));
                        }
                    }

                    // Got a current category?
                    if (request.Category != null)
                    {
                        if (request.Category.Kind == CategoryKind.Filter)
                        {
                            // Just include notifications in the category filter.
                            query = query.Where(request.Category.Filter);
                        }
                        else
                        {
                            // Just include notifications in this category.
                            query = query.Where(n => n.Category.Id == request.Category.Id);
                        }
                    }

                    // Cross state filters.
                    if (request.Filter == NotificationReadFilter.Unread)
                    {
                        query = query.Where(x => x.Unread);
                    }

                    // Sort by timestamp
                    query = query.OrderByDescending(x => x.Timestamp);

                    // Get the requested page.
                    query = query.Skip(request.Index).Take(request.Size);

                    // Map the results from the query.
                    result.AddRange(NotificationMapper.Map(query, _localizer));
                }
            }

            return result;
        }
    }
}

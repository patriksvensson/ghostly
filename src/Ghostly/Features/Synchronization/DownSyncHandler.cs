using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using MediatR;

namespace Ghostly.Features.Synchronization
{
    public sealed class DownSyncHandler : GhostlyRequestHandler<DownSyncHandler.Request>
    {
        private readonly IReadOnlyList<IVendor> _vendors;
        private readonly IMediator _mediator;
        private readonly IGhostlyLog _log;

        public DownSyncHandler(
            IEnumerable<IVendor> vendors,
            IMediator mediator,
            IGhostlyLog log)
        {
            _vendors = new List<IVendor>(vendors);
            _mediator = mediator;
            _log = log;
        }

        public sealed class Request : IRequest
        {
            public IReadOnlyList<Account> Accounts { get; }
            public Notification Notification { get; }

            public Request(IReadOnlyList<Account> accounts, Notification notification = null)
            {
                Accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
                Notification = notification;
            }
        }

        public override async Task Handle(Request request, CancellationToken cancellationToken)
        {
            // Nothing to sync?
            var mappings = GetAccountVendorMappings(request.Accounts);
            if (mappings.Count == 0)
            {
                await Task.Delay(2500, cancellationToken);
                return;
            }

            foreach (var (account, vendor) in mappings)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SynchronizationStatus result = await Synchronize(request, account, vendor);
                if (result != SynchronizationStatus.Completed)
                {
                    _log.Warning("Synchronization failed. Error={SynchronizationResult}", result);

                    if (result == SynchronizationStatus.AuthenticationFailed ||
                        result == SynchronizationStatus.RequiresAuthentication)
                    {
                        // Update the account state to unauthorized.
                        await _mediator.UpdateAccountState(account.Id, AccountState.Unauthorized);
                    }

                    continue;
                }

                // Update sync time.
                await UpdateLastSyncTime(account, DateTime.UtcNow);
            }
        }

        private static async Task<SynchronizationStatus> Synchronize(Request request, Account account, IVendor vendor)
        {
            if (request.Notification != null)
            {
                return await vendor.Synchronize(account, request.Notification);
            }
            else
            {
                return await vendor.Synchronize(account);
            }
        }

        private List<(Account Account, IVendor Vendor)> GetAccountVendorMappings(IReadOnlyList<Account> accounts)
        {
            // Build the account and vendor mapping.
            var mappings = new List<(Account Account, IVendor Vendor)>();
            foreach (var account in accounts)
            {
                if (account.State != AccountState.Active)
                {
                    continue;
                }

                var vendor = _vendors.FirstOrDefault(s => s.Matches(account.VendorKind));
                if (vendor != null)
                {
                    if (vendor.CanSynchronize(account))
                    {
                        mappings.Add((account, vendor));
                    }
                }
            }

            return mappings;
        }

        private async Task UpdateLastSyncTime(Account model, DateTime syncTime)
        {
            using (var context = new GhostlyContext())
            {
                var account = await context.Accounts.FindAsync(model.Id);
                if (account != null)
                {
                    account.LastSyncedAt = syncTime;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Ghostly.Core.Services;
using Ghostly.Data;
using Ghostly.Data.Models;
using Ghostly.Domain;
using Ghostly.Domain.Messages;
using MediatR;

namespace Ghostly.Services
{
    public interface ISynchronizationService
    {
        bool IsSynchronizing { get; }
        bool CanSynchronize { get; }

        Task<bool> Trigger(CancellationToken cancellationToken = default);
        Task<bool> Trigger(Notification notification, CancellationToken cancellationToken = default);
    }

    [DependentOn(typeof(DatabaseInitializer))]
    [DependentOn(typeof(INetworkHelper))]
    public sealed class SynchronizationService : ISynchronizationService, IInitializable, IDisposable
    {
        private readonly IMediator _mediator;
        private readonly INetworkHelper _network;
        private readonly IMessageService _messenger;
        private readonly ILocalSettings _settings;
        private readonly ILocalizer _localizer;
        private readonly IGhostlyLog _log;
        private readonly ITelemetry _telemetry;
        private readonly ManualResetEvent _isSynchronizing;
        private readonly SemaphoreSlim _semaphore;
        private bool _warnedAboutMeteredConnection;

        public bool IsSynchronizing => _isSynchronizing.WaitOne(0);
        public bool CanSynchronize { get; private set; }

        public bool IsConnected { get; private set; }
        public bool MeteredConnectionNotAllowed { get; private set; }
        public bool HasAccounts { get; private set; }

        public SynchronizationService(
            IMediator mediator,
            INetworkHelper network,
            IMessageService messenger,
            ILocalSettings settings,
            ILocalizer localizer,
            IGhostlyLog log,
            ITelemetry telemetry)
        {
            _isSynchronizing = new ManualResetEvent(false);
            _mediator = mediator;
            _network = network;
            _messenger = messenger;
            _settings = settings;
            _localizer = localizer;
            _log = log;
            _telemetry = telemetry;
            _semaphore = new SemaphoreSlim(1, 1);
            _warnedAboutMeteredConnection = false;

            _messenger.SubscribeOnUIThreadAsync<NetworkConnectivityChanged>(_ => UpdateSynchronizationAvailability());
            _messenger.SubscribeOnUIThreadAsync<AccountRemoved>(_ => UpdateSynchronizationAvailability());
            _messenger.SubscribeOnUIThreadAsync<AccountUpdated>(_ => UpdateSynchronizationAvailability());
            _messenger.SubscribeOnUIThreadAsync<UpdateSynchronizationState>(_ => UpdateSynchronizationAvailability());
            _messenger.SubscribeOnUIThreadAsync<RefreshSynchronizationAvailability>(_ => UpdateSynchronizationAvailability());
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            _isSynchronizing.Dispose();
        }

        public async Task<bool> Initialize(bool background)
        {
            await UpdateSynchronizationAvailability();
            return true;
        }

        public async Task<bool> Trigger(CancellationToken cancellationToken = default)
        {
            // Synchronization in progress?
            if (IsSynchronizing)
            {
                _log.Debug("Another synchronization is in progress. Aborting.");
                return false;
            }

            // Can't synchronize? This is most likely due to a metered connection.
            if (!CanSynchronize)
            {
                if (MeteredConnectionNotAllowed)
                {
                    _log.Debug("Cannot synchronize since we're running on a metered connection. Aborting.");
                }
                else if (!IsConnected)
                {
                    _log.Debug("Cannot synchronize since there is no internet connection. Aborting.");
                }
                else if (!HasAccounts)
                {
                    _log.Debug("Cannot synchronize since there are no accounts to synchronize. Aborting.");
                }
                else
                {
                    _log.Debug("Cannot synchronize because of unknown reasons. Aborting.");
                }

                return false;
            }

            // Aquire the sync semaphore.
            _log.Verbose("Aquiring the sync semaphore...");
            if (!_semaphore.Wait(0, cancellationToken))
            {
                _log.Warning("The sync semaphore could not be aquired. Aborting.");
                return false;
            }

            try
            {
                // No available accounts to synchronize?
                var accounts = (await _mediator.GetAccounts()).Where(a => a.State == AccountState.Active).ToList();
                if (accounts.Count == 0)
                {
                    _log.Warning("Could not find an account to synchronize. Aborting.");
                    return false;
                }

                // Perform synchronization.
                return await PerformSynchronization(accounts);
            }
            finally
            {
                // Release the sync semaphore.
                _log.Verbose("Releasing the sync semaphore...");
                _semaphore.Release();
                _log.Verbose("The sync semaphore was released.");
            }
        }

        public async Task<bool> Trigger(Notification notification, CancellationToken cancellationToken = default)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            // No available accounts to synchronize?
            var account = (await _mediator.GetAccounts()).SingleOrDefault(a => a.Id == notification.AccountId);
            if (account == null)
            {
                _log.Warning("Cannot sync single notification since since account #{AccountId} could not be found.", notification.Id);
                return false;
            }

            if (account.State != AccountState.Active)
            {
                _log.Warning("Cannot sync single notification since since account #{AccountId} is not active.", notification.Id);
                return false;
            }

            return await PerformSynchronization(account, notification);
        }

        private async Task<bool> PerformSynchronization(Account account, Notification notification)
        {
            var started = DateTime.UtcNow;

            using (_log.Push("SynchronizationId", Guid.NewGuid()))
            {
                try
                {
                    // Synchronization started.
                    _isSynchronizing.Set();
                    await _messenger.PublishAsync(new SynchronizationStateChanged(true, true));
                    await _messenger.PublishAsync(new StatusMessage(_localizer["Synchronization_Started"], null));
                    _log.Debug("Synchronizing...");

                    // Prevent synchronization on other threads.
                    await UpdateSynchronizationAvailability();

                    // Perform synchronization.
                    await _mediator.PerformDownSync(account, notification);

                    // Synchronization completed.
                    return true;
                }
                catch (Exception ex)
                {
                    _telemetry.TrackException(ex, nameof(SynchronizationService));
                    _log.Error(ex);
                    return false;
                }
                finally
                {
                    // Synchronization complete.
                    _isSynchronizing.Reset();
                    await _messenger.PublishAsync(new SynchronizationStateChanged(false, true));
                    _log.Debug("Synchronization finished.");

                    // Clear status.
                    await _messenger.PublishAsync(new StatusMessage(string.Empty, null));

                    // We can now synchronize again.
                    await UpdateSynchronizationAvailability();
                }
            }
        }

        private async Task<bool> PerformSynchronization(List<Account> accounts)
        {
            var started = DateTime.UtcNow;

            using (_log.Push("SynchronizationId", Guid.NewGuid()))
            {
                try
                {
                    // Synchronization started.
                    _isSynchronizing.Set();
                    await _messenger.PublishAsync(new SynchronizationStateChanged(true, false));
                    await _messenger.PublishAsync(new StatusMessage(_localizer["Synchronization_Started"], null));
                    _log.Debug("Synchronizing...");

                    // Prevent synchronization on other threads.
                    await UpdateSynchronizationAvailability();

                    // Perform synchronization.
                    await _mediator.PerformUpSync();
                    await _mediator.PerformDownSync(accounts);

                    // Did this go super fast?
                    if ((DateTime.UtcNow - started) < TimeSpan.FromSeconds(2))
                    {
                        await Task.Delay(1500);
                    }

                    // Synchronization completed.
                    return true;
                }
                catch (Exception ex)
                {
                    _telemetry.TrackException(ex, nameof(SynchronizationService));
                    _log.Error(ex);
                    return false;
                }
                finally
                {
                    // Synchronization complete.
                    _isSynchronizing.Reset();
                    await _messenger.PublishAsync(new SynchronizationStateChanged(false, false));
                    _log.Debug("Synchronization finished.");

                    // Clear status.
                    await _messenger.PublishAsync(new StatusMessage(string.Empty, null));

                    // We can now synchronize again.
                    await UpdateSynchronizationAvailability();
                }
            }
        }

        private async Task UpdateSynchronizationAvailability()
        {
            var oldState = CanSynchronize;

            var accounts = await _mediator.GetAccounts();
            var hasAccounts = accounts.Where(a => a.State != AccountState.Deleted).Any();

            var isConnected = _network.IsConnected;
            var meteredConnectionNotAllowed = !_settings.GetAllowMeteredConnection() && _network.IsMetered;
            if (meteredConnectionNotAllowed)
            {
                isConnected = false;
            }

            HasAccounts = hasAccounts;
            IsConnected = isConnected;
            MeteredConnectionNotAllowed = meteredConnectionNotAllowed;
            CanSynchronize = IsConnected && HasAccounts && !IsSynchronizing;

            if (CanSynchronize != oldState)
            {
                // State changed so notify subscribers.
                await _messenger.PublishAsync(new SynchronizationAvailabilityChanged(CanSynchronize));
            }

            // Warn about metered connection?
            if (_network.IsMetered && !_settings.GetAllowMeteredConnection() && !_warnedAboutMeteredConnection)
            {
                _warnedAboutMeteredConnection = true;
                await _messenger.PublishAsync(new InAppNotification(
                    _localizer["Synchronization_DisabledBecauseOfMeteredConnection"]));
            }
        }
    }
}

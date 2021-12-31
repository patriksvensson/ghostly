using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;

namespace Ghostly.Core.Services
{
    public interface IMessageService
    {
        void Subscribe<T>(Func<T, Task> handler, Func<Func<Task>, Task> marshal);
        Task Publish<T>(T message, Func<Func<Task>, Task> marshal);
    }

    public sealed class MessageService : IMessageService, IDisposable
    {
        private readonly List<IMessageSubscription> _subscriptions;
        private readonly SemaphoreSlim _semaphore;
        private readonly IGhostlyLog _log;

        public MessageService(IGhostlyLog log)
        {
            _subscriptions = new List<IMessageSubscription>();
            _semaphore = new SemaphoreSlim(1, 1);
            _log = log;
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public void Subscribe<T>(Func<T, Task> handler, Func<Func<Task>, Task> marshal)
        {
            if (!_subscriptions.Any(s => s.Matches(handler)))
            {
                var subscription = new MessageSubscription<T>(handler, typeof(T), marshal);
                _subscriptions.Add(subscription);
            }
        }

        public async Task Publish<T>(T message, Func<Func<Task>, Task> marshal)
        {
            if (marshal is null)
            {
                throw new ArgumentNullException(nameof(marshal));
            }

            if (GhostlyState.IsBackgroundActivated && !GhostlyState.InForeground)
            {
                _log.Verbose("Did not send message since we're background activated: {@Message}", message);
                return;
            }
            else
            {
                _log.Verbose("Sending message: {@Message}", message);
                await marshal(async () =>
                {
                    // Let subscriptions handle messages.
                    var subscriptions = _subscriptions
                        .Where(x => x.CanHandle(typeof(T)))
                        .ToList()
                        .Select(x => x.Handle(message))
                        .ToArray();

                    await Task.WhenAll(subscriptions);
                });
            }
        }
    }

    internal interface IMessageSubscription
    {
        bool IsDead { get; }

        bool Matches(object target);
        bool CanHandle(Type messageType);
        Task Handle(object message);
    }

    internal sealed class MessageSubscription<T> : IMessageSubscription
    {
        private readonly Func<T, Task> _reference;
        private readonly Type _messageType;
        private readonly Func<Func<Task>, Task> _marshal;

        public bool IsDead => _reference == null;

        public MessageSubscription(Func<T, Task> reference, Type messageType, Func<Func<Task>, Task> marshal)
        {
            _reference = reference;
            _messageType = messageType;
            _marshal = marshal;
        }

        public bool Matches(object target)
        {
            return ReferenceEquals(_reference, target);
        }

        public bool CanHandle(Type messageType)
        {
            return _messageType.IsAssignableFrom(messageType);
        }

        public Task Handle(object message)
        {
            return _marshal(() =>
            {
                return _marshal(() => _reference.Invoke((T)message));
            });
        }
    }
}

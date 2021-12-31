using System;
using System.Threading.Tasks;
using Ghostly.Core;
using Ghostly.Core.Services;

namespace Ghostly
{
    public static class MessageServiceExtensions
    {
        public static void SubscribeAsync<T>(this IMessageService service, Func<T, Task> handler)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            service.Subscribe(handler, marshal =>
            {
                return marshal();
            });
        }

        public static void Subscribe<T>(this IMessageService service, Action<T> handler)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            service.Subscribe<T>(
                arg =>
                {
                    handler(arg);
                    return Task.CompletedTask;
                },
                marshal =>
                {
                    return marshal();
                });
        }

        public static void SubscribeOnUIThreadAsync<T>(this IMessageService service, Func<T, Task> handler)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            service.Subscribe(handler, marshal =>
            {
                return Platform.ThreadingModel.ExecuteOnUIThread(() => marshal());
            });
        }

        public static void SubscribeOnUIThread<T>(this IMessageService service, Action<T> handler)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            service.Subscribe<T>(
                arg =>
                {
                    handler(arg);
                    return Task.CompletedTask;
                },
                marshal =>
                {
                    return Platform.ThreadingModel.ExecuteOnUIThread(() => marshal());
                });
        }

        public static void Publish<T>(this IMessageService service, T message)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            service.Publish(message, marshal => marshal()).FireAndForgetSafeAsync();
        }

        public static Task PublishAsync<T>(this IMessageService service, T message)
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return service.Publish(message, marshal => marshal());
        }
    }
}

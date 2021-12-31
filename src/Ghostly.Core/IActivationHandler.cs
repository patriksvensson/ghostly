using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ghostly.Core
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);
        Task Handle(object args);
    }

    public abstract class SyncActivationHandler<T> : IActivationHandler
    {
        public virtual bool CanHandle(T args)
        {
            return true;
        }

        public abstract void Handle(T args);

        bool IActivationHandler.CanHandle(object args)
        {
            return args is T typed && CanHandle(typed);
        }

        Task IActivationHandler.Handle(object args)
        {
            if (!(args is T typed))
            {
                throw new InvalidOperationException("Unknown argument type");
            }

            Handle(typed);
            return Task.CompletedTask;
        }
    }

    public abstract class MultipleActivationHandler : IActivationHandler
    {
        private readonly HashSet<Type> _types;

        protected MultipleActivationHandler(params Type[] types)
        {
            _types = new HashSet<Type>(types ?? Enumerable.Empty<Type>());
        }

        bool IActivationHandler.CanHandle(object args)
        {
            if (args == null || !_types.Contains(args.GetType()))
            {
                return false;
            }

            return CanHandle(args);
        }

        Task IActivationHandler.Handle(object args)
        {
            return Handle(args);
        }

        protected abstract bool CanHandle(object args);
        protected abstract Task Handle(object args);
    }

    public abstract class ActivationHandler<T> : IActivationHandler
    {
        public virtual bool CanHandle(T args)
        {
            return true;
        }

        public abstract Task Handle(T args);

        bool IActivationHandler.CanHandle(object args)
        {
            return args is T typed && CanHandle(typed);
        }

        Task IActivationHandler.Handle(object args)
        {
            if (!(args is T typed))
            {
                throw new InvalidOperationException("Unknown argument type");
            }

            return Handle(typed);
        }
    }
}

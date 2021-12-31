using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ghostly.Features
{
    public abstract class GhostlyRequestHandler<T> : IRequestHandler<T>
        where T : IRequest
    {
        async Task<Unit> IRequestHandler<T, Unit>.Handle(T request, CancellationToken cancellationToken)
        {
            await Handle(request, cancellationToken);
            return Unit.Value;
        }

        public abstract Task Handle(T request, CancellationToken cancellationToken);
    }

    public abstract class GhostlyRequestHandler<TRequest, TResult> : IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        public abstract Task<TResult> Handle(TRequest request, CancellationToken cancellationToken);
    }
}

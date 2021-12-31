using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Ghostly.GitHub
{
    internal abstract class GitHubRequestHandler<TRequest> : IRequestHandler<TRequest, GitHubResult>
        where TRequest : IRequest<GitHubResult>
    {
        public async Task<GitHubResult> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleRequest(request, cancellationToken);
            }
            catch (Exception ex)
            {
                return GitHubResult.Err(ex);
            }
        }

        protected abstract Task<GitHubResult> HandleRequest(TRequest request, CancellationToken cancellationToken);
    }

    internal abstract class GitHubRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, GitHubResult<TResponse>>
        where TRequest : IRequest<GitHubResult<TResponse>>
    {
        public async Task<GitHubResult<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await HandleRequest(request, cancellationToken);
            }
            catch (Exception ex)
            {
                return GitHubResult.Err<TResponse>(ex);
            }
        }

        protected abstract Task<GitHubResult<TResponse>> HandleRequest(TRequest request, CancellationToken cancellationToken);
    }
}

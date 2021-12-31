using System;
using System.Threading;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Features.Activities.Payloads;
using Ghostly.Jobs;
using MediatR;

namespace Ghostly.GitHub.Actions
{
    internal sealed class DownloadGitHubAvatar : GitHubRequestHandler<DownloadGitHubAvatar.Request>
    {
        private readonly IFileDownloader _downloader;
        private readonly IActivityQueue _queue;
        private readonly ILocalSettings _settings;

        public sealed class Request : IRequest<GitHubResult>
        {
            public Uri Url { get; }
            public string Hash { get; }
            public bool Deferred { get; }

            public Request(Uri url, string hash, bool deferred)
            {
                Url = url;
                Hash = hash;
                Deferred = deferred;
            }
        }

        public DownloadGitHubAvatar(
            IFileDownloader downloader,
            IActivityQueue queue,
            ILocalSettings settings)
        {
            _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            _queue = queue;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        protected override async Task<GitHubResult> HandleRequest(Request request, CancellationToken cancellationToken)
        {
            if (!_settings.GetSynchronizeAvatars())
            {
                return GitHubResult.Ok();
            }

            // Adjust the URL and add the size.
            var builder = new UriBuilder(request.Url)
            {
                Query = "s=256",
            };

            if (request.Deferred)
            {
                // Queue the file for downloading.
                _queue.Add(new DownloadFilePayload
                {
                    Url = builder.Uri.ToString(),
                    Path = "Images/Avatars",
                    Filename = $"{request.Hash}.png",
                });
            }
            else
            {
                // Download the file.
                await _downloader.Download(builder.Uri, "Images/Avatars", $"{request.Hash}.png");
            }

            return GitHubResult.Ok();
        }
    }
}

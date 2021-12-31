using System;
using System.Threading.Tasks;
using Ghostly.Core.Pal;
using Ghostly.Data.Models;
using Ghostly.Features.Activities.Payloads;

namespace Ghostly.Features.Activities.Processors
{
    public sealed class DownloadFilePayloadProcessor : ActivityPayloadProcessor<DownloadFilePayload>
    {
        private readonly INetworkHelper _network;
        private readonly IFileDownloader _downloader;

        public override ActivityKind Kind => ActivityKind.DownloadFile;

        public DownloadFilePayloadProcessor(
            INetworkHelper network,
            IFileDownloader downloader)
        {
            _network = network;
            _downloader = downloader;
        }

        protected override async Task<bool> Process(DownloadFilePayload activity)
        {
            // TODO: Respect metered connection.
            if (_network.IsConnected)
            {
                await _downloader.Download(new Uri(activity.Url), activity.Path, activity.Filename);
                return true;
            }

            return false;
        }
    }
}

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Ghostly.Core.Diagnostics;
using Ghostly.Core.Pal;
using Windows.Storage;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpFileDownloader : IFileDownloader, IDisposable
    {
        private readonly HttpClient _client;
        private readonly IGhostlyLog _log;

        public UwpFileDownloader(IGhostlyLog log)
        {
            _client = new HttpClient();
            _log = log;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task Download(Uri uri, string path, string filename)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            path = Path.Combine(ApplicationData.Current.LocalFolder.Path, path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Check if the file exist.
            filename = Path.Combine(path, filename);
            if (File.Exists(filename))
            {
                return;
            }

            // Download the file.
            _log.Debug("Downloading file {0}...", uri.AbsoluteUri);
            var stream = await _client.GetStreamAsync(uri);

            // Save the file.
            using (var output = File.OpenWrite(filename))
            {
                await stream.CopyToAsync(output);
            }
        }
    }
}

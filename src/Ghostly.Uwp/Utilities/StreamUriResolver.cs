using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace Ghostly.Uwp.Utilities
{
    public sealed class StreamUriResolver : IUriToStreamResolver
    {
        public static StreamUriResolver Instance { get; } = new StreamUriResolver();
        public string Html { get; set; }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return GetContent(uri).AsAsyncOperation();
        }

        private async Task<IInputStream> GetContent(Uri uri)
        {
            try
            {
                if (uri.AbsolutePath == "/html")
                {
                    return await Html.ToInputStreamAsync();
                }

                var path = GetLocalPath(uri.AbsolutePath);
                if (path != null)
                {
                    // We found the file.
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    return (await file.OpenAsync(FileAccessMode.Read)).GetInputStreamAt(0);
                }
                else
                {
                    // Fallback to default file.
                    var localUri = new Uri("ms-appx:///Assets/Images/Avatar.png");
                    var file = await StorageFile.GetFileFromApplicationUriAsync(localUri);
                    return (await file.OpenAsync(FileAccessMode.Read)).GetInputStreamAt(0);
                }
            }
            catch (Exception)
            {
                // Do nothing. We don't want this to fail.
            }

            return null;
        }

        private static string GetLocalPath(string path)
        {
            var directory = Path.GetDirectoryName(path).TrimStart('\\').TrimStart('/');
            directory = Path.Combine(ApplicationData.Current.LocalFolder.Path, directory);
            if (Directory.Exists(directory))
            {
                var filename = Path.Combine(directory, Path.GetFileName(path));
                if (File.Exists(filename))
                {
                    return filename;
                }
            }

            return null;
        }
    }
}

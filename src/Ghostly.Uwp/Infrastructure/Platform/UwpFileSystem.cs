using System.IO;
using Ghostly.Core.Pal;
using Windows.Storage;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpFileSystem : IFileSystem
    {
        public bool FileExist(string path)
        {
            return File.Exists($"{ApplicationData.Current.LocalFolder.Path}\\{path}");
        }
    }
}

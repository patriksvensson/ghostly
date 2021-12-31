using System;
using System.Threading.Tasks;

namespace Ghostly.Core.Pal
{
    public interface IFileDownloader
    {
        Task Download(Uri uri, string path, string filename);
    }
}

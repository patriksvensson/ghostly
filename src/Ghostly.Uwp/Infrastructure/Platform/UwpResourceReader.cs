using System.IO;
using Ghostly.Core.Pal;

namespace Ghostly.Uwp.Infrastructure.Pal
{
    public sealed class UwpResourceReader : IResourceReader
    {
        public Stream Read(string path)
        {
            var assembly = GetType().Assembly;
            return assembly.GetManifestResourceStream(path);
        }
    }
}

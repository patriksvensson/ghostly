using System.IO;

namespace Ghostly.Core.Pal
{
    public interface IResourceReader
    {
        Stream Read(string path);
    }
}
